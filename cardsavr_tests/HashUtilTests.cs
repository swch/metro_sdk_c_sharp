using System;
using System.Text;
using Xunit;

using Switch.Security;


namespace cardsavr_tests
{
    public class HashUtilTests
    {
        private static readonly string _username = "some-user-dude";
        private static readonly string _staticKey = "VeY9uD0w3zRdrlmIPxnU/l+Vr8w2bbV26E6DxajFu+A=";

        [Fact]
        public void HmacSign()
        {
            // the expected value was generated from the Javascript code.
            string input = "this is a test string";
            string expected = "Ouz/hj6TER8Sf+nI2t8h3PICweZ0Jb2o8c4OySIjIIU=";

            // test normal (not base64 encoded) input.
            string result = HashUtil.HmacSign(input, _staticKey);
            Assert.Equal(expected, result);

            // test base64 encoded input - it should be the same.
            string b64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(input));
            result = HashUtil.HmacSign(b64, _staticKey, true);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void Sha256Hash()
        {
            // the expected value was generated from the Javacript code.
            string expected = "QY6u6uPnXP1AI5TnFA0WQWrxVFm0npXvF95B8kz4COg=";

            string result = Convert.ToBase64String(HashUtil.Sha256Hash("hash-this-string!"));
            Assert.Equal(expected, result);
        }

        [Fact]
        public void Sha256Pbkdf2()
        {
            // the expected value was generated from the Javacript code.
            string expected = "o20Wf5X6aK+BmQIYK/JSUB0rXbmLQNgbJwk9zXpvMJI=";

            byte[] salt = HashUtil.Sha256Hash(_username);
            byte[] result = HashUtil.Sha256Pbkdf2("a-strong-password", salt, 5000);
            Assert.Equal(expected, Convert.ToBase64String(result));
        }
    }
}

