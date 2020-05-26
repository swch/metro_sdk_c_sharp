using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;

using Switch.CardSavr.Http;


namespace cardsavr_e2e
{
    public class CardOps : OperationBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public CardOps()
        {
        }

        public override async Task Execute(CardSavrHttpClient http, Context ctx, params object[] extra)
        {
            // get all addresses for the user (there should be at least 2).
            User user = ctx.GetNewUsers("cardholder")[0]; //first user
            CardSavrResponse<List<Address>> addrs = await http.GetAddressesAsync(new NameValueCollection() {
                { "user_ids", user.id.ToString() }
            });

            if (addrs.Body.Count < 2)
                throw new ArgumentOutOfRangeException($"only {addrs.Body.Count} addresses for user \"{user.username}\"; must be >= 2");

            log.Info($"got {addrs.Body.Count} addresss for user \"{user.username}\"");

            //Grab the session for this cardholder
           CardSavrHttpClient cardholder = ctx.CardholderSessions[user.id ?? -1].client;

            // the card we create uses our (possibly truncated) special identifier as the color
            // so we can identify it later if needed.
            DateTime expire = DateTime.Now.AddYears(1);
            PropertyBag body = new PropertyBag()
            {
                { "cardholder_id", user.id },
                { "address_id", addrs.Body[0].id },
                { "pan", "4111111111111111" },
                { "par", GenerateBogusPAR() },
                { "cvv", 345 },
                { "first_name", user.first_name },
                { "last_name", user.last_name },
                { "name_on_card", "BOGUS CARD" },
                { "expiration_month", expire.Month.ToString() },
                { "expiration_year", (expire.Year % 2000).ToString() }
            };

            // our test users have a known safe-key.
            string safeKey = Context.GenerateBogus32BitPassword(user.username);
            CardSavrResponse<Card> card = await cardholder.CreateCardAsync(body, safeKey);
            log.Info($"created card-id={card.Body.id}");

            // update it: just change the address.
            body.Clear();
            body.Add("id", card.Body.id);
            body.Add("address_id", addrs.Body[0].id);
            CardSavrResponse<List<Card>> upd = await cardholder.UpdateCardAsync(null, body);
            log.Info($"update card for user \"{user.username}\"");

            ctx.CardholderSessions[user.id ?? -1].cards = upd.Body;
        }

        public override async Task Cleanup(CardSavrHttpClient http, Context ctx)
        {
            // deletes ALL cards identified by our special "card_color" value.
            List<Card> cardsToDel = new List<Card>();
            string specialColor = Context.e2e_identifier.Substring(0, 8);

            int total = 0;
            Paging paging = new Paging() { PageLength = 100 };
            User user = ctx.GetNewUsers("cardholder")[0];  //first cardholder
            CardSavrHttpClient cardholder = ctx.CardholderSessions[user.id ?? -1].client;
            log.Info($"delete cards for user \"{user.username}\"");
            while (total < paging.TotalResults || paging.TotalResults < 0)
            {
                CardSavrResponse<List<Card>> result = await cardholder.GetCardsAsync(null, paging);
                foreach (Card c in result.Body)
                {
                    if (c.name_on_card == "BOGUS CARD")
                        cardsToDel.Add(c);
                }

                total += result.Body.Count;
                paging = result.Paging;
                paging.Page += 1;
            }

            log.Info($"scanned {paging.TotalResults} cards, found {cardsToDel.Count}.");

            int deleted = 0;
            foreach (Card c in cardsToDel)
            {
                user = ctx.FindUserById(c.cardholder_id);
                if (user == null)
                {
                    log.Error($"UNABLE TO DELETE CARD: no user found for card-id={c.id}");
                }
                else
                {
                    // our test users have a known safe-key.
                    string safeKey = Context.GenerateBogus32BitPassword(user.username);
                    await http.DeleteCardAsync(c.id, safeKey);
                    ++deleted;
                    log.Info($"deleted card-id={c.id} for user-id={user.id}");
                }
            }

            log.Info($"deleted {deleted} cards.");
        }

        private string GenerateBogusPAR()
        {
            string digits = "0123456789";
            string letters = "abcdefghijklmnopqrstuvwxyz";
            string both = letters + digits;

            string par = "";
            par += letters[Context.random.Next() % letters.Length];
            par += digits[Context.random.Next() % digits.Length];
            par += letters[Context.random.Next() % letters.Length];
            par += digits[Context.random.Next() % digits.Length];

            int remainder = 29 - par.Length;
            for (; remainder > 0; --remainder)
                par += both[Context.random.Next() % both.Length];

            return par.ToUpper();
        }
    }
}