using Alkami.Contracts;
using Strivve.MS.CardSavrDelegate.Contracts.Requests;
using Strivve.MS.CardSavrDelegate.Contracts.Responses;
using Strivve.MS.CardSavrProviderService.Contracts.Requests;
using Strivve.MS.CardSavrProviderService.Contracts.Responses;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Strivve.MS.CardSavrProviderService.Contracts
{
    /// <summary>
    /// The IService Contract for Strivve.MS.CardSavrProviderService.Contracts
    /// </summary>
    [ServiceContract]
    public interface ICardSavrProviderServiceServiceContract
    {
        /// <summary>
        /// This is a template method that demonstrates how to get settings from Alkami's data scope abstraction
        /// It is sometimes useful to have a service's settings within a widget or another service.
        /// This service may be one part of a feature, but it's possible to store all feature settings within a single service
        /// Any other widgets or services that are a part of this feature can easily get all the settings they need by calling this method
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [OperationContract]
        Task<SettingsResponse> GetSettingsAsync(GetSettingsRequest request);

        /// <summary>
        /// Get something from a third party service or API
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [OperationContract]
        Task<CustomObjectResponse> GetDataAsync(GetSomethingRequest request);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [OperationContract]
        Task<CardholderGrantResponse> GetCardholderGrantAsync(GetCardholderGrantRequest request);
    }
}