using Alkami.Contracts;
using Strivve.MS.CardsavrProvider.Data;
using System.Runtime.Serialization;

namespace Strivve.MS.CardsavrProvider.Contracts.Requests
{
    /// <summary>
    /// TODO: add xml doc comments
    /// </summary>
    [DataContract(IsReference = true)]
    public class AddOrUpdateSomethingRequest : BaseCreateOrUpdateRequest<CustomDataObject>
    {

    }
}