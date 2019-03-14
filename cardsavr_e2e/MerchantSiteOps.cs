using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;

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
            // retrieve and store (for reference) a bunch of merchant sites (probably all of them).
            Paging paging = new Paging(1, 200);
            log.Info("retrieving some merchant sites...");
            ctx.Sites = (await http.GetMerchantSitesAsync(null, paging)).Body;

            log.Info(String.Format("{0} merchant sites returned", ctx.Sites.Count));
            foreach (MerchantSite m in ctx.Sites)
                log.Info($"{m.id}: {m.site_name}: {m.site_hostname}");

            // site tags.
            log.Info("retrieving all merchant site tags:");
            CardSavrResponse<List<MerchantSiteTag>> tags = await http.GetMerchantSiteTagsAsync(null);
            foreach (MerchantSiteTag tag in tags.Body)
                log.Info($"{tag.name}=\"{tag.description}\"");

            // site tag to site tag links.
            log.Info("retrieving all merchant site tag to site tag links:");
            CardSavrResponse<List<MerchantSiteToSiteTagLink>> links =
                await http.GetMerchantSiteToSiteTagLinksAsync(null);
            foreach (MerchantSiteToSiteTagLink link in links.Body)
                log.Info($"{link.id}={link.merchant_site_id}, {link.merchant_site_tag_id}");
        }

        public override async Task Cleanup(CardSavrHttpClient http, Context ctx)
        {
            // nothing to do; we didn't create anything.
            await base.Cleanup(http, ctx);
        }
    }
}
