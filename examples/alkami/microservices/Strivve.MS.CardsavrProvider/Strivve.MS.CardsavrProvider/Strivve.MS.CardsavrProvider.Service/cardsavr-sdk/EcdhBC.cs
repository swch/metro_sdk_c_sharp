using System;
using System.Text;

using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Agreement;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;


namespace Switch.Security.Cryptography
{
    /// <summary>
    /// Multi-platform Ecdh implementation using the Bouncy Castle (BC) library. 
    /// Tested only on MacOS so far, needs testing on Windows.
    /// 
    /// BC was originally written for Java and then ported to C#. The APIs are similar but
    /// not identical, and some of the differences are significant. There's online documentation 
    /// for BC in Java, but its non-existant for C#. Java programming examples can be  found
    /// on StackOverflow, StackExchange, etc. The best way to figure something out seems to be
    /// to search the Java docs or find an online example to get an idea of that classess needed,
    /// then search through the C# source code and unit tests to find examples and figure out
    /// namespaces, classes, and method signatures.
    /// 
    /// Some references that might be useful:
    /// https://en.wikipedia.org/wiki/Elliptic-curve_Diffie%E2%80%93Hellman
    /// https://stackoverflow.com/questions/15430784/
    /// https://stackoverflow.com/questions/30945722/
    /// https://stackoverflow.com/questions/52232996/
    /// https://lapo.it/asn1js/
    /// </summary>
    public sealed class EcdhBC : Ecdh
    {
        private AsymmetricCipherKeyPair _keyPair;

        /// <summary>
        /// 
        /// </summary>
        public EcdhBC()
            : base()
        {
            // setup.
            X9ECParameters x9 = ECNamedCurveTable.GetByName("prime256v1");
            ECDomainParameters ecSpec = new ECDomainParameters(x9.Curve, x9.G, x9.N, x9.H);
            IAsymmetricCipherKeyPairGenerator kpGen = new ECKeyPairGenerator("ECDH");

            // generate and save the key pair.
            kpGen.Init(new ECKeyGenerationParameters(ecSpec, new SecureRandom()));
            _keyPair = kpGen.GenerateKeyPair();

            // generate and store the public key as a base64 string. The string must be in the
            // basic X9.62 uncompressed format -- just a point on the curve -- which is expected 
            // server-side. Most of the Bouncy Castle methods (e.g., GetEncoded, GetDerEncoded) 
            // will give you a full ASN.1/DER encoded public key which won't work for us.
            //
            // the point Q represents the public key by convention, e.g., as per here:
            // https://en.wikipedia.org/wiki/Elliptic-curve_Diffie%E2%80%93Hellman
            byte[] value = ((ECPublicKeyParameters)_keyPair.Public).Q.GetEncoded();
            PublicKey = Convert.ToBase64String(value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="privateBlob"></param>
        public EcdhBC(byte[] privateBlob)
            : base()
        {
            // break apart the blob into its constituent parts.
            int privLen = BitConverter.ToInt32(privateBlob, 0);
            int pubLen = privateBlob.Length - privLen - sizeof(Int32);
            byte[] priv = new byte[privLen];
            byte[] pub = new byte[pubLen];
            Array.Copy(privateBlob, sizeof(Int32), priv, 0, privLen);
            Array.Copy(privateBlob, sizeof(Int32) + privLen, pub, 0, pubLen);

            // load the key pair.
            _keyPair = new AsymmetricCipherKeyPair(
                PublicKeyFactory.CreateKey(pub),
                PrivateKeyFactory.CreateKey(priv));

            // store public key in base64-encoded X9.62 format.
            byte[] value = ((ECPublicKeyParameters)_keyPair.Public).Q.GetEncoded();
            PublicKey = Convert.ToBase64String(value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="otherKey"></param>
        /// <param name="isBase64"></param>
        /// <returns></returns>
        public override string ComputeSharedSecret(string otherKey, bool isBase64 = true)
        {
            byte[] blob = (isBase64 ? Convert.FromBase64String(otherKey) : Encoding.UTF8.GetBytes(otherKey));

            // the shared secret is the x-coordinate of a point on the EC. the point
            // is calculated by multiplying the remote entity's public key (which itself is
            // a point on the curve) by our private key (an integer value).
            // the following calculation follows the code example here:
            // https://stackoverflow.com/questions/15430784/
            PrivateKeyInfo info = PrivateKeyInfoFactory.CreatePrivateKeyInfo(_keyPair.Private);
            ECPrivateKeyParameters priv = (ECPrivateKeyParameters)PrivateKeyFactory.CreateKey(info);
            ECPoint point = priv.Parameters.Curve.DecodePoint(blob);
            ECPublicKeyParameters pub = new ECPublicKeyParameters(point, priv.Parameters);

            IBasicAgreement agree = new ECDHBasicAgreement();
            agree.Init(priv);
            BigInteger value = agree.CalculateAgreement(pub);
            SharedSecret = Convert.ToBase64String(value.ToByteArrayUnsigned());
            return SharedSecret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public byte[] GetBlob()
        {
            // get private key as byte array, and length of private key as byte array.
            PrivateKeyInfo info = PrivateKeyInfoFactory.CreatePrivateKeyInfo(_keyPair.Private);
            byte[] priv = info.GetDerEncoded();
            byte[] privLen = BitConverter.GetBytes((Int32)priv.Length);

            // get public key as byte array.
            AsymmetricKeyParameter pubKey = _keyPair.Public;
            SubjectPublicKeyInfo spki = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(pubKey);
            byte[] pub = spki.GetDerEncoded();

            // output byte array uses first 4 bytes for the length of the base64 private key,
            // followed by the base64 private key, then the base64 public key.
            // this is a completely homegrown non-standard format; for testing only.
            byte[] result = new byte[privLen.Length + priv.Length + pub.Length];
            privLen.CopyTo(result, 0);
            priv.CopyTo(result, privLen.Length);
            pub.CopyTo(result, privLen.Length + priv.Length);
            return result;
        }
    }
}
