using System;
using System.Text;
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
    public class CardsavrSession : IDisposable
    {
        protected static readonly log4net.ILog log = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // test env: account information.  Customer Agent can do slightly more than a Cardholder Agent
        public static readonly bool rejectUnauthorized = false;

        // other resources.
        public static readonly string e2e_identifier = "c_sharp_e2e";
        public static readonly Random random = new Random();

        private InstanceConfig config = new InstanceConfig(); 

        public CardSavrHttpClient http { get;}

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
            //TODO - Add proxy support
            public string proxy_server;
            public string proxy_port;
            public string proxy_username;
            public string proxy_password;
        }

        public CardsavrSession()
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

                    this.config.app_name = dl["testing/credentials/primary/integrator/name"];
                    this.config.app_key = dl["testing/credentials/primary/integrator/key"];
                    this.config.app_password = dl["testing/credentials/primary/user/password"];
                    this.config.app_username = dl["testing/credentials/primary/user/username"];
                    this.config.cardsavr_server = "https://api." + dl["cardsavr/config/base_url"];
                }
            } else {
                using (StreamReader r = new StreamReader("../../../strivve_creds.json"))
                {
                    string json = r.ReadToEnd();
                    this.config = JsonConvert.DeserializeObject<InstanceConfig>(json);
                }
            }
            this.http = new CardSavrHttpClient(CardsavrSession.rejectUnauthorized);
            InstanceConfig conf = this.getConfig();
            this.http.Setup(
                conf.cardsavr_server, 
                conf.app_key, 
                conf.app_name, 
                conf.app_username, 
                conf.app_password); 
            this.http.Init().Wait();

        }

        public InstanceConfig getConfig(string instance) {
            foreach (InstanceConfig conf in this.config.instances) {
                if (conf.instance == instance) {
                    return conf;
                }
            }
            return null;
        }

        public InstanceConfig getConfig() {
            if (this.config.instance == null || this.getConfig(this.config.instance) == null) {
                return this.config;                
            }
            return getConfig(this.config.instance);
        }

        public void Dispose() {
        }

    }

    public class CardsavrTests {

        [CollectionDefinition("CardsavrSession collection")]
        public class CustomerAgentCollection : ICollectionFixture<CardsavrSession> {
            // A class with no code, only used to define the collection
        }
    }

}
