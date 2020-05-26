using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;

using Switch.CardSavr.Http;


namespace cardsavr_e2e
{
    public class AddressOps : OperationBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public AddressOps()
        {
        }

        public override async Task Execute(CardSavrHttpClient http, Context ctx, params object[] extra)
        {
            if (ctx.Users == null || ctx.Users.Count == 0)
            {
                log.Warn("cannot create addresses; no new users available.");
                return;
            }
            
            // create 2 addresses for each new user.
            foreach (User u in ctx.GetNewUsers())
                if (u.role == "cardholder") {
                    await CreateAddressesForUser(u, ctx.CardholderSessions[u.id ?? -1].client, ctx);
                }
        }

        public override async Task Cleanup(CardSavrHttpClient http, Context ctx)
        {
            // use to collect IDs of addresses to delete.
            List<int> addrIds = new List<int>();

            // deletes ALL addresses identified by our special "address2" value.
            int total = 0;
            Paging paging = new Paging() { PageLength = 100 };
            while (total < paging.TotalResults || paging.TotalResults < 0)
            {
                CardSavrResponse<List<Address>> result = await http.GetAddressesAsync(null, paging);
                foreach (Address addr in result.Body)
                {
                    if (addr.address2 == Context.e2e_identifier)
                    {
                        log.Info($"found address for user-id={addr.user_id}");
                        addrIds.Add(addr.id);
                    }
                }

                total += result.Body.Count;
                paging = result.Paging;
                paging.Page += 1;
            }

            log.Info($"scanned {paging.TotalResults} addresses.");

            if (addrIds.Count > 0)
            {
                await http.DeleteAddressAsync(addrIds);
            }

            log.Info($"deleted {addrIds.Count} addresses.");
        }

        private async Task CreateAddressesForUser(User user, CardSavrHttpClient http, Context ctx)
        {
            // first the primary address.
            // set address2 to our identifer so we can find and delete these later.
            PropertyBag bag = new PropertyBag();
            bag["user_id"] = user.id;
            bag["is_primary"] = true;
            bag["address1"] = $"{Context.random.Next(1000, 9000)} SDK Ave NE";
            bag["address2"] = Context.e2e_identifier;
            bag["city"] = "Seattle";
            bag["subnational"] = "Washington";
            bag["country"] = "USA";
            bag["postal_code"] = "98119";

            CardSavrResponse<Address> addr = await ctx.CardholderSessions[user.id ?? -1].client.CreateAddressAsync(bag);
            log.Info($"created primary address {addr.Body.id} for user: {user.first_name} {user.last_name} ({user.id})");

            // update it.
            bag.Clear();
            bag["address1"] = $"{Context.random.Next(1000, 9000)} CSharp-SDK Ave NE";
            await http.UpdateAddressAsync(addr.Body.id, bag);
            log.Info($"updated primary address {addr.Body.id} for user: {user.first_name} {user.last_name} ({user.id})");

            // then some other address.
            bag.Clear();
            bag["user_id"] = user.id;
            bag["is_primary"] = false;
            bag["address1"] = $"{Context.random.Next(1000, 9000)} Snowshoe Way";
            bag["address2"] = Context.e2e_identifier;
            bag["city"] = "Plain";
            bag["subnational"] = "Washington";
            bag["country"] = "USA";
            bag["postal_code"] = "98123";

            CardSavrHttpClient client = (user.role == "cardholder" ? ctx.CardholderSessions[user.id ?? -1].client : http);

            addr = await client.CreateAddressAsync(bag);
            log.Info($"created secondary address {addr.Body.id} for user: {user.first_name} {user.last_name} ({user.id})");

            // might as well update that one too.
            bag.Clear();
            bag["address1"] = $"{Context.random.Next(1000, 9000)} Cross-Country Way";
            await client.UpdateAddressAsync(addr.Body.id, bag);
            log.Info($"updated secondary address {addr.Body.id} for user: {user.first_name} {user.last_name} ({user.id})");
        }
    }
}
