using System;
using System.Text;
using System.Security.Cryptography;


namespace Switch.Security.Cryptography
{
    /// <summary>
    /// NOTE: this class is currently incomplete, unused, and untested. The .NET classes it 
    /// uses are Windows-only, so a Windows machine is needed.
    /// 
    /// This is an incomplete implementation based on "next generation" cryptography support 
    /// (.NET classes with a "Cng" suffix). This code is NOT cross-platform. Notably it will 
    /// not work on MacOS, and likely has other limitations as well. 
    /// 
    /// Note that this code likely has issues with the format of keys that it would exchange
    /// with a Node-based server. See the EcdhBC class implementation for details.
    ///
    /// This code **might** be a better choice than EcdhBC when running on a Windows platform. 
    /// If so, the Ecdh base class Create() method provides a way to create a platform-specific 
    /// class instance once this implementation is completed.
    /// 
    /// Possibly of interest:
    /// https://github.com/dotnet/corefx/issues/8158
    /// </summary>
    public sealed class EcdhCng : Ecdh
    {
        // ECDH "next generation" algorithm implementation representing a key pair.
        private ECDiffieHellmanCng _ecdh;

        public EcdhCng()
            : base()
        {
            // according to the following reference (and others), "nistP256" is
            // equivalent to "prime256v1" (the latter is used in the Javascript code).
            // https://github.com/nodejs/node/issues/1495
            _ecdh = new ECDiffieHellmanCng(ECCurve.NamedCurves.nistP256)
            {
                KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash,
                HashAlgorithm = CngAlgorithm.Sha256
            };

            // pre-compute and store the public key as a base64-encoded string.
            PublicKey = Convert.ToBase64String(_ecdh.PublicKey.ToByteArray());
        }

        /// <summary>
        /// Initializes an instance of the this class to be used for testing purposes. This 
        /// constructor allows initialization using a known key pair so that we can verify 
        /// compatability with other systems.
        /// </summary>
        /// <param name="privateBlob">Private BLOB.</param>
        public EcdhCng(byte[] privateBlob)
        {
            // the blob is assumed to contain both the private and public key portions.
            CngKey key = CngKey.Import(privateBlob, CngKeyBlobFormat.EccPrivateBlob);
            _ecdh = new ECDiffieHellmanCng(key)
            {
                KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash,
                HashAlgorithm = CngAlgorithm.Sha256
            };

            // pre-compute and store the public key as a base64-encoded string.
            PublicKey = Convert.ToBase64String(_ecdh.PublicKey.ToByteArray());
        }

        public override string ComputeSharedSecret(string otherKey, bool isBase64 = true)
        {
            byte[] blob = (isBase64 ?
                                Convert.FromBase64String(otherKey) :
                                Encoding.UTF8.GetBytes(otherKey));

            /// compute the secret key.
            CngKey key = CngKey.Import(blob, CngKeyBlobFormat.EccPublicBlob);
            byte[] secret = _ecdh.DeriveKeyMaterial(key);

            // store the secret for later retrieval if needed.
            SharedSecret = Convert.ToBase64String(secret);
            return SharedSecret;
        }

        /// <summary>
        /// For testing purposes only. Returns a byte array containing the private and
        /// public keys of the object. The byte array can be later passed to the constructor
        /// to create the class with the same keys. 
        /// 
        /// Note that this BLOB can ONLY be used to reconstruct an EcdhCng object. It will
        /// not work for an EcdhCcg object.
        /// </summary>
        /// <returns>The BLOB.</returns>
        public byte[] GetBlob()
        {
            // for test purposes only.
            return _ecdh.Key.Export(CngKeyBlobFormat.EccPrivateBlob);
        }

        protected override void Dispose(bool disposing)
        {
            if (_ecdh != null && disposing)
            {
                _ecdh.Dispose();
                _ecdh = null;
            }
            base.Dispose(disposing);
        }
    }
}
