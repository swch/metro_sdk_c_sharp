using Alkami.Contracts;
using Strivve.MS.CardSavrProviderService.Data.ProviderSettings;
using System.Runtime.Serialization;

namespace Strivve.MS.CardSavrProviderService.Contracts.Responses
{
    /// <summary>
    /// Our response object inherits a base object that takes a type argument of the data we want to return
    /// We can add additional properties to this class that may be helpful, it's up to the discretion of the developer
    /// </summary>
    [DataContract(IsReference = true)]
    public class SettingsResponse : BaseResponse<Setting>
    {
    }
}
