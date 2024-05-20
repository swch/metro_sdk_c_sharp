using Alkami.Contracts;
using System.Runtime.Serialization;

namespace Strivve.MS.CardSavrProviderService.Contracts.Mappers
{
    /// <summary>
    /// TODO: add xml doc comments
    /// </summary>
    [DataContract(IsReference = true)]
    public class CustomDataObjectMapper : IMapping
    {
        /// <summary>
        /// //TODO: add xml doc comments
        /// </summary>
        [DataMember(EmitDefaultValue = true)]
        public bool? IncludeChildren { get; set; }
    }
}