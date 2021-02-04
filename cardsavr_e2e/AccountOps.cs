using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Newtonsoft.Json;

using Switch.CardSavr.Http;


namespace cardsavr_e2e
{
    public class AccountOps : OperationBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public AccountOps()
        {
        }

        public override async Task Execute(CardSavrHttpClient http, Context ctx, params object[] extra)
        {
            // pick a user and merchant site.
            User user = ctx.Cardholders[0];
            Context.CardholderData chd = ctx.CardholderSessions[user.id ?? -1];

            //MerchantSite site = ctx.GetSyntheticSite();
            CardSavrResponse<List<MerchantSite>> sites = await http.GetMerchantSitesAsync(new NameValueCollection() {
                    { "host", "synthetic-1-step.herokuapp.com" }
                });

            // create an account.
            PropertyBag bag = new PropertyBag();
            bag["cardholder_id"] = user.id;
            bag["merchant_site_id"] = sites.Body[0].id;
            bag["username"] = "goodemail";
            bag["password"] = "";

            // users created by our test-suite have known/bogus safe-key.
            // NOT BACKWARD COMPATIBLE - client is now always the agent
            CardSavrResponse<Account> result = await http.CreateAccountAsync(bag, chd.cardholder_safe_key);
            log.Info($"created account {result.Body.id} for user-id={user.id} ({user.username})");

            // update it.
            bag.Clear();
            bag["password"] = $"{Context.e2e_identifier}-{Context.e2e_identifier}";
            result = await http.UpdateAccountAsync(result.Body.id, bag, chd.cardholder_safe_key);
            log.Info($"updated account-id={result.Body.id}");

            List<Account> list = new List<Account>();
            list.Add(result.Body);
            ctx.CardholderSessions[user.id ?? -1].accounts = list;
        }

        public override async Task Cleanup(CardSavrHttpClient http, Context ctx)
        {
            List<Account> toDelete = new List<Account>();

            int total = 0;
            Paging paging = new Paging() { PageLength = 100 };
            while (total < paging.TotalResults || paging.TotalResults < 0)
            {
                CardSavrResponse<List<Account>> result = await http.GetAccountsAsync(null, paging);
                foreach (Account a in result.Body)
                {
                    // NOT BACKWARD COMPATIBLE - merchant_site_id now required to filter site id.
                    if (a.merchant_site_id == "1") //ctx.GetSyntheticSite().host)
                        toDelete.Add(a);
                }

                total += result.Body.Count;
                paging = result.Paging;
                paging.Page += 1;
            }

            log.Info($"found {toDelete.Count} accounts to delete.");

            if (toDelete.Count > 0)
            {
                foreach (Account a in toDelete)
                {
                    // NOT BACKWARD COMPATIBLE - safe key no longer required to delete an account.
                    //User user = ctx.FindUserById(a.cardholder_id);
                    string safeKey = null; //Context.GenerateBogus32BitPassword(user.username);
                    await http.DeleteAccountAsync(a.id, safeKey);
                }

                log.Info($"deleted {toDelete.Count} accounts.");
            }
        }
    }
}
