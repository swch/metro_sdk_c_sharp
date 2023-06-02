using Alkami.Contracts;
using System.Runtime.Serialization;

namespace Strivve.MS.CardSavrProviderService.Contracts.Requests
{
    /// <summary>
    /// TODO: add xml doc comments
    /// </summary>
    [DataContract(IsReference = true)]
    public class InitateSomethingRequest : BaseRequest
    {
        //Command Message Pattern here... returns nothing.. maybe a status
    }
}