using System.Runtime.Serialization;

namespace Strivve.MS.CardsavrProvider.Contracts
{
    /// <summary>
    /// TODO: add xml doc comments
    /// </summary>
    [DataContract]
    public enum CustomDataObjectFields
    {
        /// <summary>
        /// TODO: add xml doc comments
        /// </summary>
        [EnumMember]
        AnotherPropertyThatsAnInt = 0,

        /// <summary>
        /// TODO: add xml doc comments
        /// </summary>
        [EnumMember]
        OneOfYourObjects = 1
    }
}