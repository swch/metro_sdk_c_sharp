using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Switch.CardSavr.Exceptions;
using System.Collections.Specialized;

namespace Switch.CardSavr.Http
{
    public class ClientSession {
        public CardSavrHttpClient client { get; set; }
    }

    public sealed class CardsavrHelper
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private bool _rejectUnauthorized;
        private string _cardsavrServer;
        private string _appName;
        private string _appKey;
        private Dictionary<string, ClientSession> _sessions;

        public CardsavrHelper() {}

        public async Task CloseSession(string username) {
            if (_sessions.ContainsKey(username)) {
                CardSavrHttpClient client = _sessions[username].client;
                await client.EndAsync();
                _sessions.Remove(username);
            }
        }


        public void SetAppSettings(string cardsavrServer, string appName, string appKey, bool rejectUnauthorized = true) {
            _cardsavrServer = cardsavrServer;
            _appName = appName;
            _appKey = appKey;
            _sessions = new Dictionary<string, ClientSession>();
            _rejectUnauthorized = rejectUnauthorized;
        }

        public async Task<ClientSession> LoginAndCreateSession(string username, 
                                                               string password,
                                                               string trace = null) {
            if (_sessions.ContainsKey(username)) {
                return _sessions[username];
            }
            CardSavrHttpClient session = new CardSavrHttpClient(_rejectUnauthorized);
            session.Setup(_cardsavrServer, _appKey, _appName, username, password, trace);
            CardSavrResponse<LoginResult> login = await session.Init();
            _sessions[username] = new ClientSession { client = session };
            return _sessions[username];
        }

        public async Task<String> RotateIntegrator(string user, string integrator_name) {
            ClientSession agentSession = _sessions[user];
            CardSavrResponse<List<Integrator>> response = await agentSession.client.GetIntegratorsAsync(new NameValueCollection() {
                { "name", integrator_name }
            });
            if (response.Body == null || response.Body.Count > 1) {
                throw new ArgumentException($"Can't reset unknown integrator: {integrator_name}");
            }
            Integrator integrator = response.Body[0];            
            CardSavrResponse<Integrator> updated = await agentSession.client.RotateIntegratorsAsync(integrator.id);
            return updated.Body.current_key;
        }

        /*
         * Rotating this user's password could potentially break the current user, so ensure that the return password
         * is stored someplace safely.
         */
        public async Task<string> RotateAgentPassword(string agent, string newPassword = null) {

            ClientSession agentSession = _sessions[agent];
            CardSavrResponse<List<User>> u = await agentSession.client.GetUsersAsync(new NameValueCollection() {
                { "username", agent }
            });

            if (u == null) {
                throw new ArgumentException($"Username {agent} does not exist");
            } else if (u.Body.Count > 1) {
                throw new ArgumentException($"{u.Body.Count} users for ${agent}, there should only be one.");
            }
            PropertyBag bag = new PropertyBag();
            bag["username"] = agent;
            string password = (newPassword ?? ApiUtil.GenerateStretchedPassword(agent, 20)); //bag gets mutated
            bag["password"] = password;
            CardSavrResponse<PropertyBag> response = await agentSession.client.UpdateUserPasswordAsync((int)u.Body[0].id, bag);
            if (response.Body != null) {
                return password;
            }
            throw new ArgumentException("Couldn't update provided password.");
        }

        public async Task<ClientLogin> CreateCard(string agent, string financialInstitution, User user, Card card, Address address, string safeKey = null) {
        
            try {
                //don't need the login data
                ClientSession agentSession = _sessions[agent];
                
                PropertyBag u = new PropertyBag();
                u["email"] = user.email;
                u["phone_number"] = user.phone_number;
                u["username"] = user.username;
                u["custom_data"] = user.custom_data;
                u["role"] = "cardholder";
                
                //set the missing settings for model
                if (String.IsNullOrEmpty(user.first_name)) u["first_name"] = card.first_name;
                if (String.IsNullOrEmpty(user.last_name)) u["last_name"] = card.last_name;
                if (String.IsNullOrEmpty(card.name_on_card)) card.name_on_card = $"{u["first_name"]} {u["last_name"]}";

                CardSavrResponse<User> userResponse = await agentSession.client.CreateUserAsync(u, financialInstitution);
                if (userResponse.Body == null || userResponse.Body.id == null) {
                    throw new RequestException($"No body returned Creating User: {u}");
                }
                int cardholderId = userResponse.Body.id ?? -1;
                address.user_id = cardholderId;
                CardSavrResponse<Address> addressResponse = await agentSession.client.CreateAddressAsync(ApiUtil.BuildPropertyBagFromObject(address));
                card.cardholder_id = cardholderId;
                card.address_id = addressResponse.Body.id ?? -1;
                card.par = ApiUtil.GenerateRandomPAR(card.pan, card.expiration_month, card.expiration_year, userResponse.Body.username);
                CardSavrResponse<Card> cardResponse = await agentSession.client.CreateCardAsync(ApiUtil.BuildPropertyBagFromObject(card), safeKey); 
                return new ClientLogin(){ userCredentialGrant = userResponse.Body.credential_grant, card = cardResponse.Body, address = addressResponse.Body, cardholder = userResponse.Body };

            } catch (RequestException e) {
                log.Info(e.StackTrace);
            }
            return null;
        }
    }
}