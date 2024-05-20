using Alkami.Contracts;
using Alkami.Data.Validations;
using Alkami.MicroServices.Settings.ProviderBasedService;
using Strivve.MS.CardsavrProvider.Contracts;
using Strivve.MS.CardsavrProvider.Contracts.Requests;
using Strivve.MS.CardsavrProvider.Contracts.Responses;
using Strivve.MS.CardsavrProvider.Data.Validations;
using System;
using System.Threading.Tasks;

namespace Strivve.MS.CardsavrProvider.Service.Host
{
    public class DistributedService : ProviderBasedService<ICardsavrProviderServiceContract, ServiceImp>, ICardsavrProviderServiceContract
    {
        private ICardsavrProviderServiceContract _serviceContract; //This doesn't need to be injected, since the service will ALWAYS run the instance
        private readonly string _providerName;
        private readonly string _providerType;

        /// <summary>
        /// The new provider based service will require additional parameters during construction
        /// </summary>
        /// <param name="friendlyName"></param>
        /// <param name="providerName"></param>
        /// <param name="providerType"></param>
        public DistributedService(string friendlyName, string providerName, string providerType) : base(friendlyName, providerName, providerType)
        {
            _providerName = providerName;
            _providerType = providerType;
        }

        public void OnStart()
        {
            _serviceContract = new ServiceImp(_providerType, _providerName);

            // TODO: Add validators here.
            EntityValidator.AddValidator(new AddOrUpdateSomethingRequestValidator());
            EntityValidator.AddValidator(new GetSomethingRequestValidator());
            EntityValidator.AddValidator(new CustomDataObjectFilterValidator());
            EntityValidator.AddValidator(new CustomDataObjectValidator());

            Alkami.Broker.ZeroMq.Setup.PublishUsingZeroMqLocally();
            Alkami.Broker.ZeroMq.Setup.SubscribeUsingZeroMqLocally(_serviceCancellationToken.Token);
            Alkami.Broker.App.Subscription.InitializeSubscriber();

            base.Start();
        }

        public void OnStop(TimeSpan fromSeconds)
        {
            base.Stop(fromSeconds);
        }

        /// <inheritdoc />
        public Task<SettingsResponse> GetSettingsAsync(GetSettingsRequest request)
        {
            return _serviceContract.GetSettingsAsync(request);
        }

        /// <inheritdoc />
        public Task<CustomObjectResponse> GetDataAsync(GetSomethingRequest request)
        {
            return _serviceContract.GetDataAsync(request);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Task<CardholderGrantResponse> GetCardholderGrantAsync(GetCardholderGrantRequest request)
        {
            return _serviceContract.GetCardholderGrantAsync(request);
        }
    }
}