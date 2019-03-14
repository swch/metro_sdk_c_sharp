using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;

using Switch.CardSavr.Http;


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
            if (!await VerifyRole(http, ctx))
                return;

            CardSavrResponse<List<SingleSiteCardPlacementJob>> singleJobs = await http.GetSingleSiteJobsAsync(null);
            log.Info($"retrieved {singleJobs.Body.Count} single-site jobs.");
            foreach (SingleSiteCardPlacementJob sj in singleJobs.Body)
                log.Info($"{sj.id}: {sj.status}");

            CardSavrResponse<List<MultipleSiteCardPlacementJob>> multiJobs = await http.GetBatchJobsAsync(null);
            log.Info($"retrieved {multiJobs.Body.Count} multiple-site jobs.");
            foreach (MultipleSiteCardPlacementJob mj in multiJobs.Body)
                log.Info($"{mj.id}: {mj.total_site_count}=OK({mj.successful_site_count})+Failed({mj.failure_reason})");

            CardSavrResponse<List<CardPlacementResult>> results = 
                await http.GetCardPlacementResultReportingJobsAsync(null);
            log.Info($"retrieved {results.Body.Count} card-placement result reporting jobs.");
            foreach (CardPlacementResult cpr in results.Body)
                log.Info($"{cpr.id}: {cpr.site_name}, {cpr.status}");

            // TODO: create multi-site job.
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
