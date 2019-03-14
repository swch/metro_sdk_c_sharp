using System;
using System.Threading.Tasks;

using Switch.CardSavr.Http;


namespace cardsavr_e2e
{
    public abstract class OperationBase
    {
        protected OperationBase()
        {
        }

        #pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public virtual async Task Execute(CardSavrHttpClient http, Context ctx, params object[] extra) { }
        public virtual async Task Cleanup(CardSavrHttpClient http, Context ctx) { }
        #pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    }
}
