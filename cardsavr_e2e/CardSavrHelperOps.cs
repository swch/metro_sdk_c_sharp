using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Net.Http;
using System.Dynamic;

using Switch.CardSavr.Http;
using Newtonsoft.Json;

namespace cardsavr_e2e
{
    public class CardSavrHelperOps : OperationBase
    {
        public CardSavrHelperOps()
        {
        }

        protected static readonly log4net.ILog log = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public override async Task Execute(CardSavrHttpClient http, Context ctx, params object[] extra)
        {
            CardsavrHelper helper = new CardsavrHelper();
            helper.SetAppSettings(Context.accountBaseUrl, Context.accountCardholderAgentAppID, Context.accountCardholderAgentStaticKey, Context.rejectUnauthorized);
            await helper.LoginAndCreateSession(Context.accountCardholderAgentUserName, Context.accountCardholderAgentPassword, null, "{\"key\": \"my_trace\"}");
            
            PropertyBag cd = new PropertyBag(){{"my_fi", new PropertyBag(){{"token", "123"}}}};

            ClientLogin login = await helper.CreateCard(Context.accountCardholderAgentUserName, "default", 
                new User(){ email = "foo@foo.com", phone_number = "5555555555", custom_data = cd, username = $"{Context.e2e_identifier}_{Context.random.Next(100)}_0" },
                new Card(){ first_name="Strivve", last_name="User", pan="4111111111111111", cvv="111", expiration_month="01", expiration_year="25" },
                new Address(){ is_primary=true, address1="1234 1st ave", city="Seattle", subnational="WA", postal_code="98006", country="USA" }
            );
            await helper.CloseSession(Context.accountCardholderAgentUserName);
            log.Info("CARDSAVRHELPEROPS username: " + login.cardholder.username + ", grant: " + login.userCredentialGrant + ", card_id: " + login.card.id);
            //login can now be used as a redirect to a url that can log in and process jobs
        }

        public override async Task Cleanup(CardSavrHttpClient http, Context ctx)
        {
            await base.Cleanup(http, ctx);
        }

    }
}