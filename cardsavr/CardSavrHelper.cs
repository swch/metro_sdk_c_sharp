using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Switch.CardSavr.Exceptions;

namespace Switch.CardSavr.Http
{
    public sealed class CardsavrHelper
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private string _cardsavrServer;
        private string _appName;
        private string _appKey;
        private Dictionary<string, ClientSession> _sessions;

        public CardsavrHelper() {}

        public class ClientSession {
            public CardSavrHttpClient client { get; set; }
            public string safeKey { get; set; }
            public int userId { get; set; }
        }

        public async Task CloseSession(string userName) {
            if (!_sessions.ContainsKey(userName)) {
                CardSavrHttpClient client = _sessions[userName].client;
                await client.EndAsync();
                _sessions.Remove(userName);
            }
        }


        public void SetAppSettings(string cardsavrServer, string appName, string appKey) {
            _cardsavrServer = cardsavrServer;
            _appName = appName;
            _appKey = appKey;
            _sessions = new Dictionary<string, ClientSession>();
        }

        public async Task<ClientSession> LoginAndCreateSession(string userName, 
                                                               string password,
                                                               string grant = null,
                                                               string trace = null) {
            if (_sessions.ContainsKey(userName)) {
                return _sessions[userName];
            }
            try {
                CardSavrHttpClient session = new CardSavrHttpClient(_cardsavrServer, _appKey, _appName, userName, password, grant, trace);
                CardSavrResponse<LoginResult> login = await session.Init();
                _sessions[userName] = new ClientSession { client = session, userId = login.Body.user_id, safeKey = login.Body.cardholder_safe_key };
                return _sessions[userName];
            } catch(RequestException ex) {
                log.Error("Unable to create sessions for: " + userName);
                log.Error(ex.StackTrace);
            }
            return null;
        }

        public async Task<ClientLogin> CreateCard(string agent, string financialInstitution, User user, Card card, Address address) {
        
            try {
                //don't need the login data
                ClientSession agentSession = _sessions[agent];
                
                PropertyBag u = new PropertyBag();
                u["email"] = user.email;
                u["phone_number"] = user.phone_number;
                u["username"] = user.username;
                u["role"] = "cardholder";
                
                //set the missing settings for model
                if (String.IsNullOrEmpty(user.first_name)) u["first_name"] = card.first_name;
                if (String.IsNullOrEmpty(user.last_name)) u["last_name"] = card.last_name;
                if (String.IsNullOrEmpty(card.name_on_card)) card.name_on_card = $"{u["first_name"]} {u["last_name"]}";
                u["cardholder_safe_key"] =  agentSession.client.GenerateCardholderSafeKey(card.name_on_card + u["email"]); 
        
                CardSavrResponse<User> userResponse = await agentSession.client.CreateUserAsync(u, (string)u["cardholder_safe_key"], financialInstitution);
                if (userResponse.Body == null || userResponse.Body.id == null) {
                    throw new RequestException($"No body returned Creating User: {u}");
                }
                int cardholderId = userResponse.Body.id ?? -1;
                //eventually these will be one time grants
                CardSavrResponse<CredentialGrant> grantCardholder = await agentSession.client.CreateUserGrantAsync(cardholderId);
                CardSavrResponse<CredentialGrant> grantHandoff = await agentSession.client.CreateUserGrantAsync(cardholderId);
                
                ClientSession cardholderSession = await LoginAndCreateSession(userResponse.Body.username, null, grantCardholder.Body.user_credential_grant);

                CardSavrResponse<Address> addressResponse = await cardholderSession.client.CreateAddressAsync(ApiUtil.BuildPropertyBagFromObject(address));
                card.cardholder_id = cardholderId;
                card.address_id = addressResponse.Body.id ?? -1;
                card.par = ApiUtil.GenerateRandomPAR(card.pan, card.expiration_month, card.expiration_year, userResponse.Body.username);
                CardSavrResponse<Card> cardResponse = await cardholderSession.client.CreateCardAsync(ApiUtil.BuildPropertyBagFromObject(card), (string)u["cardholder_safe_key"]);
                return new ClientLogin(){ userCredentialGrant = grantHandoff.Body.user_credential_grant, card = cardResponse.Body, address = addressResponse.Body, cardholder = userResponse.Body };

            } catch (RequestException e) {
                log.Info(e.StackTrace);
            }
            return null;
        }
    }
}