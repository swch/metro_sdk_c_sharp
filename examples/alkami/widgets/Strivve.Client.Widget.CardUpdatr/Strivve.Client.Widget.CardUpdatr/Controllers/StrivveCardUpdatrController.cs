using Alkami.MicroServices.Accounts.Contracts;
using Alkami.Client.Framework.Mvc;
using Alkami.Common;
using Alkami.MicroServices.Accounts.Service.Client;
using Alkami.Security.Common.Claims;
using Alkami.MicroServices.Security.Contracts;
using Alkami.MicroServices.Security.Contracts.Requests;
using Alkami.MicroServices.Security.Service.Client;
using Common.Logging;
using Strivve.Client.Widget.CardUpdatr.Models;
using Strivve.MS.CardsavrProvider.Contracts.Requests;
using Strivve.MS.CardsavrProvider.Contracts.Responses;
using Strivve.MS.CardsavrProvider.Data;
using Strivve.MS.CardsavrProvider.Contracts;
using Strivve.MS.CardsavrProvider.Service.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Linq;
using WebToolkit;
using Alkami.Client.Services.Bank.Repository;
using UserContactEmail = Alkami.Security.Common.DataContracts.UserContactEmail;
using UserContactPhone = Alkami.Security.Common.DataContracts.UserContactPhone;
using UserContactAddress = Alkami.Security.Common.DataContracts.UserContactAddress;
using UserAccount = Alkami.MicroServices.Security.Data.UserAccount;
using Alkami.MicroServices.Accounts.Contracts.Requests;
using Alkami.MicroServices.Accounts.Contracts.Responses;
using Alkami.MicroServices.CardManagement.Service.Client;
using Alkami.MicroServices.CardManagement.Contracts;
using Alkami.MicroServices.CardManagement.Contracts.Requests;
using Alkami.MicroServices.CardManagement.Contracts.Responses;
using Alkami.MicroServices.CardManagement.Contracts.Filters;
using Alkami.MicroServices.CardManagementProviders.Data;
using Strivve.MS.CardsavrProvider.Data.ProviderSettings;

namespace Strivve.Client.Widget.CardUpdatr.Controllers
{
    [ClaimsAuthorizationFilter(PermissionNames.NoPermissions)]
    public class StrivveCardUpdatrController : BaseController
    {
        public ICustomerRepository CustomerRepository { get; set; }

        /// <summary>
        /// Gets the logger
        /// </summary>
        private static readonly ILog Logger = LogManager.GetLogger<StrivveCardUpdatrController>();

        public static readonly Func<ICardsavrProviderServiceContract> ServiceFactory = () => new CardsavrProviderServiceClient();
        public static Func<IAccountServiceContract> accountsService = () => new AccountServiceClient();
        public static Func<ISecurityServiceContract> securityService = () => new SecurityServiceClient();
        public static Func<ICardManagementServiceContract> cardManagementService = () => new CardManagementServiceClient();

        Dictionary<long, string> accountDisplayNames = new Dictionary<long, string>();
        Dictionary<long, int> accountThemeColoIndex = new Dictionary<long, int>();

        public StrivveCardUpdatrController() { }

        public async Task<ActionResult> Index()
        {
            // get the card GUID from querystring
            var cardGUID = Request.QueryString["cardid"];
            if ( cardGUID != null)
            {
                Logger.Info("CARD GUID Found = ");
                Logger.Info(cardGUID);
            }


            GetCardFilter cardFilter = new GetCardFilter();
            cardFilter.CardIdentifiers = new List<string> { cardGUID };
            GetCardsRequest cardsRequest = new GetCardsRequest();
            cardsRequest.Filter = cardFilter;
            this.AugmentRequest(cardsRequest);

            GetCardsResponse cardsResponse = await cardManagementService().GetCardsAsync(cardsRequest);
            var cards = cardsResponse.Cards.ToArray();
            Card card = cards[0];
            if ( card != null )
            {
                Logger.Info("Card found");
            }

            UserContactEmail emailContact =
                CurrentUser.GetUserContact<UserContactEmail>(x => x.IsPrimary).FirstOrDefault()
                ?? CurrentUser.GetUserContact<UserContactEmail>().FirstOrDefault();

            UserContactPhone phoneContact =
                CurrentUser.GetUserContact<UserContactPhone>(x => x.IsPrimary).FirstOrDefault()
                ?? CurrentUser.GetUserContact<UserContactPhone>().FirstOrDefault();

            UserContactAddress addressContact =
                CurrentUser.GetUserContact<UserContactAddress>(x => x.IsPrimary).FirstOrDefault()
                ?? CurrentUser.GetUserContact<UserContactAddress>().FirstOrDefault();

            string userDisplayName = CurrentUser.Name;
            string firstName = CurrentUser.FirstName;
            string lastName = CurrentUser.LastName;
            string email = emailContact.Email;
            string phone = phoneContact.PhoneNumber;            

            /*
            var member = CustomerRepository.Get();
            var memberRequest = new GetUserRequest() { UserId = member.Model.ID, Mapping = new UserMapper() { ShouldIncludeExternalUserAccounts = true, ShouldIncludeUserAccounts = true, ShouldIncludePermissions = true, ShouldIncludeUserAccountPermissions = true } };
            this.AugmentRequest(memberRequest);

            var accountsResponse = GetAccounts(memberRequest);
            var accounts = accountsResponse.Accounts.ToArray();
            var account = accounts[0];
            var address = account.AddressLine1;
            */

            // Format expiration date
            DateTime expirationDate = card.ExpirationDate;
            int month = expirationDate.Month;
            string monthString = month < 10 ? "0"+month.ToString() : month.ToString();
            int year = expirationDate.Year;
            string yearString = (year % 100).ToString();

            GetCardholderGrantRequest request = new GetCardholderGrantRequest();
            request.CardNumber = card.CardNumber;
            request.ExpirationMonth = monthString;
            request.ExpirationYear = yearString;

            request.CHAddress = new CardholderAddress();

            BillingAddress billingAddress = card.BillingAddress;
            if ( billingAddress != null )
            {
                request.CHAddress.AddressLine1 = billingAddress.AddressLine1;
                request.CHAddress.AddressLine2 = billingAddress.AddressLine2;
                request.CHAddress.City = billingAddress.City;
                request.CHAddress.State = billingAddress.State;
                request.CHAddress.Country = billingAddress.Country;
                request.CHAddress.PostalCode = billingAddress.PostalCode;
            }
            else
            {
                // if billing address is not found in the card data, 
                // use User Account's address instead.
                request.CHAddress.AddressLine1 = addressContact.AddressLine1;
                request.CHAddress.AddressLine2 = addressContact.AddressLine2;
                request.CHAddress.City = addressContact.City;
                request.CHAddress.State = addressContact.State;
                request.CHAddress.Country = addressContact.Country;
                request.CHAddress.PostalCode = addressContact.PostalCode;
            }

            request.CHAddress.FirstName = firstName;
            request.CHAddress.LastName = lastName;
            request.CHAddress.Email = email;
            request.CHAddress.PhoneNumber = phone;

            this.AugmentRequest(request);

            CardholderGrantResponse response = await ServiceFactory().GetCardholderGrantAsync(request);
            var items = response.ItemList.ToArray();
            CardholderGrant grantData = items[0];

            GetSettingsRequest settingsRequest = new GetSettingsRequest();
            this.AugmentRequest(settingsRequest);

            SettingsResponse settingsResponse = await ServiceFactory().GetSettingsAsync(settingsRequest);
            var settingsItems = settingsResponse.ItemList;
            
            var cardupdatrAppUrlSetting = settingsItems.Find(x => x.Name == SettingNames.CardupdatrAppURL);
            var cardupdatrAppUrl = "";
            if (cardupdatrAppUrlSetting != null )
            {
                cardupdatrAppUrl = cardupdatrAppUrlSetting.CurrentValue;
            }

            if ( !cardupdatrAppUrl.EndsWith("/") )
            {
                cardupdatrAppUrl += "/";
            }

            var model = new StrivveCardUpdatrModel();
            model.UserDisplayName = userDisplayName;
            model.Grant = grantData.Grant;
            model.CardID = grantData.CardId;
            model.Card = card;
            model.CardNumberDisplay = "**** **** **** " + card.CardNumber.Substring(card.CardNumber.Length - 4);
            model.ExpirationDateDisplay = monthString + "/" + yearString;
            model.Address = card.BillingAddress;
            model.Email = email;
            model.Phone = phone;
            model.FirstName = firstName;
            model.LastName = lastName;
            model.CardupdatrAppUrl = cardupdatrAppUrl;
            model.ScriptSrcUrl = cardupdatrAppUrl + "cardupdatr-client-v2.js";

            try
            {
                Logger.DebugFormat("[GET] Controller/Index");
                return View("Index", model);
            }
            catch (System.Exception e)
            {
                Logger.Error("Error [GET] Controller/Index", e);
                return View("Error");
            }
        }

        private AccountResponse GetAccounts(GetUserRequest memberRequest)
        {
            var userAccount = AsyncHelper.RunSync(() => securityService().GetUserAsync(memberRequest));

            accountDisplayNames = new Dictionary<long, string>();
            if (userAccount == null || userAccount.Users.FirstOrDefault().IsDeleted)
                return new AccountResponse() { Accounts = new List<Alkami.MicroServices.Accounts.Data.Account>() };

            var userAccounts = userAccount.Users.FirstOrDefault().UserAccounts;

            List<long> accountIds = new List<long>();
            foreach (UserAccount ua in userAccounts)
            {
                if ((ua.Relationship <= 3 || ua.HasMasterRights) && !ua.Deleted && !ua.HideFromEndUser)
                {
                    accountIds.Add(ua.AccountId);
                    accountDisplayNames.Add(ua.AccountId, ua.DisplayName);
                    accountThemeColoIndex.Add(ua.AccountId, (int)ua.ThemeColorIndex);
                }
            }

            var accountRequest = new GetAccountRequest()
            {
                Filter = new AccountFilter() { Ids = accountIds, IncludeExternal = true },
                Mapping = new AccountMapper()
                {
                    IncludeAccountType = true,
                    IncludeAccountTypeFields = true,
                    IncludeRoutingInfo = true
                }
            };
            this.AugmentRequest(accountRequest);

            return AsyncHelper.RunSync(() => accountsService().GetAccountAsync(accountRequest));
        }
    }
}
