using System.Runtime.Serialization;

namespace Strivve.MS.CardSavrProviderService.Data.ProviderSettings
{
    /// <summary>
    /// This class contains a list of setting names as string constants
    /// </summary>
    public class SettingNames
    {
        /// <summary>
        /// URL where Cardsavr instance is running (https)
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public const string CardsavrURL = "CardsavrURL";

        /// <summary>
        /// Integrator Name configured in portal
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public const string IntegratorName = "IntegratorName";

        /// <summary>
        /// Integrator Key
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public const string IntegratorKey = "IntegratorKey";

        /// <summary>
        /// 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public const string CustomerAgentUsername = "CustomerAgentUsername";

        /// <summary>
        /// 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public const string CustomerAgentPassword = "CustomerAgentPassword";
    }
}
