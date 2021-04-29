using System;
using System.Threading.Tasks;
using Switch.CardSavr.Http;

namespace cardsavr_app
{
    class Program
    {

        static void Main(string[] args) {
            new Program().TestCreateHydratedCard().Wait();
        }

        public async Task<ClientLogin> TestCreateHydratedCard() {

            try {
                CardsavrHelper helper = new CardsavrHelper();
                helper.SetAppSettings("<REDACTED>", "<REDACTED>", "<REDACTED>", true);
                ClientSession auth = await helper.LoginAndCreateSession("<REDACTED>", "<REDACTED>", "{\"key\": \"my_trace\"}");
                
                PropertyBag cd = new PropertyBag(){{"my_fi", new PropertyBag(){{"token", "123"}}}};
                ClientLogin login = await helper.CreateCard("testing_user", "default", 
                    new Cardholder(){ email = "foo@foo.com", custom_data = cd, cuid = "1234" },
                    new Card(){ first_name="Strivve", last_name="User", pan="4111111111111111", cvv="111", expiration_month="01", expiration_year="25" },
                    new Address(){ is_primary=true, phone_number = "5555555555", address1="1234 1st ave", city="Seattle", subnational="WA", postal_code="98006", country="USA" }
                );
                await helper.CloseSession("AdvAppUser");
                Console.WriteLine("CARDSAVRHELPERTESTS cuid: " + login.cardholder.cuid + ", grant: " + login.grant + ", card_id: " + login.card.id);

                return login;
            } catch (Exception e) {
                Console.Write(e);
            }
            return null;
        }
    }
}
