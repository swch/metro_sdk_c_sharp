using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Switch.CardSavr.Http;
using Switch.Security;
using System.Linq;
using Xunit;

namespace cardsavr_e2e
{
    /// <summary>
    /// Context of the current test run.
    /// </summary>
    public class Context
    {

        // test env: account information.  Customer Agent can do slightly more than a Cardholder Agent
        public static readonly bool rejectUnauthorized = false;
        
        public static readonly string accountBaseUrl = "https://api.localhost.cardsavr.io:8443";
        public static readonly string accountCustomerAgentAppID = "testing_integrator";
        public static readonly string accountCustomerAgentStaticKey = "OkzxB4HQit43k3OEbSYTAPG5hf96erAAlhb4prAjH/I=";
        public static readonly string accountCustomerAgentUserName = "testing_user";
        public static readonly string accountCustomerAgentPassword = "MasterPassword!";

        public static readonly string accountCardholderAgentAppID = "testing_integrator";
        public static readonly string accountCardholderAgentStaticKey = "OkzxB4HQit43k3OEbSYTAPG5hf96erAAlhb4prAjH/I=";
        public static readonly string accountCardholderAgentUserName = "testing_user";
        public static readonly string accountCardholderAgentPassword = "MasterPassword!";

        /*
        public static readonly string accountBaseUrl = "https://api.mbudos.cardsavr.io";
        public static readonly string accountCustomerAgentAppID = "job_post_agent";
        public static readonly string accountCustomerAgentStaticKey = "/px1olwCWk4q2qq1flOrBmTUZXm6qzWm6Muf4TPQUAw=";
        public static readonly string accountCustomerAgentUserName = "test_customer_agent";
        public static readonly string accountCustomerAgentPassword = "92cM@tYxR@iiR5#K0kj*";

        public static readonly string accountCardholderAgentAppID = "job_post_agent";
        public static readonly string accountCardholderAgentStaticKey = "/px1olwCWk4q2qq1flOrBmTUZXm6qzWm6Muf4TPQUAw=";
        public static readonly string accountCardholderAgentUserName = "test_cardholder_agent";
        public static readonly string accountCardholderAgentPassword = "2syF6cGpqpmZ%8XNk66C";
        */
        /*       
        public static readonly string accountBaseUrl = "https://api.squann.cardsavr.io";
        public static readonly string accountCustomerAgentAppID = "CardUpdatr Demo";
        public static readonly string accountCustomerAgentStaticKey = "PxAnasgdN6K82I4Ra5Tbt0RxEeWhedI1jgd0f7kZd98=";
        public static readonly string accountCustomerAgentUserName = "sdk_tester_cust";
        public static readonly string accountCustomerAgentPassword = "password";
        
        public static readonly string accountCardholderAgentAppID = "CardUpdatr Demo";
        public static readonly string accountCardholderAgentStaticKey = "PxAnasgdN6K82I4Ra5Tbt0RxEeWhedI1jgd0f7kZd98=";
        public static readonly string accountCardholderAgentUserName = "sdk_tester_card";
        public static readonly string accountCardholderAgentPassword = "password";
        */
        /*()
        public static readonly string accountBaseUrl = "https://api.customer-dev.cardsavr.io";
        public static readonly string accountCustomerAgentAppID = "AdvancialDev";
        public static readonly string accountCustomerAgentStaticKey = "WOA1Oy5VOfDO66sdBR/gCuQq/w3vxEUUM04EmHzghF8=";
        public static readonly string accountCustomerAgentUserName = "custUser";
        public static readonly string accountCustomerAgentPassword = "CustUser@2020";

        public static readonly string accountCardholderAgentAppID = "AdvancialDev";
        public static readonly string accountCardholderAgentStaticKey = "WOA1Oy5VOfDO66sdBR/gCuQq/w3vxEUUM04EmHzghF8=";
        public static readonly string accountCardholderAgentUserName = "cardupdatr_ux_user";
        public static readonly string accountCardholderAgentPassword = "EaluI08Jg2iHaV5EoPA+f+4/y211W74DGMpK9v3ukfw=";
        */

        // test env: account information.  Customer Agent can do slightly more than a Cardholder Agent
        /*
        public static readonly string accountBaseUrl = "https://api.customer-dev.cardsavr.io";
        public static readonly string accountCustomerAgentAppID = "CardUpdatr Demo";
        public static readonly string accountCustomerAgentStaticKey = "<redacted>";
        public static readonly string accountCustomerAgentUserName = "<redacted>";
        public static readonly string accountCustomerAgentPassword = "<redacted>";

        public static readonly string accountCardholderAgentAppID = "CardUpdatr Demo";
        public static readonly string accountCardholderAgentStaticKey = "<redacted>";
        public static readonly string accountCardholderAgentUserName = "<redacted>";
        public static readonly string accountCardholderAgentPassword = "<redacted>";
        */
        // other resources.
        public static readonly string e2e_identifier = "c_sharp_e2e";
        public static readonly Random random = new Random();

        // properties.
        public List<MerchantSite> Sites { get; set; }
        public List<User> Users { get; set; }
        public List<User> Cardholders { get; set; }
        public Dictionary<int, CardholderData> CardholderSessions { get; set; }
        public string Trace { get; set; }
        public string ExecutionRole { get; set; }
        public string CardholderSafeKey { get; set; }

        public class CardholderData {
            public List<Card> cards { get; set; }
            public List<Account> accounts { get; set; }
            public String cardholder_safe_key { get; set; }
            public CardSavrHttpClient client { get; set; }
        }

        public static string GenerateBogus32BitPassword(string username)
        {
            byte[] sequence = new byte[32];
            for (byte n = 0; n < sequence.Length; ++n)
                sequence[n] = n;

            string password = Convert.ToBase64String(sequence);
            byte[] salt = Encoding.UTF8.GetBytes(username);
            return Convert.ToBase64String(HashUtil.Sha256Pbkdf2(password, salt, 5000, 32));
        }

        public Context()
        {
            CardholderSessions = new Dictionary<int, Context.CardholderData>();
        }

        public List<User> GetNewUsers(String role = null) {
            return Users.Where(
                user => user.username.StartsWith(Context.e2e_identifier, StringComparison.CurrentCulture) &&
                        (role == null || user.role == role)).ToList();
        }

        public User FindUserById(int userId)
        {
            if (Users == null || Users.Count == 0)
                throw new ArgumentException("user-list not available.");

            return Users.Find(u => u.id == userId);
        }

        public User GetRandomUser(string role = null, bool throwOnError = true)
        {
            List<User> users = GetNewUsers(role);
            if (users == null || users.Count == 0)
            {
                if (!throwOnError)
                    return null;
                throw new ArgumentException("no new users available.");
            }

            return users[random.Next(0, users.Count - 1)];
        }

        public MerchantSite GetSyntheticSite(bool throwOnError = true)
        {
            if (Sites == null || Sites.Count == 0)
            {
                if (!throwOnError)
                    return null;
                throw new ArgumentException("no merchant sites available.");
            }
            return Sites.Find(site => site.name == "Synthetic 1-Step Login");
        }
    }

    public class CardsavrTests {

        [CollectionDefinition("CustomerAgentSession collection")]
        public class CustomerAgentCollection : ICollectionFixture<CustomerAgentSession> {
            // A class with no code, only used to define the collection
        }
    }

    public class CustomerAgentSession : IDisposable {
        public CardSavrHttpClient http { get; }
        public Context context { get; }
        public CustomerAgentSession() {
            this.http = new CardSavrHttpClient(Context.rejectUnauthorized);
            this.http.Setup(Context.accountBaseUrl, Context.accountCustomerAgentStaticKey,
                Context.accountCustomerAgentAppID, Context.accountCustomerAgentUserName, Context.accountCustomerAgentPassword);
            this.context = new Context();
            this.http.Init().Wait();
        }

        public void Dispose() {
        }
    }

}
