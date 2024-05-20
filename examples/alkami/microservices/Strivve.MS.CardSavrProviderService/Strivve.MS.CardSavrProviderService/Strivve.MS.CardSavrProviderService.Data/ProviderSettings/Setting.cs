using System.Runtime.Serialization;

namespace Strivve.MS.CardSavrProviderService.Data.ProviderSettings
{
    /// <summary>
    /// This class is used to represent the setting details of any given provider setting
    /// </summary>
    [DataContract(IsReference = false)]
    public class Setting
    {
        /// <summary>
        /// The "key" name of the setting
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// The current active value of the setting
        /// </summary>
        [DataMember]
        public string CurrentValue { get; set; }

        /// <summary>
        /// The default value that is initially configured
        /// </summary>
        [DataMember]
        public string DefaultValue { get; set; }

        /// <summary>
        /// A descriptive summary of this setting and it's meaning
        /// </summary>
        [DataMember]
        public string Description { get; set; }
    }
}
