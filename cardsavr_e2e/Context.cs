using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

using Switch.CardSavr.Http;
using Switch.Security;
using System.Linq;
using Xunit;
using System.IO;
using Newtonsoft.Json;

namespace cardsavr_e2e
{
    /// <summary>
    /// Context of the current test run.
    /// </summary>
    public class Context
    {

        protected static readonly log4net.ILog log = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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

        private InstanceConfig config = new InstanceConfig(); 

        public static string GenerateBogus32BitPassword(string username)
        {
            byte[] sequence = new byte[32];
            for (byte n = 0; n < sequence.Length; ++n)
                sequence[n] = n;

            String password = Convert.ToBase64String(sequence);
            byte[] salt = Encoding.UTF8.GetBytes(username);
            return Convert.ToBase64String(HashUtil.Sha256Pbkdf2(password, salt, 5000, 32));
        }

        class KeyValue {
            #pragma warning disable 0649
            public string key;
            public dynamic value;
        }

        public class InstanceConfig {
            public string instance;
            public InstanceConfig[] instances;
            public string app_name;
            public string app_key;
            public string app_username;
            public string app_password;
            public string cardsavr_server;
            public string proxy_server;
            public string proxy_port;
            public string proxy_username;
            public string proxy_password;
        }

        public Context()
        {
            string localGeneratedConfig = "../../../docker.local.json";
            
            if (File.Exists(localGeneratedConfig)) {
                using (StreamReader r = new StreamReader("../../../docker.local.json"))
                {
                    string json = r.ReadToEnd();
                    KeyValue[] array = JsonConvert.DeserializeObject<KeyValue[]>(json);
                    Dictionary<string, string> dl = new Dictionary<string, string>();
                    foreach (KeyValue kv in array) {
                        dl[kv.key] = kv.value;
                    }

                    config.app_name = dl["testing/credentials/primary/integrator/name"];
                    config.app_key = dl["testing/credentials/primary/integrator/key"];
                    config.app_password = dl["testing/credentials/primary/user/password"];
                    config.app_username = dl["testing/credentials/primary/user/username"];
                    config.cardsavr_server = dl["api_url_override"];
                }
            } else {
                using (StreamReader r = new StreamReader("../../../strivve_creds.json"))
                {
                    string json = r.ReadToEnd();
                    config = JsonConvert.DeserializeObject<InstanceConfig>(json);
                }
             }
        }

        public InstanceConfig getConfig(string instance) {
            foreach (InstanceConfig conf in config.instances) {
                if (conf.instance == instance) {
                    return conf;
                }
            }
            return null;
        }

        public InstanceConfig getConfig() {
            if (config.instance == null) {
                return config;                
            }
            return getConfig(config.instance);
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

        protected static readonly log4net.ILog log = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public CustomerAgentSession() {

            this.context = new Context();
            this.http = new CardSavrHttpClient(Context.rejectUnauthorized);
            Context.InstanceConfig config = context.getConfig();
            this.http.Setup(
                config.cardsavr_server, 
                config.app_key, 
                config.app_name, 
                config.app_username, 
                config.app_password); 
            this.http.Init().Wait();
        }

        public void Dispose() {
        }
    }

}
