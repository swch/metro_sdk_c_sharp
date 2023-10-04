using System;
using System.Runtime.Serialization;

namespace Strivve.MS.CardsavrProvider.Data
{
    /// <summary>
    /// TODO: add xml doc comments
    /// </summary>
    [DataContract(IsReference = true)]
    public class CustomChildObject
    {
        /// <summary>
        /// TODO: add xml doc comments
        /// </summary>
        [DataMember(EmitDefaultValue = true)]
        public DateTime DateTime { get; set; }

        /// <summary>
        /// TODO: add xml doc comments
        /// </summary>
        [DataMember(EmitDefaultValue = true)]
        public string String { get; set; }

        /// <summary>
        /// TODO: add xml doc comments
        /// </summary>
        [DataMember(EmitDefaultValue = true)]
        public int Int { get; set; }
    }
}