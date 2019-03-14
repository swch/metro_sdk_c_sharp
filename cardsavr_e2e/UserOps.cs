using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;

using Switch.CardSavr.Http;

namespace cardsavr_e2e
{
    public class UserOps : OperationBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static async Task GetExecutingUserRole(CardSavrHttpClient http, Context ctx)
        {
            if (ctx.ExecutionRole != null)
            {
                log.Info($"role of username={Context.accountUserName} already found: \"{ctx.ExecutionRole}\"");
                return;
            }

            // retrieve the user and get the role.
            CardSavrResponse<List<User>> lst = await http.GetUsersAsync(new NameValueCollection()
            {
                { "username", Context.accountUserName }
            });

            if (lst.Body.Count != 1)
            {
                log.Error($"found {lst.Body.Count} users for username={Context.accountUserName}; expected 1.");
                throw new ArgumentException(Context.accountUserName);
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
            List<User> newUsers = new List<User>();

            // create users.
            PropertyBag bag = new PropertyBag();
            for (int n = 0; n < count; ++n)
            {
                // generate using an easily reproducible safe-key.
                // the username, role and email help us identify these users later.
                bag["username"] = $"{Context.e2e_identifier}_{Context.random.Next(100)}_{n}";
                bag["password"] = Context.GenerateBogus32BitPassword(Context.e2e_identifier);
                bag["cardholder_safe_key"] = Context.GenerateBogus32BitPassword(bag.GetString("username"));
                bag["role"] = "cardholder";
                bag["first_name"] = $"Otto_{n}";
                bag["last_name"] = $"Matic_{n}";
                bag["email"] = $"cardsavr_e2e_{Context.random.Next(100)}@gmail.com";
                bag["phone_number"] = $"206-555-{n}{n + 1}{n + 2}{n + 3}";

                CardSavrResponse<User> result = await http.CreateUserAsync(bag);
                newUsers.Add(result.Body);
            }

            // update them. we'll just change the phone number.
            for (int n = 0; n < newUsers.Count; ++n)
            {
                bag.Clear();
                bag["id"] = newUsers[n].id;
                bag["phone_number"] = $"206-555-{n + 3}{n + 2}{n + 1}{n}";
                await http.UpdateUserAsync(bag.GetString("id"), bag);
            }

            // log what we just did.
            foreach (User user in newUsers)
                log.Info($"created/updated user: {user.username} = \"{user.first_name} {user.last_name}\"");

            // update the password for just one of the users.
            await UpdateUserPassword(http, ctx, newUsers[Context.random.Next(0, newUsers.Count - 1)]);

            // store the users we created for other code to use.
            ctx.NewUsers = newUsers;

            // get a list of all users, including the ones we just created.
            await GetAllUsers(http, ctx, 100);
        }

        public override async Task Cleanup(CardSavrHttpClient http, Context ctx)
        {
            if (ctx.Users == null)
            {
                log.Warn("no user list available; cannot scan for users to delete.");
                return;
            }

            int count = 0;
            int n = ctx.Users.FindIndex(
                user => user.username.StartsWith(Context.e2e_identifier, StringComparison.CurrentCulture));
            while (n >= 0)
            {
                User u = ctx.Users[n];
                log.Info($"deleting user: {u.username}");
                await http.DeleteUserAsync(u.id);
                ctx.Users.RemoveAt(n);
                count++;

                n = ctx.Users.FindIndex(
                    user => user.username.StartsWith(Context.e2e_identifier, StringComparison.CurrentCulture));
            }

            // all of the users we created should be history now.
            log.Info($"deleted {count} users.");
            if (ctx.NewUsers != null)
                ctx.NewUsers.Clear();
        }

        private async Task GetAllUsers(CardSavrHttpClient http, Context ctx, int pageLength = 7)
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

                    /* echo users to the log.
                    foreach (User u in result.Body)
                        log.Info(String.Format("{0} = \"{1} {2}\"", u.username, u.first_name, u.last_name));
                    */                       
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
            bag["old_password"] = Context.GenerateBogus32BitPassword(Context.e2e_identifier);
            bag["password"] = Context.GenerateBogus32BitPassword(user.username);
            bag["password_copy"] = Context.GenerateBogus32BitPassword(user.username);

            CardSavrResponse<PropertyBag> result = await http.UpdateUserPasswordAsync((int)user.id, bag);
            log.Info($"password update successful for user \"{user.username}\" ({user.id})");
        }
    }
}