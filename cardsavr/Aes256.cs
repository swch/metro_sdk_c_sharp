using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

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
        private const int DEFAULT_KEY_BIT_SIZE = 256;
        private const int DEFAULT_MAC_BIT_SIZE = 128;
        private const int DEFAULT_NONCE_BIT_SIZE = 96;
        private readonly int _keySize;
        private readonly int _macSize;
        private readonly int _nonceSize;
        private readonly byte[] _testIv;     // initialization vector; for test purposes only.

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Aes256(string base64TestIv) : this(base64TestIv, DEFAULT_KEY_BIT_SIZE, DEFAULT_MAC_BIT_SIZE, DEFAULT_NONCE_BIT_SIZE)
        {
        }

        public Aes256(string base64TestIv, int keyBitSize, int macBitSize, int nonceBitSize)
        {
            // this value will be used instead of any IV passed into encrypt, allowing
            // a predictable/repeatable outcome for testing/comparison purposes.
            _testIv = base64TestIv != null ? Convert.FromBase64String(base64TestIv) : null;

            _keySize = keyBitSize;
            _macSize = macBitSize;
            _nonceSize = nonceBitSize;        
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
            Aes256 aes = new Aes256(null);
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
            Aes256 aes = new Aes256(null);
            return aes.Encrypt(clearText, b64Key);
        }

        /// <summary>
        /// Most code will use the static method DecryptText().
        /// See the static DecryptText() method for more information.
        /// </summary>
        public string Decrypt(string b64CipherText, string b64Iv, string b64Key)
        {
            if (string.IsNullOrEmpty(b64CipherText))
            {
                throw new ArgumentException("Encrypted Message Required!", "encryptedMessage");
            }

            var decodedKey = Convert.FromBase64String(b64Key);

            var cipherText = Convert.FromBase64String(b64CipherText);

            var nonce = Convert.FromBase64String(b64Iv);

            var plaintext = DecryptWithKey(cipherText, nonce, decodedKey, 0);

            return Encoding.UTF8.GetString(plaintext);
        }

        /// <summary>
        /// Most code will use the static method EncryptText().
        /// See the static EncryptText() method for more information.
        /// </summary>
        public string Encrypt(string clearText, string b64Key)
        {

            if (string.IsNullOrEmpty(clearText))
            {
                throw new ArgumentException("Secret Message Required!", "messageToEncrypt");
            }

            var decodedKey = Convert.FromBase64String(b64Key);

            var plainText = Encoding.UTF8.GetBytes(clearText);
            var gcmEncrypt = EncryptWithKey(plainText, decodedKey, null);

            //log.Warn(Convert.ToBase64String(gcmEncrypt.Item1) + "$" + Convert.ToBase64String(gcmEncrypt.Item2) + "$aes-256-gcm");

            // the base64 encrypted data is suffixed with the IV, separated by "$".
            return Convert.ToBase64String(gcmEncrypt.Item1) + "$" + Convert.ToBase64String(gcmEncrypt.Item2) + "$aes-256-gcm";

        }

        public byte[] DecryptWithKey(byte[] encryptedMessage, byte[] nonce, byte[] key, int nonSecretPayloadLength = 0)
        {
            //User Error Checks
            CheckKey(key);

            if (encryptedMessage == null || encryptedMessage.Length == 0)
            {
                throw new ArgumentException("Encrypted Message Required!", "encryptedMessage");
            }

            using (var cipherStream = new MemoryStream(encryptedMessage))
            using (var cipherReader = new BinaryReader(cipherStream))
            {
                //Grab Payload
                var nonSecretPayload = cipherReader.ReadBytes(nonSecretPayloadLength);

                var cipher = new GcmBlockCipher(new AesEngine());
                var parameters = new AeadParameters(new KeyParameter(key), _macSize, nonce, nonSecretPayload);
                cipher.Init(false, parameters);

                //Decrypt Cipher Text
                var cipherText = cipherReader.ReadBytes(encryptedMessage.Length - nonSecretPayloadLength);
                var plainText = new byte[cipher.GetOutputSize(cipherText.Length)];

                var len = cipher.ProcessBytes(cipherText, 0, cipherText.Length, plainText, 0);
                cipher.DoFinal(plainText, len);

                return plainText;
            }
        }

        public Tuple<byte[], byte[]> EncryptWithKey(byte[] messageToEncrypt, byte[] key, byte[] nonSecretPayload = null)
        {
            //User Error Checks
            CheckKey(key);

            //Non-secret Payload Optional
            nonSecretPayload = nonSecretPayload ?? new byte[] { };

            //Using random nonce large enough not to repeat
            var nonce = _testIv;
            if (_testIv == null) {
                nonce = Aes256.GetRandomBytes(_nonceSize / 8);
            }

            var cipher = new GcmBlockCipher(new AesEngine());
            var parameters = new AeadParameters(new KeyParameter(key), _macSize, nonce, nonSecretPayload);
            cipher.Init(true, parameters);

            //Generate Cipher Text With Auth Tag
            var cipherText = new byte[cipher.GetOutputSize(messageToEncrypt.Length)];
            var len = cipher.ProcessBytes(messageToEncrypt, 0, messageToEncrypt.Length, cipherText, 0);
            cipher.DoFinal(cipherText, len);

            //Assemble Message
            using (var combinedStream = new MemoryStream())
            {
                using (var binaryWriter = new BinaryWriter(combinedStream))
                {
                    //Prepend Authenticated Payload
                    binaryWriter.Write(nonSecretPayload);
                    //Write Cipher Text
                    binaryWriter.Write(cipherText);
                }
                return new Tuple<byte[], byte[]>(combinedStream.ToArray(), nonce);
            }
        }

        private void CheckKey(byte[] key)
        {
            if (key == null || key.Length != _keySize / 8)
            {
                throw new ArgumentException(String.Format("Key needs to be {0} bit! actual:{1}", _keySize, key?.Length * 8), "key");
            }
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
            return RandomNumberGenerator.GetBytes(len);
        }

    }
}
