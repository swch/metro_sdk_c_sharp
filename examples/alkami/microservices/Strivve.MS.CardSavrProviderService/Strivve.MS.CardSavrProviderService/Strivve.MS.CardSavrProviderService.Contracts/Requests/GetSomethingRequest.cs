using Alkami.Contracts;
using Strivve.MS.CardSavrProviderService.Contracts.Filters;
using Strivve.MS.CardSavrProviderService.Contracts.Mappers;
using Strivve.MS.CardSavrProviderService.Contracts.Sorters;
using System.Runtime.Serialization;

namespace Strivve.MS.CardSavrProviderService.Contracts.Requests
{
    /// <summary>
    /// TODO: add xml doc comments
    /// </summary>
    [DataContract(IsReference = true)]
    public class GetSomethingRequest : BaseGetRequest<CustomDataObjectFilter, CustomDataObjectMapper, CustomDataObjectSorter>
    {

    }
}
