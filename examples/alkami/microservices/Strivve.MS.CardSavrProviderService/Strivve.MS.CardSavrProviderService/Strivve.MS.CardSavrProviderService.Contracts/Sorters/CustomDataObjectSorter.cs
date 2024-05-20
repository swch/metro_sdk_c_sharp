using Alkami.Contracts;
using Strivve.MS.CardSavrProviderService.Contracts;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Strivve.MS.CardSavrProviderService.Contracts.Sorters
{
    /// <summary>
    /// TODO: add xml doc comments
    /// </summary>
    [DataContract(IsReference = true)]
    public class CustomDataObjectSorter : ISortOrder<CustomDataObjectFields>
    {
        /// <inheritdoc />
        [DataMember(EmitDefaultValue = false)]
        public bool Ascending { get; set; }

        /// <inheritdoc />
        [DataMember(EmitDefaultValue = false)]
        public List<CustomDataObjectFields> OrderByFields { get; set; }
    }
}