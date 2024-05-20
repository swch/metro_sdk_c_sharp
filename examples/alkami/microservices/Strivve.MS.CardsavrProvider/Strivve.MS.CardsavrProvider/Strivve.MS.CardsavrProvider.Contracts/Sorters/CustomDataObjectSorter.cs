using Alkami.Contracts;
using Strivve.MS.CardsavrProvider.Contracts;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Strivve.MS.CardsavrProvider.Contracts.Sorters
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