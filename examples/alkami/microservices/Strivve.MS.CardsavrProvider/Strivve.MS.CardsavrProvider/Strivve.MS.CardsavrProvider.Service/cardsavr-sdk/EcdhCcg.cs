using System;
using System.Text;
using System.Security.Cryptography;


namespace Switch.Security.Cryptography
{
    /// <summary>
    /// NOTE: this class is currently incomplete, unused and untested. The .NET classes it uses 
    /// are Windows-only, so a Windows machine is needed.
    /// 
    /// This is an incomplete implementation based on "current generation" cryptography support 
    /// (as opposed to "next generation" support). Like the EcdhCng class, this code is NOT
    /// cross-platform. Notably it will not work on MacOS, and likely has other limitations
    /// as well. 
    /// 
    /// Note that this code likely has issues with the format of keys that it would exchange
    /// with a Node-based server. The EcdhCng (next-generation) class is based on the more
    /// current cryptography classes (which are also Windows-only at this time), so it might
    /// be the case that this class should be discarded in favor of completing the implementation
    /// of the EdchCng class.
    /// </summary>
    public sealed class EcdhCcg : Ecdh
    {
        // ECDH algorithm implementation representing a key pair.
        private ECDiffieHellman _ecdh;

        public EcdhCcg()
        {
            // according to the following reference (and others), "nistP256" is
            // equivalent to "prime256v1" (the latter is used in the Javascript code).
            // https://github.com/nodejs/node/issues/1495
            _ecdh = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);
            _ecdh.GenerateKey(ECCurve.NamedCurves.nistP256);

            // pre-compute and store the public key as a base64-encoded string.
            // The call to ToByteArray() fails on MacOS, apparently because the exported format
            // is Windows-specific. ExportParameters() and ExportExplicitParameters() also fail,
            // rendering ECDiffieHellman nearly useless on MacOS.
            PublicKey = Convert.ToBase64String(_ecdh.PublicKey.ToByteArray());
        }

        public override string ComputeSharedSecret(string otherKey, bool isBase64 = true)
        {
            // https://stackoverflow.com/questions/6665353/
            throw new NotImplementedException();
        }

        public byte[] GetBlob()
        {
            // for test purposes only.
            throw (new NotImplementedException());
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
