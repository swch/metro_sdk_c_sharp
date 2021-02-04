using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;

using Switch.CardSavr.Http;
using Newtonsoft.Json;


namespace cardsavr_e2e
{
    public class JobOps: OperationBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public JobOps()
        {
        }

        public override async Task Execute(CardSavrHttpClient http, Context ctx, params object[] extra)
        {
            // NOT BACKWARD COMPATIBLE - Only using cardholders, users are a different list
            List<User> cardholders = ctx.Cardholders;
            Context.CardholderData chd = ctx.CardholderSessions[cardholders[0].id ?? -1];
            
            PropertyBag bag = new PropertyBag()
            {
                { "account_id",  chd.accounts[0].id },
                { "cardholder_id", cardholders[0].id },
                { "card_id",  chd.cards[0].id },
                { "do_not_queue", false },
                { "requesting_brand", "mbudos" },
                { "site_hostname", chd.accounts[0].site_hostname},
                { "status", "INITIATED" }
            };
            
            // NOT BACKWARD COMPATIBLE - Only using agent now
            CardSavrResponse<SingleSiteJob> job = await http.CreateSingleSiteJobAsync(bag, chd.cardholder_safe_key);

            CardSavrResponse<List<SingleSiteJob>> singleJobs = await http.GetSingleSiteJobsAsync(
                new NameValueCollection() {
                    { "ids", job.Body.id.ToString() }
                }
            );
            log.Info($"retrieved {singleJobs.Body.Count} single-site jobs.");
            foreach (SingleSiteJob sj in singleJobs.Body)
                log.Info($"{sj.id}: {sj.status}");

            bag.Clear();
            bag["status"] = "CANCEL_REQUESTED";
            
            // NOT BACKWARD COMPATIBLE - Only using agent now
            job = await http.UpdateSingleSiteJobAsync(job.Body.id, bag, chd.cardholder_safe_key);
            log.Info($"{job.Body.id}: {job.Body.status}");

            //CardSavrResponse<List<CardPlacementResult>> results = 
            //    await http.GetCardPlacementResultReportingJobsAsync(null);
            //log.Info($"retrieved {results.Body.Count} card-placement result reporting jobs.");
            //foreach (CardPlacementResult cpr in results.Body)
            //    log.Info($"{cpr.id}: {cpr.site_name}, {cpr.status}");
        }

        public override async Task Cleanup(CardSavrHttpClient http, Context ctx)
        {
            // nothing to do currently.
            await base.Cleanup(http, ctx);
        }

        private async Task<bool> VerifyRole(CardSavrHttpClient http, Context ctx)
        {
            string[] roles = new string[] { "admin", "developer", "analyst", "swch_agent" };
            await UserOps.GetExecutingUserRole(http, ctx);
            if (Array.Find<string>(roles, r => r == ctx.ExecutionRole) == null)
            {
                log.Error($"execution role \"{ctx.ExecutionRole}\" not one of: {string.Join(", ", roles)}");
                return false;
            }

            return true;
        }
    }
}
