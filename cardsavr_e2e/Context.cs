using System;
using System.Collections.Generic;
using System.Text;

using Switch.CardSavr.Http;
using Switch.Security;
using System.Linq;

namespace cardsavr_e2e
{
    /// <summary>
    /// Context of the current test run.
    /// </summary>
    public class Context
    {
        // test env: account information.  Customer Agent can do slightly more than a Cardholder Agent
        public static readonly string accountBaseUrl = "https://api.mbudos.cardsavr.io";
        public static readonly string accountCustomerAgentAppID = "cardupdatr_integrator";
        public static readonly string accountCustomerAgentStaticKey = "sEuuK6KhawoJE9Zf+uGebJ/rkCMaCLvowM46T4JoE8Q=";
        public static readonly string accountCustomerAgentUserName = "cardupdatr_user";
        public static readonly string accountCustomerAgentPassword = "tRSfD6HMuua6ai2S98B5zzgQC1jIo7ea06yiItQt9UQ=";
        public static readonly string accountCardholderAgentAppID = "CardUpdatr Demo";
        public static readonly string accountCardholderAgentStaticKey = "TGSEjt4TuK0j55TeF7x1cao88Bc1j8nyHeaBHueT5gQ=";
        public static readonly string accountCardholderAgentUserName = "cardupdatr_demo";
        public static readonly string accountCardholderAgentPassword = "uLa64$#Rf8bh";
        // other resources.
        public static readonly string e2e_identifier = "c_sharp_e2e";
        public static readonly Random random = new Random();

        // properties.
        public List<MerchantSite> Sites { get; set; }
        public List<User> Users { get; set; }
        public Dictionary<int, CardholderData> CardholderSessions { get; set; }
        public string Trace { get; set; }
        public bool Started { get; set; }
        public string ExecutionRole { get; set; }

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

        public Context(bool started = false)
        {
            Started = started;
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
}
