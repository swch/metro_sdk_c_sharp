using System;
using System.IO;
using System.Security.Cryptography;

namespace Switch.Security
{
    /// <summary>
    /// Convenience methods for encryption/decryption via symmetric AES in CBC mode.
    /// This class is intended for internal (assembly-only) use.
    /// 
    /// Most code will use the static methods EncryptText() and DecryptText(). Test
    /// code can instantiate an instance of this class and provide an initialization
    /// vector to achieve repeatable results.
    /// </summary>
    internal sealed class Aes256
    {
        private readonly byte[] _testIv;     // initialization vector; for test purposes only.

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Aes256()
        {
            // under normal (non-test) conditions this value is never used.
            _testIv = null;
        }

        public Aes256(string base64TestIv)
        {
            // this value will be used instead of any IV passed into encrypt, allowing
            // a predictable/repeatable outcome for testing/comparison purposes.
            _testIv = Convert.FromBase64String(base64TestIv);
        }

        /// <summary>
        /// Decrypt the specified base64 cipher-text using the specified base64 key
        /// and initialization vector.
        /// </summary>
        /// <returns>The decrypted text string.</returns>
        /// <param name="b64CipherText">The base64 cipher-text to decrypt.</param>
        /// <param name="b64Iv">The base64 initialization vector.</param>
        /// <param name="b64Key">The base64 key.</param>
        public static string DecryptText(string b64CipherText, string b64Iv, string b64Key)
        {
            Aes256 aes = new Aes256();
            return aes.Decrypt(b64CipherText, b64Iv, b64Key);
        }

        /// <summary>
        /// Encrypt the specified clearText using the specified base64 key. The result is
        /// a base64 string containing the encrypted text, a dollar sign ("$"), and finally
        /// the base64 initialization vector.
        /// </summary>
        /// <returns>The encrypted text and initialization-vector.</returns>
        /// <param name="clearText">The string to encrypt.</param>
        /// <param name="b64Key">The base64 key to use with the symmetric algorithm.</param>
        public static string EncryptText(string clearText, string b64Key)
        {
            Aes256 aes = new Aes256();
            return aes.Encrypt(clearText, b64Key);
        }

        /// <summary>
        /// Most code will use the static method DecryptText().
        /// See the static DecryptText() method for more information.
        /// </summary>
        public string Decrypt(string b64CipherText, string b64Iv, string b64Key)
        {
            byte[] IV = Convert.FromBase64String(b64Iv);
            byte[] key = Convert.FromBase64String(b64Key);
            byte[] rv = Convert.FromBase64String(b64CipherText);
            byte[] cipherText = new byte[rv.Length - 16];
            byte[] tag = new byte[16];
            System.Buffer.BlockCopy(rv, 0, cipherText, 0, cipherText.Length);
            System.Buffer.BlockCopy(rv, cipherText.Length, tag, 0, tag.Length);
            var plainBytes = new byte[cipherText.Length];
            
            var aesGcm = new AesGcm(Convert.FromBase64String(b64Key));
            aesGcm.Decrypt(IV, cipherText, tag, plainBytes);

            return System.Text.Encoding.UTF8.GetString(plainBytes);
        }

        /// <summary>
        /// Most code will use the static method EncryptText().
        /// See the static EncryptText() method for more information.
        /// </summary>
        public string Encrypt(string clearText, string b64Key)
        {
            var aesGcm = new System.Security.Cryptography.AesGcm(Convert.FromBase64String(b64Key));
            var plainBytes = System.Text.Encoding.UTF8.GetBytes(clearText);
            var cipherText = new byte[plainBytes.Length];

            var keygen = new System.Security.Cryptography.Rfc2898DeriveBytes(new DateTime().ToString("r"), 64, 10000);
            byte[] IV = _testIv ?? keygen.GetBytes(AesGcm.NonceByteSizes.MaxSize);

            var tag = new byte[16];
        
            aesGcm.Encrypt(IV, plainBytes, cipherText, tag);
            byte[] rv = new byte[cipherText.Length + tag.Length];

            //public static void BlockCopy (Array src, int srcOffset, Array dst, int dstOffset, int count);
            System.Buffer.BlockCopy(cipherText, 0, rv, 0, cipherText.Length);
            System.Buffer.BlockCopy(tag, 0, rv, cipherText.Length, tag.Length);

            // the base64 encrypted data is suffixed with the IV, separated by "$".
            return Convert.ToBase64String(rv) + "$" + Convert.ToBase64String(IV) + "$aes-256-gcm";

        }

        //return a random 64 encoded string -- set modFour to guarantee it can be decoded to something of use later
        public static string GetRandomString(int len, int originalByteLength = -1) {
            
            byte[] array;
            if (originalByteLength != -1) {
                array = GetRandomBytes(originalByteLength);
            } else {
                array = GetRandomBytes((int)Math.Ceiling(((double)len) * 3 / 4));
            }
            string ret = Convert.ToBase64String(array).Substring(0, len); 
    		if (ret.Length != len) {
                throw new SystemException("Can't create a base64 string legnth " + len + " with base length of " + originalByteLength);
            }
            return ret;
        }

        /// <summary>
        /// Generate a cryptographically strong sequence of random bytes.
        /// </summary>
        /// <returns>The random bytes.</returns>
        /// <param name="len">The desired sequence length.</param>
        public static byte[] GetRandomBytes(int len)
        {
            byte[] bytes = new byte[len];
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
                rng.GetBytes(bytes);
            return bytes;
        }

    }
}
