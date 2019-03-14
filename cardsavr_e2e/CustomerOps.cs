using System;
using System.Threading.Tasks;

using Switch.CardSavr.Http;


namespace cardsavr_e2e
{
    public class CustomerOps: OperationBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public CustomerOps()
        {
        }

        public override async Task Execute(CardSavrHttpClient http, Context ctx, params object[] extra)
        {
            // not implemented. call base class to silence the compiler.
            await base.Execute(http, ctx, extra);
        }

        public override async Task Cleanup(CardSavrHttpClient http, Context ctx)
        {
            // not implemented. call base class to silence the compiler.
            await base.Cleanup(http, ctx);
        }
    }
}
