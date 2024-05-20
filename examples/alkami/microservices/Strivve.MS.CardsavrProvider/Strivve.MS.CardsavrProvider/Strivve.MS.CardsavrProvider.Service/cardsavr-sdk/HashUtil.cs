using System;
using System.Security.Cryptography;
using System.Text;


namespace Switch.Security
{
    internal static class HashUtil
    {
        public static string HmacSign(string input, string b64Key, bool isInput64 = false)
        {
            byte[] key = Convert.FromBase64String(b64Key);
            byte[] toSign = (isInput64 ?
                                Convert.FromBase64String(input) :
                                Encoding.UTF8.GetBytes(input));

            using (HMACSHA256 hmac = new HMACSHA256(key))
            {
                byte[] hash = hmac.ComputeHash(toSign);
                return Convert.ToBase64String(hash);
            }
        }

        public static bool HmacVerify(string input, string b64Key, string b64VerificationSignature)
        {
            return HmacSign(input, b64Key) == b64VerificationSignature;
        }

        public static byte[] Sha256Hash(string input)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] toHash = Encoding.UTF8.GetBytes(input);
                return sha.ComputeHash(toHash);
            }
        }

        public static byte[] Sha256Pbkdf2(
            string password, byte[] salt, int iterations, int keyLen = 32)
        {
            using (Rfc2898DeriveBytes db = new Rfc2898DeriveBytes(
                password, salt, iterations, HashAlgorithmName.SHA256))
            {
                // generate and return the password key.
                return db.GetBytes(keyLen);
            }
        }
    }
}
