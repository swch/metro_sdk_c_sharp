using Alkami.Contracts;
using Strivve.MS.CardsavrProvider.Contracts.Filters;
using Strivve.MS.CardsavrProvider.Contracts.Mappers;
using Strivve.MS.CardsavrProvider.Contracts.Sorters;
using System.Runtime.Serialization;

namespace Strivve.MS.CardsavrProvider.Contracts.Requests
{
    /// <summary>
    /// TODO: add xml doc comments
    /// </summary>
    [DataContract(IsReference = true)]
    public class GetSomethingRequest : BaseGetRequest<CustomDataObjectFilter, CustomDataObjectMapper, CustomDataObjectSorter>
    {

    }
}
