using System;

using Newtonsoft.Json;
using Xunit;

using Switch.CardSavr.Http;


namespace cardsavr_tests
{
    public class AccountTests
    {
        [Fact]
        public void DefaultAccountPropertyValues()
        {
            // this just verifies that nullable property values are null by default, without explicitly
            // setting them to null in the constructor (as expected).
            Account a = new Account();
            Assert.Null(a.site_hostname);
            Assert.Null(a.username);
        }

        [Fact]
        public void AccountJsonSerialization()
        {
            Account a = new Account()
            {
                id = 12,
                site_hostname = "amazon.com"
            };

            // be sure the serialized string contains ONLY the non-null properties.
            string s = JsonConvert.SerializeObject(a);
            Assert.Contains("id", s);
            Assert.Contains("amazon.com", s);
            Assert.DoesNotContain("show_payment_method", s);
            Assert.DoesNotContain("auto_login", s);
            Assert.DoesNotContain("created_on", s);
        }

        [Fact]
        public void AccountJsonDeserialization()
        {
            string s = "{last_login: \"some-date\", username: \"some-user\"}";
            Account a = JsonConvert.DeserializeObject<Account>(s);

            // be sure the nullable values are null in the deerialized object.
            Assert.Null(a.last_password_update);
            Assert.Null(a.last_saved_card);
            Assert.Null(a.last_updated_on);
        }
    }
}
