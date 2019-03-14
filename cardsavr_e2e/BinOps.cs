using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;

using Switch.CardSavr.Http;


namespace cardsavr_e2e
{
    public class BinOps : OperationBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public BinOps()
        {
        }

        public override async Task Execute(CardSavrHttpClient http, Context ctx, params object[] extra)
        {
            // list the first page of BINs to exercise a generic get.
            CardSavrResponse<List<BIN>> bins = await http.GetBinsAsync(null);
            log.Info($"BIN query returned {bins.Body.Count} items.");

            foreach (BIN b in bins.Body)
                log.Info($"BIN: {b.id}: {b.brand} {b.issuer}");

            // create a new BIN using our special identifier as the brand.
            PropertyBag bag = new PropertyBag();
            bag["id"] = $"98765{Context.random.Next(1, 99)}";
            bag["brand"] = Context.e2e_identifier;
            bag["issuer"] = Context.e2e_identifier;
            bag["level"] = "Platinum";
            bag["type"] = "prepaid";

            CardSavrResponse<BIN> result = await http.CreateBinAsync(bag);
            log.Info($"created BIN: {result.Body.id}: {result.Body.brand}");

            // update it to be sure we can.
            bag.Clear();
            bag["id"] = result.Body.id;
            bag["type"] = "credit";
            CardSavrResponse<List<BIN>> lst = await http.UpdateBinAsync(bag.GetString("id"), bag);
            log.Info($"updated bin: returned {lst.Body.Count} items");

            // save a reference to the BIN for other code to use.
            ctx.Bin = lst.Body[0];
        }

        public override async Task Cleanup(CardSavrHttpClient http, Context ctx)
        {
            // deletes all bins using our special identifier as the brand. there should only be one
            // unless an earlier run crashed and left things lying around.
            CardSavrResponse<List<BIN>> bins = await http.GetBinsAsync(new NameValueCollection()
            {
                { "brand", Context.e2e_identifier }
            });

            foreach (BIN b in bins.Body)
                await http.DeleteBinAsync(b.id);

            log.Info($"deleted {bins.Body.Count} bins");
        }
    }
}
