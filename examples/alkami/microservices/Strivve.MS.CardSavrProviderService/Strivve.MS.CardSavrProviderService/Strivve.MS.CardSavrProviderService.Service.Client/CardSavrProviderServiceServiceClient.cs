using Alkami.Contracts;
using Alkami.MicroServices.Settings.ProviderBasedClient;
using Strivve.MS.CardSavrDelegate.Contracts.Requests;
using Strivve.MS.CardSavrDelegate.Contracts.Responses;
using Strivve.MS.CardSavrProviderService.Contracts;
using Strivve.MS.CardSavrProviderService.Contracts.Requests;
using Strivve.MS.CardSavrProviderService.Contracts.Responses;
using System.Threading.Tasks;

namespace Strivve.MS.CardSavrProviderService.Service.Client
{
    /// <inheritdoc />
    public class CardSavrProviderServiceServiceClient : ProviderBasedClient<ICardSavrProviderServiceServiceContract>, ICardSavrProviderServiceServiceContract
    {
        /// <summary>
        /// The unique provider type designator for this service
        /// </summary>
        private const string ProviderType = "Strivve";

        /// <summary>
        ///  ProviderBased constructor
        /// </summary>
        public CardSavrProviderServiceServiceClient() : base(ProviderType)
        { }

        /// <summary>
        /// ProviderBased constructor override
        /// </summary>
        /// <param name="providerId"></param>
        public CardSavrProviderServiceServiceClient(long providerId) : base(ProviderType, providerId)
        { }

        /// <inheritdoc />
        public Task<SettingsResponse> GetSettingsAsync(GetSettingsRequest request)
        {
            return ProxyCall((operation, inner) => operation.GetSettingsAsync(inner), request);
        }

        /// <inheritdoc />
        public Task<CustomObjectResponse> GetDataAsync(GetSomethingRequest request)
        {
            return ProxyCall((operation, inner) => operation.GetDataAsync(inner), request);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Task<CardholderGrantResponse> GetCardholderGrantAsync(GetCardholderGrantRequest request)
        {
            return ProxyCall((operation, inner) => operation.GetCardholderGrantAsync(inner), request);

        }
    }
}