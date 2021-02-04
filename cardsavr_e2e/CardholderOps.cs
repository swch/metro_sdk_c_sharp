using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Net.Http;

using Switch.CardSavr.Http;

namespace cardsavr_e2e
{
    public class CardholderOps : UserOps
    {
        protected new static readonly log4net.ILog log = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public CardholderOps()
        {
        }

        public override async Task Execute(CardSavrHttpClient http, Context ctx, params object[] extra)
        {
            int count = extra.Length > 0 ? (int)extra[0] : 4;
            Dictionary<int, Context.CardholderData> sessions = new Dictionary<int, Context.CardholderData>();

            List<User> newUsers = new List<User>();

            // create users.
            PropertyBag bag = new PropertyBag();
            for (int n = 0; n < count; ++n)
            {
                // generate using an easily reproducible safe-key.
                // the username, role and email help us identify these users later.
                bag["username"] = $"{Context.e2e_identifier}_{Context.random.Next(100)}_{n}";
                bag["role"] = "cardholder";
                bag["first_name"] = $"Otto_{n}_cardholder";
                bag["last_name"] = $"Matic_{n}_cardholder";
                bag["email"] = $"cardsavr_e2e_{Context.random.Next(100)}@gmail.com";
                bag["phone_number"] = $"206-555-{n}{n + 1}{n + 2}{n + 3}".Substring(0, 12);

                CardSavrResponse<User> result = await http.CreateUserAsync(bag, count % 2 == 0 ? null : Context.GenerateBogus32BitPassword(bag.GetString("username")));
                newUsers.Add(result.Body);
            }

            // update them. we'll just change the phone number.
            for (int n = 0; n < newUsers.Count; ++n)
            {
                // NOT BACKWARD COMPATIBLE - No longer can assume cardholders
                /*
                string token = newUsers[n].credential_grant;
                CardSavrHttpClient cardholder = new CardSavrHttpClient(false);
                cardholder.Setup(Context.accountBaseUrl, Context.accountCardholderAgentStaticKey, Context.accountCardholderAgentAppID, newUsers[n].cuid, null, token);
                CardSavrResponse<LoginResult> login = await cardholder.Init();
                */
                Context.CardholderData chd = new Context.CardholderData();
                //chd.client = cardholder; 
                sessions.Add((int)newUsers[n].id, chd);
            }
            
            // store the users we created for other code to use.
            ctx.CardholderSessions = sessions;
            await GetAllCardholders(http, ctx);
            
        }

        public override async Task Cleanup(CardSavrHttpClient http, Context ctx)
        {
            await base.Cleanup(http, ctx);
        }

        protected async Task GetAllCardholders(CardSavrHttpClient http, Context ctx, int pageLength = 7)
        {
            log.Info("retrieving users...");
            Paging p = new Paging(1, pageLength);
            List<Cardholder> users = new List<Cardholder>();

            while (users.Count < p.TotalResults || p.TotalResults < 0)
            {
                log.Info($"getting page {p.Page}");

                CardSavrResponse<List<Cardholder>> result = await http.GetCardholdersAsync(null, p);
                log.Info(String.Format("{0} users returned", result.Body.Count));

                if (result.Body.Count > 0)
                {
                    users.AddRange(result.Body);

                    foreach (Cardholder u in result.Body)
                        log.Info(String.Format("{0} = \"{1} {2}\" {3}", u.username, u.first_name, u.last_name, u.role));
                }

                p = result.Paging;
                p.Page += 1;
            }

            log.Info($"retrieved a total of {users.Count} users");
            ctx.Cardholders = users.ConvertAll(x => (User)x);
        }
    }
}