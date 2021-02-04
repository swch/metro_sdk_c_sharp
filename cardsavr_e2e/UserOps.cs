using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Net.Http;

using Switch.CardSavr.Http;
using Switch.Security;

namespace cardsavr_e2e
{
    public class UserOps : OperationBase
    {
        protected static readonly log4net.ILog log = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static async Task GetExecutingUserRole(CardSavrHttpClient http, Context ctx)
        {
            if (ctx.ExecutionRole != null)
            {
                log.Info($"role of username={Context.accountCustomerAgentUserName} already found: \"{ctx.ExecutionRole}\"");
                return;
            }

            // retrieve the user and get the role.
            CardSavrResponse<List<User>> lst = await http.GetUsersAsync(new NameValueCollection()
            {
                { "username", Context.accountCustomerAgentUserName }
            });

            if (lst.Body.Count != 1)
            {
                log.Error($"found {lst.Body.Count} users for username={Context.accountCustomerAgentUserName}; expected 1.");
                throw new ArgumentException(Context.accountCustomerAgentUserName);
            }

            ctx.ExecutionRole = lst.Body[0].role;
            log.Info($"execution role of username={lst.Body[0].username} is \"{lst.Body[0].role}\"");
        }

        public UserOps()
        {
        }

        public override async Task Execute(CardSavrHttpClient http, Context ctx, params object[] extra)
        {
            int count = extra.Length > 0 ? (int)extra[0] : 4;
            // create users.
            PropertyBag bag = new PropertyBag();
            string password = "";
            for (int n = 0; n < count; ++n)
            {
                // generate using an easily reproducible safe-key.
                // the username, role and email help us identify these users later.
                bag["username"] = $"{Context.e2e_identifier}_{Context.random.Next(100)}_{n}";
                password = $"{Context.e2e_identifier}_{Context.random.Next(100)}_{n}";
                bag["password"] = password; //cardholers don't have passwords
                bag["role"] = "customer_agent";
                bag["first_name"] = $"Otto_{n}";
                bag["last_name"] = $"Matic_{n}";
                bag["email"] = $"cardsavr_e2e_{Context.random.Next(100)}@gmail.com";
                bag["phone_number"] = $"206-555-{n}{n + 1}{n + 2}{n + 3}".Substring(0, 12);
                string safe_key = Aes256.GetRandomString(30);
                log.Info(bag["username"] + " " + bag["password"]);
                CardSavrResponse<User> result = await http.CreateUserAsync(bag, safe_key, "default");

                Context.CardholderData chd = new Context.CardholderData();
                chd.cardholder_safe_key = safe_key;
            }

            CardsavrHelper helper = new CardsavrHelper();
            helper.SetAppSettings(Context.accountBaseUrl, Context.accountCustomerAgentAppID, Context.accountCustomerAgentStaticKey, Context.rejectUnauthorized);
            string testing_agent = (string)bag["username"];
            await helper.LoginAndCreateSession(testing_agent, password);
            string new_password = await helper.RotateAgentPassword(testing_agent);
            log.Info("New Password: " + new_password);
            await helper.CloseSession(testing_agent);
            await helper.LoginAndCreateSession(testing_agent, new_password);

            // get a list of all users, including the ones we just created.
            await GetAllUsers(http, ctx, 100);
            // log what we just did.
            foreach (User user in ctx.Users)
                log.Info($"created/updated user: {user.username} = \"{user.first_name} {user.last_name}\"");

            List<User> newUsers = ctx.GetNewUsers();
            // update them. we'll just change the phone number.
            for (int n = 0; n < newUsers.Count; ++n)
            {
                bag.Clear();
                bag["id"] = newUsers[n].id;
                bag["phone_number"] = $"206-555-{n + 3}{n + 2}{n + 1}{n}".Substring(0, 12);
                await http.UpdateUserAsync(bag.GetString("id"), bag);
            }

            User u = newUsers[Context.random.Next(0, newUsers.Count - 1)];
            // update the password for just one of the users.
            await UpdateUserPassword(http, ctx, u);

            

        }

        public override async Task Cleanup(CardSavrHttpClient http, Context ctx)
        {
            if (ctx.Users == null)
            {
                log.Warn("no user list available; cannot scan for users to delete.");
                return;
            }

            foreach (User u in ctx.GetNewUsers()) {
                log.Info($"deleting user: {u.username}");
                await http.DeleteUserAsync(u.id);
            }

            CardSavrResponse<List<Cardholder>> result = await http.GetCardholdersAsync(null);
            log.Info(String.Format("{0} cardholders returned", result.Body.Count));
            if (result.Body.Count > 0)
            {
                foreach (Cardholder u in result.Body) {
                    if (u.cuid.StartsWith(Context.e2e_identifier)) {
                        log.Info("Delete cardholder: " + u.cuid);
                        await http.DeleteCardholderAsync(u.id);
                    }
                }
            }

            ctx.Users.Clear();
            ctx.CardholderSessions.Clear();
            ctx.Cardholders.Clear();
        }

        protected async Task GetAllUsers(CardSavrHttpClient http, Context ctx, int pageLength = 7)
        {
            log.Info("retrieving users...");
            Paging p = new Paging(1, pageLength);
            List<User> users = new List<User>();

            while (users.Count < p.TotalResults || p.TotalResults < 0)
            {
                log.Info($"getting page {p.Page}");

                CardSavrResponse<List<User>> result = await http.GetUsersAsync(null, p);
                log.Info(String.Format("{0} users returned", result.Body.Count));

                if (result.Body.Count > 0)
                {
                    users.AddRange(result.Body);

                    foreach (User u in result.Body)
                        log.Info(String.Format("{0} = \"{1} {2}\" {3}", u.username, u.first_name, u.last_name, u.role));
                }

                p = result.Paging;
                p.Page += 1;
            }

            log.Info($"retrieved a total of {users.Count} users");
            ctx.Users = users;
        }

        private async Task UpdateUserPassword(CardSavrHttpClient http, Context ctx, User user)
        {
            log.Info($"updating password for user \"{user.username}\" ({user.id})");

            string pwBase = "foobar";
            string newPassword = pwBase.PadRight(44 - pwBase.Length);
            PropertyBag bag = new PropertyBag();
            bag["username"] = user.username;
            bag["old_password"] = Context.GenerateBogus32BitPassword(Context.e2e_identifier);
            bag["password"] = Context.GenerateBogus32BitPassword(user.username);
            bag["password_copy"] = Context.GenerateBogus32BitPassword(user.username);

            CardSavrResponse<PropertyBag> result = await http.UpdateUserPasswordAsync((int)user.id, bag);
            log.Info($"password update successful for user \"{user.username}\" ({user.id})");
        }
    }
}