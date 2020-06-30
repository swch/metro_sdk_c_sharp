using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Net.Http;

using Switch.CardSavr.Http;

namespace cardsavr_e2e
{
    public class CardSavrHelperOps : OperationBase
    {
        public CardSavrHelperOps()
        {
        }

        public override async Task Execute(CardSavrHttpClient http, Context ctx, params object[] extra)
        {
            CardsavrHelper helper = new CardsavrHelper();
            helper.SetAppSettings(Context.accountBaseUrl, Context.accountCardholderAgentAppID, Context.accountCardholderAgentStaticKey);
            await helper.LoginAndCreateSession(Context.accountCardholderAgentUserName, Context.accountCardholderAgentPassword);
            ClientLogin login = await helper.CreateCard(Context.accountCardholderAgentUserName, "default", 
                new User(){ email = "foo@foo.com", phone_number = "5555555555" },
                new Card(){ first_name="Strivve", last_name="User", pan="4111111111111111", cvv="111", expiration_month="01", expiration_year="25" },
                new Address(){ is_primary=true, address1="1234 1st ave", city="Seattle", subnational="WA", postal_code="98006", country="USA" }
            );
            await helper.CloseSession(Context.accountCardholderAgentUserName);
            //login can now be used as a redirect to a url that can log in and process jobs
        }

        public override async Task Cleanup(CardSavrHttpClient http, Context ctx)
        {
            await base.Cleanup(http, ctx);
        }

    }
}