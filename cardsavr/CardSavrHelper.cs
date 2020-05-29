using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Switch.CardSavr.Exceptions;

using Newtonsoft.Json;


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

        public void setAppSettings(string cardsavrServer, string appName, string appKey) {
            _cardsavrServer = cardsavrServer;
            _appName = appName;
            _appKey = appKey;
            _sessions = new Dictionary<string, ClientSession>();
        }

        public async Task<CardSavrHttpClient> loginAndCreateSession(string userName, 
                                                                   string password,
                                                                   string grant = null,
                                                                   string trace = null) {
            if (_sessions[userName] != null) {
                return _sessions[userName].client;
            }
            try {
                CardSavrHttpClient session = new CardSavrHttpClient(_cardsavrServer, _appKey, _appName, userName, password, grant, trace);
                CardSavrResponse<LoginResult> login = await session.Init();
                _sessions[userName] = new ClientSession { client = session, userId = login.Body.user_id, safeKey = login.Body.cardholder_safe_key };
                return _sessions[userName].client;
            } catch(RequestException ex) {
                log.Error("Unable to create sessions for: " + userName);
                log.Error(ex.StackTrace);
            }
            return null;
        }

        public async Task<Login> createCard(string agent, string financialInstitution, User user, Card card, Address address) {
        
            try {
                //don't need the login data
                ClientSession agentSession = _sessions[agent];
                
                PropertyBag u = new PropertyBag();
                u["email"] = user.email;
                u["phone_number"] = user.phone_number;
                u["role"] = "cardholder";
                
                //set the missing settings for model
                if (user.first_name != null) u["first_name"] = card.first_name;
                if (user.last_name != null) u["last_name"] = card.last_name;
                if (card.name_on_card != null) card.name_on_card = $"{u["first_name"]} {u["last_name"]}";
                u["cardholder_safe_key"] =  agentSession.client.GenerateCardholderSafeKey(card.name_on_card); 
        
                CardSavrResponse<User> userResponse = await agentSession.client.CreateUserAsync(u, (string)u["cardholder_safe_key"], financialInstitution);
                int cardholderId = userResponse.Body.id ?? -1;
                /*
                //eventually these will be one time grants
                const grant_response_login = await agent_session.getCredentialGrant(cardholder_id);
                const grant_login = grant_response_login.body.user_credential_grant;
                const grant_response_handoff = await agent_session.getCredentialGrant(cardholder_id);
                const grant_handoff = grant_response_handoff.body.user_credential_grant;

                await this.loginAndCreateSession(cardholder_data_copy.username, undefined, grant_login);
                const session_user = this.getSession(cardholder_data_copy.username);

                const address_response = await session_user.createAddress(address_data);
        
                card_data.cardholder_id = cardholder_id;
                card_data.address_id = address_response.body.id;
                card_data.user_id = cardholder_id;
                card_data.par = generateRandomPar(card_data.pan, card_data.expiration_month, card_data.expiration_year, cardholder_data_copy.username);
                const card_response = await session_user.createCard(card_data, cardholder_data_copy.cardholder_safe_key);
        
                return { grant: grant_handoff, username: cardholder_data_copy.username, card_id: card_response.body.id } ;
                */
            } catch (Exception e) {
            }
            return null;
        }
    }
}