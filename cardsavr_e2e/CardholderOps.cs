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
        public CardholderOps()
        {
        }

        public override async Task Execute(CardSavrHttpClient http, Context ctx, params object[] extra)
        {
            int count = extra.Length > 0 ? (int)extra[0] : 4;
            Dictionary<int, CardSavrHttpClient> sessions = new Dictionary<int, CardSavrHttpClient>();

            List<User> newUsers = new List<User>();

            // create users.
            PropertyBag bag = new PropertyBag();
            for (int n = 0; n < count; ++n)
            {
                // generate using an easily reproducible safe-key.
                // the username, role and email help us identify these users later.
                bag["username"] = $"{Context.e2e_identifier}_{Context.random.Next(100)}_{n}";
                bag["cardholder_safe_key"] = Context.GenerateBogus32BitPassword(bag.GetString("username"));  //if we want to store it server-side
                bag["role"] = "cardholder";
                bag["first_name"] = $"Otto_{n}_cardholder";
                bag["last_name"] = $"Matic_{n}_cardholder";
                bag["email"] = $"cardsavr_e2e_{Context.random.Next(100)}@gmail.com";
                bag["phone_number"] = $"206-555-{n}{n + 1}{n + 2}{n + 3}".Substring(0, 12);

                CardSavrResponse<User> result = await http.CreateUserAsync(bag, bag.GetString("cardholder_safe_key"), "default");
                newUsers.Add(result.Body);
            }

            // update them. we'll just change the phone number.
            for (int n = 0; n < newUsers.Count; ++n)
            {
                CardSavrResponse<CredentialGrant> grant = await http.CreateUserGrantAsync((int)newUsers[n].id);
                string token = grant.Body.user_credential_grant;
                CardSavrHttpClient cardholder = new CardSavrHttpClient(Context.accountBaseUrl, Context.accountStaticKey[1], Context.accountAppID[1], newUsers[n].username, null, token);
                CardSavrResponse<LoginResult> login = await cardholder.Init();
                sessions.Add(newUsers[n].id ?? -1, cardholder); //never -1
            }

            // store the users we created for other code to use.
            ctx.CardholderSessions = sessions;
        }

        public override async Task Cleanup(CardSavrHttpClient http, Context ctx)
        {
            await base.Cleanup(http, ctx);
        }

    }
}