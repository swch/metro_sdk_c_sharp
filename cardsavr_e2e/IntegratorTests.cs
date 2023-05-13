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

namespace cardsavr_e2e
{
    [TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
    [Collection("CardsavrSession collection")]
    public class IntegratorTests
    {
        CardsavrSession session;

        public IntegratorTests(CardsavrSession session)
        {
            this.session = session;
        }

        public static Integrator integrator { get; set; }

        [Fact, Priority(1)]
        public async void TestCreateIntegrator() {

            PropertyBag body = new PropertyBag();
            body["name"] = CardsavrSession.e2e_identifier + $"_{CardsavrSession.random.Next(1000, 2000)}";
            string integrator_name = (string)body["name"];
            body["description"] = CardsavrSession.e2e_identifier;
            body["integrator_type"] = "cust_internal";

            CardSavrResponse<Integrator> result = await this.session.http.CreateIntegratorAsync(body);
            log.Info($"created integrator: {result.Body.id}");
            IntegratorTests.integrator = result.Body;
            Assert.Equal(HttpStatusCode.Created, result.StatusCode);
            Assert.True(integrator.id > 0);
        }

        [Fact, Priority(2)]
        public async void TestUpdateIntegrator() {
            string last_key = IntegratorTests.integrator.current_key;
            string new_key = Aes256.GetRandomString(44, 32);
            PropertyBag body = new PropertyBag();
            body["description"] = $"{CardsavrSession.e2e_identifier}_{CardsavrSession.e2e_identifier}";
            body["current_key"] = new_key;
            body["last_key"] = last_key;

            CardSavrResponse<Integrator> updated = await this.session.http.UpdateIntegratorsAsync(IntegratorTests.integrator.id, body);

            IntegratorTests.integrator = updated.Body;
            Assert.Equal(new_key, IntegratorTests.integrator.current_key);
            Assert.Equal(last_key, IntegratorTests.integrator.last_key);
            log.Info($"updated integrator {IntegratorTests.integrator.name}");

            Assert.Equal(HttpStatusCode.Created, updated.StatusCode);

            CardsavrSession.InstanceConfig config = session.getConfig();
            CardsavrHelper helper = new CardsavrHelper();
            helper.SetAppSettings(config.cardsavr_server, IntegratorTests.integrator.name, last_key, CardsavrSession.rejectUnauthorized);
            ClientSession login = await helper.LoginAndCreateSession(config.app_username, config.app_password, "{\"key\": \"my_trace\"}");
            CardSavrResponse<List<Integrator>> integrator = await login.client.GetIntegratorsAsync(IntegratorTests.integrator.id);
            Assert.Equal(integrator.Body[0].id, IntegratorTests.integrator.id);

            string new_new_key = await helper.RotateIntegrator(config.app_username, IntegratorTests.integrator.name);

            log.Info($"rotated integrator {IntegratorTests.integrator.name} to {new_new_key} using 'old' integrator");

            helper.SetAppSettings(config.cardsavr_server, IntegratorTests.integrator.name, new_key, CardsavrSession.rejectUnauthorized);
            login = await helper.LoginAndCreateSession(config.app_username, config.app_password, "{\"key\": \"my_trace\"}");
            log.Info($"logged in successfully with 'old' 'new' integrator");

            helper.SetAppSettings(config.cardsavr_server, IntegratorTests.integrator.name, new_new_key, CardsavrSession.rejectUnauthorized);
            login = await helper.LoginAndCreateSession(config.app_username, config.app_password, "{\"key\": \"my_trace\"}");
            log.Info($"logged in successfully with 'new' 'new' integrator");

            helper.SetAppSettings(config.cardsavr_server, IntegratorTests.integrator.name, last_key, CardsavrSession.rejectUnauthorized);
            Exception ex = await Assert.ThrowsAsync<RequestException>(async () => {
                await helper.LoginAndCreateSession(config.app_username, config.app_password, "{\"key\": \"my_trace\"}");
            });
            Assert.True(ex.Message.IndexOf("Request Signature failed") >= 0);
        }

        [Fact, Priority(10)]
        public async void TestDeleteIntegrator() {

            CardSavrResponse<Integrator> deleted = await this.session.http.DeleteIntegratorAsync(IntegratorTests.integrator.id);

            IntegratorTests.integrator = deleted.Body;

            Assert.Equal(HttpStatusCode.OK, deleted.StatusCode);

        }

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    }
}
