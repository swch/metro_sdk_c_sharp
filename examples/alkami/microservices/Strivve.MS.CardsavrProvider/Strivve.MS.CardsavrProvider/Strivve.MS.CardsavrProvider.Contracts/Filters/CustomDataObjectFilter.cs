using Alkami.Contracts;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Strivve.MS.CardsavrProvider.Contracts.Filters
{
    /// <summary>
    /// TODO: add xml doc comments
    /// </summary>
    [DataContract(IsReference = true)]
    public class CustomDataObjectFilter : IFilter
    {
        /// <summary>
        /// TODO: add xml doc comments
        /// </summary>
        [DataMember(EmitDefaultValue = true)]
        public List<long> Ids { get; set; }

        /// <summary>
        /// TODO: add xml doc comments
        /// </summary>
        [DataMember(EmitDefaultValue = true)]
        public string PartialOneOfYourObjects { get; set; }
    }
}