using System;


namespace Switch.Security.Cryptography
{
    /// <summary>
    /// Abstract base class from which Ecdh implementations are derived. Callers should
    /// use the Create() static method on this class to create an platform-appropriate 
    /// object instance.
    /// </summary>
    public abstract class Ecdh: IDisposable
    {
        protected bool _disposed;

        /// <summary>
        /// Gets the public key of the key pair as a base64-encoded string. The string
        /// contains an X9.62 point in uncompressed format (not ASN.1/DER encoded).
        /// </summary>
        /// <value>The public key.</value>
        public string PublicKey { get; protected set; }

        /// <summary>
        /// Gets the secret key shared with a remote entity following a key exchange, as a
        /// base64-encoded string. The ComputeSharedSecret() method must have been called
        /// once previously, otherwise this property value will be null.
        /// </summary>
        /// <value>The secret key of the remote entity.</value>
        public string SharedSecret { get; protected set; }

        public static Ecdh Create()
        {
            // clients call this method to create an Ecdh object that's appropriate for
            // the current platform. currently, an EcdhBC is always returned. 
            //
            // once the EcdhCng class has been tested, a try/catch block can attempt to
            // create an EcdhCng, and return an EcdhBC if a PlatformNotSupportedException
            // is caught.
            return new EcdhBC();
        }

        protected Ecdh()
        {
            _disposed = false;
        }

        /// <summary>
        /// Compute the secret key shared with a remote entity following a key exchange. 
        /// The key is stored and can subsequenly be retrieved via the "SharedSecret"
        /// property.
        /// </summary>
        /// <returns>The secret key shared with the remote entity.</returns>
        /// <param name="otherKey">The other entity's public key.</param>
        /// <param name="isBase64">If true, <paramref name="otherKey"/> is base64-encoded.</param>
        public abstract string ComputeSharedSecret(string otherKey, bool isBase64 = true);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // see here for discussion of the IDispose pattern:
            // https://docs.microsoft.com/en-us/dotnet/api/system.idisposable.dispose
            _disposed = true;
        }

        ~Ecdh()
        {
            Dispose(false);
        }
    }
}
