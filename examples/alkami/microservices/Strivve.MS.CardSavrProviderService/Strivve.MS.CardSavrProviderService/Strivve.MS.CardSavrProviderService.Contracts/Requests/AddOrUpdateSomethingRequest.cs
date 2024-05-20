using Alkami.Contracts;
using Strivve.MS.CardSavrProviderService.Data;
using System.Runtime.Serialization;

namespace Strivve.MS.CardSavrProviderService.Contracts.Requests
{
    /// <summary>
    /// TODO: add xml doc comments
    /// </summary>
    [DataContract(IsReference = true)]
    public class AddOrUpdateSomethingRequest : BaseCreateOrUpdateRequest<CustomDataObject>
    {

    }
}