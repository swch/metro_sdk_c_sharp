using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Net;

using Switch.CardSavr.Http;


namespace cardsavr_e2e
{
    public class MerchantSiteOps : OperationBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public MerchantSiteOps()
        {
        }

        public override async Task Execute(CardSavrHttpClient http, Context ctx, params object[] extra)
        {
            Paging paging = new Paging() { PageLength = 200, Sort = "name" };
            CardSavrResponse<List<MerchantSite>> merchants = await http.GetMerchantSitesAsync(null, paging);
            foreach (MerchantSite merch in merchants.Body) {
                log.Info($"Loaded merchant site: {merch.name}: {merch.host}");
            }
            ctx.Sites = merchants.Body;
        }

        public override async Task Cleanup(CardSavrHttpClient http, Context ctx)
        {
            // nothing to do; we didn't create anything.
            await base.Cleanup(http, ctx);
        }
    }
}
