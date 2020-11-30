using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;

using Switch.CardSavr.Http;
using Switch.Security;
using Switch.CardSavr.Exceptions;

namespace cardsavr_e2e
{
    public class IntegratorOps : OperationBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public IntegratorOps()
        {
        }

        public override async Task Execute(CardSavrHttpClient http, Context ctx, params object[] extra)
        {
            // create one.
            PropertyBag body = new PropertyBag();
            body["name"] = Context.e2e_identifier + $"_{Context.random.Next(100, 200)}";
            string integrator_name = (string)body["name"];
            body["description"] = Context.e2e_identifier;
            body["integrator_type"] = "cust_internal";

            CardSavrResponse<Integrator> result = await http.CreateIntegratorAsync(body);
            log.Info($"created integrator: {result.Body.id}");

            // update it.
            log.Info(integrator_name);
            string last_key = result.Body.current_key;
            string new_key = Aes256.GetRandomString(44, 32);
            body.Clear();
            body["description"] = $"{Context.e2e_identifier}_{Context.e2e_identifier}";
            body["current_key"] = new_key;
            body["last_key"] = last_key;

            CardSavrResponse<List<Integrator>> updated = await http.UpdateIntegratorsAsync(result.Body.id, body);
            if (updated.Body.Count != 1) {
                throw new InvalidStateException("Only should be updating a single Integrator.");
            }
            Integrator i = updated.Body[0];
            if (i.current_key != new_key || i.last_key != last_key) {
                throw new InvalidStateException("Integrator not updated.");
            }
            log.Info($"updated integrator {integrator_name}");
            
            CardsavrHelper helper = new CardsavrHelper();
            helper.SetAppSettings(Context.accountBaseUrl, Context.accountCustomerAgentAppID, Context.accountCustomerAgentStaticKey, Context.rejectUnauthorized);
            await helper.LoginAndCreateSession(Context.accountCustomerAgentUserName, Context.accountCustomerAgentPassword, null, "{\"key\": \"my_trace\"}");

            new_key = await helper.RotateIntegrator(Context.accountCustomerAgentUserName, integrator_name);
            log.Info($"rotated integrator {integrator_name} to {new_key}");

            helper.SetAppSettings(Context.accountBaseUrl, integrator_name, new_key, Context.rejectUnauthorized);
            await helper.LoginAndCreateSession(Context.accountCustomerAgentUserName, Context.accountCustomerAgentPassword, null, "{\"key\": \"my_trace\"}");

            string new_new_key = await helper.RotateIntegrator(Context.accountCustomerAgentUserName, integrator_name);
            log.Info($"rotated integrator {integrator_name} to {new_new_key} using 'new' integrator");

            helper.SetAppSettings(Context.accountBaseUrl, integrator_name, new_key, Context.rejectUnauthorized);
            await helper.LoginAndCreateSession(Context.accountCustomerAgentUserName, Context.accountCustomerAgentPassword, null, "{\"key\": \"my_trace\"}");
            log.Info($"logged in successfully with 'old' 'new' integrator");

            helper.SetAppSettings(Context.accountBaseUrl, integrator_name, new_new_key, Context.rejectUnauthorized);
            await helper.LoginAndCreateSession(Context.accountCustomerAgentUserName, Context.accountCustomerAgentPassword, null, "{\"key\": \"my_trace\"}");
            log.Info($"logged in successfully with 'new' 'new' integrator");

            helper.SetAppSettings(Context.accountBaseUrl, integrator_name, (string)body["current_key"], Context.rejectUnauthorized);
            if ((await helper.LoginAndCreateSession(Context.accountCustomerAgentUserName, Context.accountCustomerAgentPassword, null, "{\"key\": \"my_trace\"}")) == null) {
                log.Info($"SHOULD BE AN EXCEPTION ABOVE: Correctly can't log in, integrator key is rotated out (twice)");
            }
        }

        public override async Task Cleanup(CardSavrHttpClient http, Context ctx)
        {
            List<string> ids = new List<string>();

            int total = 0;
            Paging paging = new Paging() { PageLength = 100 };
            while (total < paging.TotalResults || paging.TotalResults < 0)
            {
                CardSavrResponse<List<Integrator>> result = await http.GetIntegratorsAsync(null, paging);
                foreach (Integrator i in result.Body)
                {
                    if (i.description != null && i.description.StartsWith(Context.e2e_identifier, StringComparison.CurrentCulture)) {
                        ids.Add(i.id.ToString());
                    }
                }

                total += result.Body.Count;
                paging = result.Paging;
                paging.Page += 1;
            }

            log.Info($"found {ids.Count} integrators to delete.");
            if (ids.Count > 0)
            {
                await http.DeleteIntegratorAsync(ids);
                log.Info($"deleted {ids.Count} integrators.");
            }
        }
    }
}
