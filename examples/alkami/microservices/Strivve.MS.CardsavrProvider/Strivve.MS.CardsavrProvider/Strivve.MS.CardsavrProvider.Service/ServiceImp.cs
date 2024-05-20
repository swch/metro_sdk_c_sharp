using Alkami.Contracts;
using Alkami.Data.Validations;
using Common.Logging;
using Strivve.MS.CardsavrProvider.Contracts;
using Strivve.MS.CardsavrProvider.Contracts.Requests;
using Strivve.MS.CardsavrProvider.Contracts.Responses;
using Strivve.MS.CardsavrProvider.Data;
using Strivve.MS.CardsavrProvider.Data.ProviderSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Switch.CardSavr.Http;

namespace Strivve.MS.CardsavrProvider.Service
{
    /// <inheritdoc />
    public partial class ServiceImp : ICardsavrProviderServiceContract
    {
        private static readonly ILog Logger = LogManager.GetLogger<ServiceImp>();

        /// <inheritdoc />
        public async Task<SettingsResponse> GetSettingsAsync(GetSettingsRequest request)
        {
            // Create a new response object that encapsulates the data type we'll be returning
            var response = new SettingsResponse();

            // We want some local variables that are available outside of the data scope
            string cardsavrUrl = string.Empty;
            string integratorName = string.Empty;
            string integratorKey = string.Empty;
            string custAgentUsername = string.Empty;
            string custAgentPassword = string.Empty;
            string cardupdatrAppUrl = string.Empty;

            // GetScopeAsync() is how we retrieve settings using the request type of this service
            using (var scope = await GetScopeAsync(request))
            {
                // Assigning the settings to our local variables
                cardsavrUrl = scope.GetSettingOrDefault<string>(SettingNames.CardsavrURL);
                integratorName = scope.GetSettingOrDefault<string>(SettingNames.IntegratorName);
                integratorKey = scope.GetSettingOrDefault<string>(SettingNames.IntegratorKey);
                custAgentUsername = scope.GetSettingOrDefault<string>(SettingNames.CustomerAgentUsername);
                custAgentPassword = scope.GetSettingOrDefault<string>(SettingNames.CustomerAgentPassword);
                cardupdatrAppUrl = scope.GetSettingOrDefault<string>(SettingNames.CardupdatrAppURL);
            }

            // It's always good to add a trace log for future troubleshooting
            Logger.Trace($"{nameof(GetSettingsAsync)} | Cardsavr URL: [{cardsavrUrl}] | Integrator Name: [{cardsavrUrl}] | Integrator Key : [{integratorKey}] | Customer Agent Username : [{custAgentUsername}] | Cardupdatr App URL: [{cardupdatrAppUrl}]");

            // Populate the details of the Setting object with this service's two template settings
            // The response object's "ItemList" property is an enumerable list of the type we passed into the class definition of SettingsResponse
            response.ItemList.Add(new Setting
            {
                Name = SettingNames.CardsavrURL,
                DefaultValue = DefaultSettings()[SettingNames.CardsavrURL],
                CurrentValue = cardsavrUrl,
                Description = SettingDescriptors().FirstOrDefault(x => x.Name == SettingNames.CardsavrURL)?.Description
            });

            // We'll do the same for the second setting, adding another instance of the Setting to the response's ItemList
            response.ItemList.Add(new Setting
            {
                Name = SettingNames.IntegratorName,
                DefaultValue = DefaultSettings()[SettingNames.IntegratorName],
                CurrentValue = integratorName,
                Description = SettingDescriptors().FirstOrDefault(x => x.Name == SettingNames.IntegratorName)?.Description
            });

            response.ItemList.Add(new Setting
            {
                Name = SettingNames.IntegratorKey,
                DefaultValue = DefaultSettings()[SettingNames.IntegratorKey],
                CurrentValue = integratorKey,
                Description = SettingDescriptors().FirstOrDefault(x => x.Name == SettingNames.IntegratorKey)?.Description
            });

            response.ItemList.Add(new Setting
            {
                Name = SettingNames.CustomerAgentUsername,
                DefaultValue = DefaultSettings()[SettingNames.CustomerAgentUsername],
                CurrentValue = custAgentUsername,
                Description = SettingDescriptors().FirstOrDefault(x => x.Name == SettingNames.CustomerAgentUsername)?.Description
            });

            response.ItemList.Add(new Setting
            {
                Name = SettingNames.CustomerAgentPassword,
                DefaultValue = DefaultSettings()[SettingNames.CustomerAgentPassword],
                CurrentValue = custAgentPassword,
                Description = SettingDescriptors().FirstOrDefault(x => x.Name == SettingNames.CustomerAgentPassword)?.Description
            });

            response.ItemList.Add(new Setting
            {
                Name = SettingNames.CardupdatrAppURL,
                DefaultValue = DefaultSettings()[SettingNames.CardupdatrAppURL],
                CurrentValue = cardupdatrAppUrl,
                Description = SettingDescriptors().FirstOrDefault(x => x.Name == SettingNames.CardupdatrAppURL)?.Description
            });


            // Return the response using an awaited task
            return await Task.FromResult(response);
        }

        /// <summary>
        /// We can get something from the web
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<CustomObjectResponse> GetDataAsync(GetSomethingRequest request)
        {
            // A service GET is going to retrieve some data from some other source and populate a response
            // In this case we have a CustomObjectResponse that contains an ItemList of CustomDataObjects
            // For the sake of demonstration a response has been populated with constants and returned to the caller
            return await Task.FromResult(new CustomObjectResponse
            {
                ItemList = new List<CustomDataObject>
                {
                    new CustomDataObject
                    {
                        AnotherPropertyThatsAnInt = 9999,
                        ChildrenObjects = new List<CustomChildObject>
                        {
                            new CustomChildObject
                            {
                                DateTime = DateTime.Now,
                                Int = 007,
                                String = "James Bond"
                            },
                            new CustomChildObject
                            {
                                DateTime = DateTime.Today,
                                Int = 006,
                                String = "Alec Trevelyn"
                            }
                        },
                        OneOfYourObjects = "MI6"
                    }
                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<CardholderGrantResponse> GetCardholderGrantAsync(GetCardholderGrantRequest request)
        {
            // first get the settings
            GetSettingsRequest settingsRequest = new GetSettingsRequest();
            settingsRequest.CopyBaseFrom(request);

            SettingsResponse settings = await GetSettingsAsync(settingsRequest);
            List<Setting> items = settings.ItemList;
            string cardsavrUrl = null;
            string integratorName = null;
            string integratorKey = null;
            string custAgentUsername = null;
            string custAgentPassword = null;

            items.ForEach(x => {
                if (x.Name.Equals(SettingNames.CardsavrURL))
                {
                    cardsavrUrl = x.CurrentValue;
                }
                else if (x.Name.Equals(SettingNames.IntegratorName))
                {
                    integratorName = x.CurrentValue;
                }
                else if (x.Name.Equals(SettingNames.IntegratorKey))
                {
                    integratorKey = x.CurrentValue;
                }
                else if (x.Name.Equals(SettingNames.CustomerAgentUsername))
                {
                    custAgentUsername = x.CurrentValue;
                }
                else if (x.Name.Equals(SettingNames.CustomerAgentPassword))
                {
                    custAgentPassword = x.CurrentValue;
                }
            });

            CardsavrHelper helper = new CardsavrHelper();
            helper.SetAppSettings(cardsavrUrl, integratorName, integratorKey);

            // Create a new response object that encapsulates the data type we'll be returning
            var response = new CardholderGrantResponse();

            var userIdentifier = request.UserIdentifier.ToString();
            Console.WriteLine("Received User ID = " + userIdentifier);

            // Create Sesssion 
            //
            // TODO: use cuid for trace (or hash it)
            string trace = "{\"key\": \"" + userIdentifier + "\"}";       // optional
            ClientSession auth = await helper.LoginAndCreateSession(custAgentUsername, custAgentPassword, trace);

            Random randomGen = new Random();
            DateTime start = new DateTime(1995, 1, 1);
            int range = (DateTime.Today - start).Days;
            string random_cuid = start.AddDays(randomGen.Next(range)).Ticks.ToString();

            // Create CardHolder
            //
            Cardholder cardholder = new Cardholder
            {
                // TODO: this needs to be the account ID
                // cuid = userIdentifier,
                cuid = userIdentifier + "-" + random_cuid
            };

            // Create card data
            //
            Card card = new Card()
            {
                pan = request.CardNumber,
                expiration_month = request.ExpirationMonth,
                expiration_year = request.ExpirationYear
            };

            // Cardholder address data
            //
            Address address = new Address();
            if (request.CHAddress != null)
            {
                address.is_primary = true;
                address.first_name = request.CHAddress.FirstName;
                address.last_name = request.CHAddress.LastName;
                address.phone_number = request.CHAddress.PhoneNumber;
                address.address1 = request.CHAddress.AddressLine1;
                address.address2 = request.CHAddress.AddressLine2;
                address.city = request.CHAddress.City;
                address.subnational = request.CHAddress.State;
                address.postal_code = request.CHAddress.PostalCode;
                address.country = request.CHAddress.Country;
                address.email = request.CHAddress.Email;
            }

            //            var safe_key = "zbt1e7GnQWwU0TI8t9jvkTbr1K3EFZOBObgB7xl2kiE=";
            ClientLogin login = await helper.CreateCard(custAgentUsername, cardholder, card, address);

            await helper.CloseSession(custAgentUsername);

            Console.WriteLine("Username:" + custAgentUsername);
            Console.WriteLine("Cuid: " + login.cardholder.cuid);
            Console.WriteLine("Grant: " + login.grant);
            Console.WriteLine("Card_id: " + login.card.id);

            string grant = string.Empty;
            string cardid = string.Empty;
            string cuid = string.Empty;

            grant = login.grant;
            cardid = login.card.id + "";
            cuid = login.cardholder.cuid;

            // It's always good to add a trace log for future troubleshooting
            Logger.Trace($"{nameof(GetSettingsAsync)} | Cardsavr URL: [{cardsavrUrl}] | Integrator Name: [{cardsavrUrl}] | Integrator Key : [{integratorKey}]");

            // Populate the details of the Setting object with this service's two template settings
            // The response object's "ItemList" property is an enumerable list of the type we passed into the class definition of SettingsResponse
            response.ItemList.Add(new CardholderGrant
            {
                Grant = grant,
                CardId = cardid,
                CUID = cuid
            });

            // Return the response using an awaited task
            return await Task.FromResult(response);

        }
    }
}