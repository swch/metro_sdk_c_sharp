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
            byte[] iv = Convert.FromBase64String(b64Iv);
            byte[] key = Convert.FromBase64String(b64Key);
            byte[] cipher = Convert.FromBase64String(b64CipherText);

            using (Aes aes = Aes.Create())
            {
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                using (var decryptor = aes.CreateDecryptor(key, iv))
                {
                    using (MemoryStream ms = new MemoryStream(cipher))
                    {
                        using (CryptoStream cs = new CryptoStream(
                            ms, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader reader = new StreamReader(cs))
                            {
                                return reader.ReadToEnd();
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Most code will use the static method EncryptText().
        /// See the static EncryptText() method for more information.
        /// </summary>
        public string Encrypt(string clearText, string b64Key)
        {
            byte[] iv = _testIv ?? GetRandomBytes(16);
            byte[] key = Convert.FromBase64String(b64Key);

            string encrypted;
            using (Aes aes = Aes.Create())
            {
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                using (var encryptor = aes.CreateEncryptor(key, iv))
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(
                            ms, encryptor, CryptoStreamMode.Write))
                        {
                            // a StreamWriter defaults to using UTF8 encoding to write to
                            // the underlying stream.
                            using (StreamWriter writer = new StreamWriter(cs))
                            {
                                writer.Write(clearText);
                                writer.Flush();
                            }

                            // get the encrypted result as a base64 string.
                            encrypted = Convert.ToBase64String(ms.ToArray());
                        }
                    }
                }
            }

            // the base64 encrypted data is suffixed with the IV, separated by "$".
            return encrypted + "$" + Convert.ToBase64String(iv);
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
