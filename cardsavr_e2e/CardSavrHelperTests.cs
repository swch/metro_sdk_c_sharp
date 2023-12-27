using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Net;

using Switch.CardSavr.Http;
using Switch.Security;
using Switch.CardSavr.Exceptions;
using Xunit;
using Xunit.Priority;
using Newtonsoft.Json;

namespace cardsavr_e2e
{
    [TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
    [Collection("CardsavrSession collection")]
    public class CardSavrHelperTests
    {
        CardsavrSession session;

        public CardSavrHelperTests(CardsavrSession session)
        {
            this.session = session;
        }

        [Fact, Priority(1)]
        public async void TestCreateHydratedCard() {

            CardsavrSession.InstanceConfig config = this.session.getConfig();
            CardsavrHelper helper = new CardsavrHelper();
            helper.SetAppSettings(config.cardsavr_server, config.app_name, config.app_key, CardsavrSession.rejectUnauthorized);
            await helper.LoginAndCreateSession(config.app_username, config.app_password, "{\"key\": \"my_trace\"}");
            
            PropertyBag cd = new PropertyBag(){{"my_fi", new PropertyBag(){{"token", "123"}}}};
            string cuid = $"{CardsavrSession.e2e_identifier}_{CardsavrSession.random.Next(10000)}_0";
            ClientLogin login = await helper.CreateCard(config.app_username, 
                new Cardholder(){ custom_data = cd, cuid = cuid },
                new Card(){ pan="4111111111111111", cvv="111", expiration_month="01", expiration_year="25", customer_key="5555555555" },
                new Address(){ first_name="Strivve", last_name="User", email = "foo@foo.com", is_primary=true, phone_number = "5555555555", address1="1234 1st ave", city="Seattle", subnational="WA", postal_code="98006", country="USA" }
            );
            await helper.CloseSession(config.app_username);
            Assert.NotNull(login.cardholder.grant);
            Assert.Equal(login.cardholder.cuid, cuid);
            Assert.True(login.card.id > 0);
            log.Info("CARDSAVRHELPERTESTS cuid: " + login.cardholder.cuid + ", grant: " + login.grant + ", card_id: " + login.card.id);
            //login can now be used as a redirect to a url that can log in and process jobs
            await this.session.http.DeleteCardholderAsync(login.cardholder.id);
        }

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    }
}
