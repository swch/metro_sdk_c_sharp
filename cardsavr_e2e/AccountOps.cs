using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
            User user = ctx.GetRandomNewUser();
            MerchantSite site = ctx.GetRandomSite();

            // create an account.
            PropertyBag bag = new PropertyBag();
            bag["cardholder_id"] = user.id;
            bag["merchant_site_id"] = site.id;
            bag["username"] = user.username;
            bag["custom_display_text"] = Context.e2e_identifier;

            // users created by our test-suite have known/bogus safe-key.
            string safeKey = Context.GenerateBogus32BitPassword(user.username);
            CardSavrResponse<Account> result = await http.CreateAccountAsync(bag, safeKey);
            log.Info($"created account {result.Body.id} for user-id={user.id} ({user.username})");

            // update it.
            bag.Clear();
            bag["custom_display_text"] = $"{Context.e2e_identifier}-{Context.e2e_identifier}";
            result = await http.UpdateAccountAsync(result.Body.id, bag, safeKey);
            log.Info($"updated account-id={result.Body.id}");
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
                    if (a.custom_display_text.StartsWith(Context.e2e_identifier, StringComparison.CurrentCulture))
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
                    User user = ctx.FindUserById(a.cardholder_id);
                    string safeKey = Context.GenerateBogus32BitPassword(user.username);
                    await http.DeleteAccountAsync(a.id, safeKey);
                }

                log.Info($"deleted {toDelete.Count} accounts.");
            }
        }
    }
}
