using Alkami.Contracts;
using System.Runtime.Serialization;

namespace Strivve.MS.CardsavrProvider.Contracts.Requests
{
    /// <summary>
    /// The request doesn't necessarily have to define anything beyond the base request object
    /// In this case we'll add a property just for example purposes
    /// </summary>
    [DataContract(IsReference = true)]
    public class GetSettingsRequest : BaseRequest
    {
    }
}
