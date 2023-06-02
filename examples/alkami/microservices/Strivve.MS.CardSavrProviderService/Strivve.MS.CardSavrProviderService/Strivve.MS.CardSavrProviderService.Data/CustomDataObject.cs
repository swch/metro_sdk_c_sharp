using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Strivve.MS.CardSavrProviderService.Data
{
    /// <summary>
    /// TODO: add xml doc comments
    /// </summary>
    [DataContract(IsReference = true)]
    public class CustomDataObject
    {
        /// <summary>
        /// TODO: add xml doc comments
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string OneOfYourObjects { get; set; }

        /// <summary>
        /// TODO: add xml doc comments
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int AnotherPropertyThatsAnInt { get; set; }

        /// <summary>
        /// TODO: add xml doc comments
        /// </summary>
        [DataMember(EmitDefaultValue = true)]
        public List<CustomChildObject> ChildrenObjects { get; set; }
    }
}