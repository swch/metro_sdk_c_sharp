using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;

using Switch.CardSavr.Http;
using Switch.Security;
using Xunit;
using Xunit.Priority;
using Newtonsoft.Json; 

namespace cardsavr_e2e
{
    [TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
    [Collection("CardsavrSession collection")]
    public class UserTests
    {
        protected static readonly log4net.ILog log = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        CardsavrSession session;

        public UserTests(CardsavrSession session)
        {
            this.session = session;
        }

        [Fact, Priority(1)]
        public async void TestCreateUpdateUsers() {

            // create users.
            PropertyBag bag = new PropertyBag();
            string password = "";
            List<User> list = new List<User>();
            for (int n = 0; n < 10; ++n)
            {
                // generate using an easily reproducible safe-key.
                // the username, role and email help us identify these users later.
                bag["username"] = $"{CardsavrSession.e2e_identifier}_{CardsavrSession.random.Next(1000)}_{n}";
                password = $"{CardsavrSession.e2e_identifier}_{CardsavrSession.random.Next(1000)}_{n}";
                bag["password"] = password;
                bag["role"] = "customer_agent";
                bag["first_name"] = $"Otto_{n}";
                bag["last_name"] = $"Matic_{n}";
                bag["email"] = $"cardsavr_e2e_{CardsavrSession.random.Next(1000)}@gmail.com";
                CardSavrResponse<User> result = await this.session.http.CreateUserAsync(bag);
            }

            CardSavrResponse<List<User>> users = await this.session.http.GetUsersAsync(null, new Paging() { PageLength = 100 });
            foreach (User user in users.Body) {
                if (!user.username.StartsWith(CardsavrSession.e2e_identifier)) {
                    continue;
                }
                bag.Clear();
                bag["id"] = user.id;
                bag["email"] = "new_" + user.email;
                CardSavrResponse<List<User>> updated_users = await this.session.http.UpdateUserAsync(user.id, bag);
                //log.Info(JsonConvert.SerializeObject(updated_users.Body[0], Formatting.Indented));
                Assert.Equal(bag["email"], updated_users.Body[0].email);
                list.Add(user);
            }

            Assert.Equal(10, list.Count);

            CardsavrHelper helper = new CardsavrHelper();
            CardsavrSession.InstanceConfig config = session.getConfig();
            helper.SetAppSettings(config.cardsavr_server, config.app_name, config.app_key, CardsavrSession.rejectUnauthorized);
            //use the latest user
            string testing_agent = list[list.Count - 1].username;
            await helper.LoginAndCreateSession(testing_agent, password);
            string new_password = await helper.RotateAgentPassword(testing_agent);
            await helper.CloseSession(testing_agent);
            ClientSession login = await helper.LoginAndCreateSession(testing_agent, new_password);
            Assert.NotNull(login);

        }

        [Fact, Priority(20)]
        public async void TestUpdatePassword() {
            CardSavrResponse<List<User>> users = await this.session.http.GetUsersAsync(null);
            foreach (User user in users.Body) {
                if (user.username.StartsWith(CardsavrSession.e2e_identifier)) {
                    log.Info($"updating password for user \"{user.username}\" ({user.id})");

                    string pwBase = "foobar";
                    string newPassword = pwBase.PadRight(44 - pwBase.Length);
                    PropertyBag bag = new PropertyBag();
                    bag["username"] = user.username;
                    bag["old_password"] = CardsavrSession.GenerateBogus32BitPassword(CardsavrSession.e2e_identifier);
                    bag["password"] = CardsavrSession.GenerateBogus32BitPassword(user.username + CardsavrSession.e2e_identifier);
                    bag["password_copy"] = bag["password"];

                    CardSavrResponse<PropertyBag> result = await this.session.http.UpdateUserPasswordAsync((int)user.id, bag);
                    log.Info($"password update successful for user \"{user.username}\" ({user.id})");
                    Assert.Equal(HttpStatusCode.Created, result.StatusCode);
                    break;
                }
            }

            List<int> list = new List<int>();
            users = await this.session.http.GetUsersAsync(null, new Paging() { PageLength = 100 });
            foreach (User u in users.Body) {
                if (!u.username.StartsWith(CardsavrSession.e2e_identifier)) {
                    continue;
                }
                list.Add((int)u.id);
            }
            CardSavrResponse<List<User>> deleted_users = await this.session.http.DeleteUserAsync(list);
            Assert.Equal(10, deleted_users.Body.Count);
        }

    }

}