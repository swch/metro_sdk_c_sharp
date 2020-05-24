using System;
using System.Threading.Tasks;

using Switch.CardSavr.Http;


namespace cardsavr_e2e
{
    /// <summary>
    /// This program is for exercising/testing the HTTP API.
    /// </summary>
    class Program
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static CardSavrHttpClient _http = new CardSavrHttpClient();
        private static Context _context = new Context();

        private class WaitOp: OperationBase
        {
            private int _seconds = 60;

            public WaitOp(int seconds)
            {
                _seconds = seconds;
            }

            public override async Task Execute(CardSavrHttpClient http, Context ctx, params object[] extra)
            {
                log.Info($"waiting for {_seconds} seconds.");
                await Task.Delay(_seconds * 1000);
            }
        }

        static async Task<CardSavrResponse<LoginResult>> StartSession(bool cardholder_agent = false)
        {
            log.Info("starting session....");
            log.Info(Context.accountAppID);
            int idx = cardholder_agent ? 1 : 0;
            _http.Setup(Context.accountBaseUrl, Context.accountStaticKey[idx],
                Context.accountAppID[idx], Context.accountUserName[idx], Context.accountPassword[idx]);
            log.Info("logging in...");
            CardSavrResponse<LoginResult> login = await _http.Init();
            _context.Started = true;
            return login;
        }

        static async Task EndSession()
        {
            if (!_context.Started)
            {
                log.Info("session not started, nothing to end.");
            }
            else
            {
                log.Info("ending session...");
                await _http.EndAsync();
                log.Info("end session complete.");
                _context.Started = false;
            }
        }

        static async Task ExecuteOps(OperationBase[] ops)
        {
            log.Info("STARTING OPERATION EXECUTION:");

            foreach (OperationBase op in ops)
                await op.Execute(_http, _context);

            log.Info("FINISHED OPERATION EXECUTION.");
        }

        static async Task CleanupOps(OperationBase[] ops)
        {
            log.Info("STARTING OPERATION CLEANUP:");

            // reverse order.
            for (int n = ops.Length - 1; n >= 0; --n)
            {
                OperationBase op = ops[n];
                try
                {
                    await op.Cleanup(_http, _context);
                }
                catch (Exception ex)
                {
                    log.Error("cleanup", ex);
                }
            }

            log.Info("FINISHED OPERATION CLEANUP.");
        }

        static void Main(string[] args)
        {
            Exception exLast = null;
            log.Info("HELLO WORLD!");

            OperationBase[] ops_ch = new OperationBase[]
            {
                new CardholderOps()
            };

            // order can be important. some operations depend on other resources having been 
            // created previosly (and stored in the context).
            OperationBase[] ops = new OperationBase[]
            {
                // these have no dependencies.
                new MerchantSiteOps(),
                //new BinOps(),
                //new IntegratorOps(),

                new UserOps(),
                
                // addresses depends on: users
                new AddressOps(),

                // cards depends on: users, addresses, bins, and merchant sites.
                new CardOps(),

                // acounts depends on: users and merchant sites.
                new AccountOps(),
                //new JobOps()

            };

            try
            {
                StartSession(true).Wait();
                ExecuteOps(ops_ch).Wait();
            }
            catch (Exception ex)
            {
                // eat the exception for now. we'll log it again at exit cause otherwise it
                // can get lost in all of the log output.
                log.Error("an operation failed", ex);
                exLast = ex;
            }
            finally
            {
                CleanupOps(ops_ch).Wait(); 
                EndSession().Wait();
            }
            _http = new CardSavrHttpClient();
            try
            {
                StartSession().Wait();
                ExecuteOps(ops).Wait();
            }
            catch (Exception ex)
            {
                log.Error("an operation failed", ex);
                exLast = ex;
            }
            finally
            {
                CleanupOps(ops).Wait(); 
                EndSession().Wait();
            }

            if (exLast != null)
                log.Error("EXCEPTION DURING EXECUTION", exLast);               

            log.Info("finished. press any key.");
        }
    }
}
