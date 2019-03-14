using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;

using Switch.CardSavr.Http;


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
            body["description"] = Context.e2e_identifier;
            body["integrator_type"] = "cust_internal";

            CardSavrResponse<Integrator> result = await http.CreateIntegratorAsync(body);
            log.Info($"created integrator: {result.Body.id}");

            // update it.
            body.Clear();
            body["description"] = $"{Context.e2e_identifier}_{Context.e2e_identifier}";
            await http.UpdateIntegratorAsync(result.Body.id, body);
            log.Info($"updated integrator");
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
                    if (i.description.StartsWith(Context.e2e_identifier, StringComparison.CurrentCulture))
                        ids.Add(i.id.ToString());
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
