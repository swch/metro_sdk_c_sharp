using System;
using System.Net;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

using Newtonsoft.Json;

using Switch.CardSavr.Exceptions;
using Switch.Security;

/* DEVELOPER NOTES:
 * 
 * The CardSavrHttpClient exposes public methods that mirror the Switch HTTP endpoints. Most accept
 * an "object" parameter called "query". This is a little unorthodox, but simplifies the customer
 * programming model by letting them pass an int or string ID, a list of IDs, or a NameValueCollection
 * containing a more complex filter definition. This is implemented via the internal class QueryDef.
 * Overloading would've been another way to achieve the same goal, but using a class like QueryDef
 * and accepting an "object" parameter seemed cleaner if a little unusual in the strongly-typed C# 
 * world (Javascript or Python people wouldn't blink at this).
 * 
 * All methods are async. They return a class that is an instance of the generic class CardSavrResponse.
 * This class contains the deserialized HTTP response body, and a Paging object representing the
 * contents of the paging header returned by the server. Many methods also accept a Paging object
 * as a parameter.
 * 
 * In create and update methods, the entity parameters are specified as a PropertyBag instead of
 * the strongly-type object (e.g., Account, or Card). Since these methods generally only accept
 * a subset of the full strongly-typed object properties, this seemed to make more sense than
 * defining stripped-down variants for create/update of each entity type.
 * 
 * Additional methods/endpoints can be easily added. Just use the existing methods as an example.
 */

namespace Switch.CardSavr.Http
{
    public sealed class CardSavrHttpClient : HttpClient
    {

        private static HttpClientHandler _handler;
        private static HttpClientHandler _unauthorizedHandler;
        static CardSavrHttpClient() {
            _handler = new HttpClientHandler();
            _unauthorizedHandler = new HttpClientHandler();
            _unauthorizedHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { 
                return true; 
            };
        }

        // class logger.
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // JSON-serializable HTTP request body used for encrypted communications.
        private class EncryptedBody
        {
            public string encryptedBody { get; set; }
        }
       
        // stores session-specific information.
        private SessionData _data;
        /*========== INITIALIZATION AND SETUP ==========*/

        public CardSavrHttpClient(bool rejectUnauthorized = true)
            : base(rejectUnauthorized ? _handler : _unauthorizedHandler)
        {
            _data = null;
        }

        public void SetIdentificationHeader(string clientId)
        {
            DefaultRequestHeaders.Add("client-application", clientId);
        }

        public void Setup(string baseUrl, string staticKey, string appName, string userName, string password, string grant = null, string trace = null, string cert = null)
        {
            if (_data != null)
            {
                log.Error("calling the Setup method more than once isn't supported.");
                throw new NotSupportedException();
            }
            SetIdentificationHeader(appName);
            _data = new SessionData(baseUrl, staticKey, appName, userName, password, grant, trace, cert);
        }

        public async Task<CardSavrResponse<LoginResult>> Init() 
        {
            CardSavrResponse<StartResult> start = await StartAsync(null);
            return await LoginAsync(start.Body.sessionSalt);
        }

        /*========== SESSION MANAGEMENT (START, LOGIN, END) ==========*/

        public async Task<CardSavrResponse<StartResult>> 
            StartAsync(HttpRequestHeaders headers = null)
        {
            CardSavrResponse<StartResult> result = await ApiGetAsync<StartResult>(
                "/session/start", null, null, headers);
            _data.SessionToken = result.Body.sessionToken;

            return result;
        }

        public async Task<CardSavrResponse<LoginResult>> 
            LoginAsync(string sessionSalt, HttpRequestHeaders headers = null)
        {
            object body = new Login()
            {
                clientPublicKey = _data.Ecdh.PublicKey,
                userName = _data.UserName,
                userCredentialGrant = _data.Grant != null ? _data.Grant : null,
                signedSalt = _data.Password != null ? HashUtil.HmacSign(sessionSalt, MakePasswordKey(_data.UserName, _data.Password), true) : null
            };
            //log.Info(JsonConvert.SerializeObject(body));
            CardSavrResponse<LoginResult> result = await ApiPostAsync<LoginResult>(
                "/session/login", body, null, headers);
            // the shared secret will be used in future encrpyted communications.
            _data.Ecdh.ComputeSharedSecret(result.Body.serverPublicKey, true);
            log.Debug("received server public key; computed shared secret.");

            return result;
        }

        public async Task EndAsync()
        {
            await ApiGetAsync<string>("/session/end", null);
        }

        /*========== ACCOUNTS ==========*/

        /// <summary>
        /// Retrieves a page from the list of Account objects, selected according to the specified 
        /// query. If the <paramref name="query"/> parameter is null, all Accounts will be selected.
        /// 
        /// Paging can be controlled via the <paramref name="paging"/> parameter. If this parameter is
        /// null, the first page of results will be returned using the default page-size.
        /// 
        /// The <paramref name="query"/> parameter can be an int or string ID, an IList of IDs, or
        /// a NameValueCollection specifying a more complex filter.
        /// </summary>
        /// <returns>A CardSavrResponse object containing a list of one page of accounts.</returns>
        /// <param name="query">The query. Can be an int, string, IList, or NameValueCollection.</param>
        /// <param name="paging">Paging parameters, or null for the default.</param>
        /// <param name="headers">Additional HTTP headers to add to the request.</param>
        public async Task<CardSavrResponse<List<Account>>> 
            GetAccountsAsync(object query, Paging paging = null, HttpRequestHeaders headers = null)
        {
            QueryDef qd = new QueryDef(query);
            return await ApiGetAsync<List<Account>>("/cardsavr_accounts", qd, paging, headers);
        }

        public async Task<CardSavrResponse<Account>> 
            CreateAccountAsync(PropertyBag body, string safeKey = null, HttpRequestHeaders headers = null)
        {
            return await ApiPostAsync<Account>("/cardsavr_accounts", body, safeKey, headers);
        }

        public async Task<CardSavrResponse<Account>> 
            UpdateAccountAsync(object query, PropertyBag body, string safeKey = null, HttpRequestHeaders headers = null)
        {
            QueryDef qd = new QueryDef(query, body, false, true);
            return await ApiPutDelAsync<Account>(
                "/cardsavr_accounts/{0}", qd.ID, HttpMethod.Put, body, safeKey, headers);
        }

        public async Task<CardSavrResponse<Account>> 
            DeleteAccountAsync(object query, string safeKey = null, HttpRequestHeaders headers = null)
        {
            QueryDef qd = new QueryDef(query, null, false, true);
            return await ApiPutDelAsync<Account>(
                "/cardsavr_accounts/{0}", qd.ID, HttpMethod.Delete, null, safeKey, headers);
        }

        /*========== ADDRESSES ==========*/

        public async Task<CardSavrResponse<List<Address>>> 
            GetAddressesAsync(object query, Paging paging = null, HttpRequestHeaders headers = null)
        {
            QueryDef qd = new QueryDef(query);
            return await ApiGetAsync<List<Address>>("/cardsavr_addresses", qd, paging, headers);
        }

        public async Task<CardSavrResponse<Address>> 
            CreateAddressAsync(PropertyBag body, HttpRequestHeaders headers = null)
        {
            return await ApiPostAsync<Address>("/cardsavr_addresses", body, null, headers);
        }

        public async Task<CardSavrResponse<List<Address>>> 
            UpdateAddressAsync(object query, PropertyBag body, Paging paging = null, HttpRequestHeaders headers = null)
        {
            QueryDef qd = new QueryDef(query, body);
            return await ApiMultiPutDelAsync<Address>(
                "/cardsavr_addresses", null, qd, HttpMethod.Put, body, null, paging, headers);
        }

        public async Task<CardSavrResponse<List<Address>>> 
            DeleteAddressAsync(object query, Paging paging = null, HttpRequestHeaders headers = null)
        {
            QueryDef qd = new QueryDef(query);
            return await ApiMultiPutDelAsync<Address>(
                "/cardsavr_addresses", null, qd, HttpMethod.Delete, null, null, paging, headers);
        }

        /*========== BINS ==========*/

        public async Task<CardSavrResponse<List<BIN>>> 
            GetBinsAsync(object query, Paging paging = null, HttpRequestHeaders headers = null)
        {
            QueryDef qd = new QueryDef(query);
            return await ApiGetAsync<List<BIN>>("/cardsavr_bins", qd, paging, headers);
        }

        public async Task<CardSavrResponse<BIN>> 
            CreateBinAsync(PropertyBag body, HttpRequestHeaders headers = null)
        {
            return await ApiPostAsync<BIN>("/cardsavr_bins", body, null, headers);
        }

        public async Task<CardSavrResponse<List<BIN>>> 
            UpdateBinAsync(object query, PropertyBag body, Paging paging = null, HttpRequestHeaders headers = null)
        {
            QueryDef qd = new QueryDef(query, body);
            return await ApiMultiPutDelAsync<BIN>(
                "/cardsavr_bins", null, qd, HttpMethod.Put, body, null, paging, headers);
        }

        public async Task<CardSavrResponse<List<BIN>>> 
            DeleteBinAsync(object query, Paging paging = null, HttpRequestHeaders headers = null)
        {
            QueryDef qd = new QueryDef(query);
            return await ApiMultiPutDelAsync<BIN>(
                "/cardsavr_bins", null, qd, HttpMethod.Delete, null, null, paging, headers);
        }

        /*========== CARDS ==========*/

        public async Task<CardSavrResponse<List<Card>>> 
            GetCardsAsync(object query, Paging paging = null, HttpRequestHeaders headers = null)
        {
            QueryDef qd = new QueryDef(query);
            return await ApiGetAsync<List<Card>>("/cardsavr_cards", qd, paging, headers);
        }

        public async Task<CardSavrResponse<Card>> 
            CreateCardAsync(PropertyBag body, string safeKey = null, HttpRequestHeaders headers = null)
        {
            return await ApiPostAsync<Card>("/cardsavr_cards", body, safeKey, headers);
        }

        public async Task<CardSavrResponse<List<Card>>> 
            UpdateCardAsync(object query, PropertyBag body, Paging paging = null, HttpRequestHeaders headers = null)
        {
            QueryDef qd = new QueryDef(query, body);
            return await ApiMultiPutDelAsync<Card>(
                "/cardsavr_cards", null, qd, HttpMethod.Put, body, null, paging, headers);
        }

        public async Task<CardSavrResponse<List<Card>>> 
            DeleteCardAsync(object query, string safeKey = null, Paging paging = null, HttpRequestHeaders headers = null)
        {
            QueryDef qd = new QueryDef(query, null, false, true);
            return await ApiMultiPutDelAsync<Card>(
                "/cardsavr_cards", null, qd, HttpMethod.Delete, null, safeKey, paging, headers);
        }

        /*========== CARD PLACEMENT JOBS ==========*/

        public async Task<CardSavrResponse<List<SingleSiteJob>>> 
            GetSingleSiteJobsAsync(object query, Paging paging = null, HttpRequestHeaders headers = null)
        {
            QueryDef qd = new QueryDef(query);
            return await ApiGetAsync<List<SingleSiteJob>>(
                "/place_card_on_single_site_jobs", qd, paging, headers);
        }

        public async Task<CardSavrResponse<SingleSiteJob>> 
            CreateSingleSiteJobAsync(PropertyBag body, string safeKey = null, HttpRequestHeaders headers = null)
        {
            return await ApiPostAsync<SingleSiteJob>(
                "/place_card_on_single_site_jobs", body, safeKey, headers);
        }

        public async Task<CardSavrResponse<SingleSiteJob>>
            UpdateSingleSiteJobAsync(object query, PropertyBag body, string safeKey = null, HttpRequestHeaders headers = null)
        {
            QueryDef qd = new QueryDef(query, body);

            return await ApiPutDelAsync<SingleSiteJob>(
                "/place_card_on_single_site_jobs/{0}", qd.ID, HttpMethod.Put, body, safeKey, headers);
        }

        public async Task<CardSavrResponse<List<CardPlacementResult>>> 
            GetCardPlacementResultReportingJobsAsync(
                object query, Paging paging = null, HttpRequestHeaders headers = null)
        {
            QueryDef qd = new QueryDef(query);
            return await ApiGetAsync<List<CardPlacementResult>>(
                "/card_placement_result_reporting_jobs", qd, paging, headers);
        }

        public async Task<CardSavrResponse<BIN>> 
            CreateCardPlacementResultReportingJobAsync(
                PropertyBag body, string safeKey, HttpRequestHeaders headers = null)
        {
            return await ApiPostAsync<BIN>("/card_placement_result_reporting_jobs", body, null, headers);
        }

        /*========== INTEGRATORS ==========*/

        public async Task<CardSavrResponse<List<Integrator>>> 
            GetIntegratorsAsync(object query, Paging paging = null, HttpRequestHeaders headers = null)
        {
            QueryDef qd = new QueryDef(query);
            return await ApiGetAsync<List<Integrator>>("/integrators", qd, paging, headers);
        }

        public async Task<CardSavrResponse<Integrator>> 
            CreateIntegratorAsync(PropertyBag body, HttpRequestHeaders headers = null)
        {
            return await ApiPostAsync<Integrator>("/integrators", body, null, headers);
        }

        public async Task<CardSavrResponse<List<Integrator>>> 
            UpdateIntegratorsAsync(
                object query, PropertyBag body, Paging paging = null, HttpRequestHeaders headers = null)
        {
            QueryDef qd = new QueryDef(query, body);
            return await ApiMultiPutDelAsync<Integrator>(
                "/integrators", null, qd, HttpMethod.Put, body, null, paging, headers);
        }

        public async Task<CardSavrResponse<Integrator>> 
            RotateIntegratorsAsync(
                object query, HttpRequestHeaders headers = null)
        {
            QueryDef qd = new QueryDef(query);
            string path = "/integrators/{0}/rotate_key";
            return await ApiPutDelAsync<Integrator>(
                path, qd.ID, HttpMethod.Put, null, null, headers);
        }

        public async Task<CardSavrResponse<List<Integrator>>> 
            DeleteIntegratorAsync(object query, Paging paging = null, HttpRequestHeaders headers = null)
        {
            QueryDef qd = new QueryDef(query);
            return await ApiMultiPutDelAsync<Integrator>(
                "/integrators", null, qd, HttpMethod.Delete, null, null, paging, headers);
        }

        /*========== MERCHANT SITES ==========*/

        public async Task<CardSavrResponse<List<MerchantSite>>> 
            GetMerchantSitesAsync(object query, Paging paging = null, HttpRequestHeaders headers = null)
        {
            QueryDef qd = new QueryDef(query);
            return await ApiGetAsync<List<MerchantSite>>("/merchant_sites", qd, paging, headers);
            //string url = "https://swch-site-images-mgmt.s3-us-west-2.amazonaws.com/branding/sites.json";
        }

        /*========== USERS ==========*/

        public async Task<CardSavrResponse<List<User>>> 
            GetUsersAsync(object query, Paging paging = null, HttpRequestHeaders headers = null)
        {
            QueryDef qd = new QueryDef(query);
            return await ApiGetAsync<List<User>>("/cardsavr_users", qd, paging, headers);
        }

        public async Task<CardSavrResponse<User>> 
            CreateUserAsync(PropertyBag body, string newSafeKey = null, string financialInstitution = "default", HttpRequestHeaders headers = null)
        {
            if ((string)body["role"] == "cardholder" && !body.ContainsKey("username") || String.IsNullOrEmpty((string)body["username"])) {
                body["username"] = ApiUtil.RandomString(40);
            }
            log.Info("CREATE CARDHOLDER " + body["username"]);
            if (body.ContainsKey("password")) {
                body["password"] = MakePasswordKey((string)body["username"], (string)body["password"]);
                log.Info((string)body["password"]);
            }
            if (headers == null) {
                headers = new HttpRequestMessage().Headers;
            }
            AddNewSafeKeyHeader(headers, newSafeKey);
            headers.Add("financial-institution", financialInstitution);

            return await ApiPostAsync<User>("/cardsavr_users", body, null, headers);
        }

        public async Task<CardSavrResponse<CredentialGrant>> 
            CreateUserGrantAsync(int id, HttpRequestHeaders headers = null)
        {
            string path = "/cardsavr_users/{0}/credential_grant";
            path = String.Format(path, id);

            return await ApiGetAsync<CredentialGrant>(path, null, null, headers);
        }


       public async Task<CardSavrResponse<List<User>>> 
            UpdateUserAsync(object query, PropertyBag body, string newSafeKey = null, string safeKey = null, Paging paging = null, HttpRequestHeaders headers = null)
        {
           QueryDef qd = new QueryDef(query, body);
            string path = "/cardsavr_users";
            if (headers == null) {
                headers = new HttpRequestMessage().Headers;
            }
            AddNewSafeKeyHeader(headers, newSafeKey);
            return await ApiMultiPutDelAsync<User>(path, null, qd, HttpMethod.Put, body, null, paging, headers);
        }

        public async Task<CardSavrResponse<List<User>>> 
            DeleteUserAsync(object query, Paging paging = null, HttpRequestHeaders headers = null)
        {
            QueryDef qd = new QueryDef(query);
            return await ApiMultiPutDelAsync<User>(
                "/cardsavr_users", null, qd, HttpMethod.Delete, null, null, paging, headers);
        }


        public async Task<CardSavrResponse<PropertyBag>> 
            UpdateUserPasswordAsync(object query, PropertyBag body, HttpRequestHeaders headers = null)
        {
            body["password"] = body["password_copy"] = MakePasswordKey((string)body["username"], (string)body["password"]);
            QueryDef qd = new QueryDef(query, body, false, true);
            string path = "/cardsavr_users/{0}/update_password";
            return await ApiPutDelAsync<PropertyBag>(path, qd.ID, HttpMethod.Put, body, null, headers);
        }       
        
         /*========== CARDHOLDERS ==========*/

        public async Task<CardSavrResponse<List<Cardholder>>> 
            GetCardholdersAsync(object query, Paging paging = null, HttpRequestHeaders headers = null)
        {
            QueryDef qd = new QueryDef(query);
            return await ApiGetAsync<List<Cardholder>>("/cardholders", qd, paging, headers);
        }

        public async Task<CardSavrResponse<Cardholder>> 
            CreateCardholderAsync(PropertyBag body, string newSafeKey = null, string financialInstitution = "default", HttpRequestHeaders headers = null)
        {
            if (headers == null) {
                headers = new HttpRequestMessage().Headers;
            }
            AddNewSafeKeyHeader(headers, newSafeKey);
            headers.Add("financial-institution", financialInstitution);

            return await ApiPostAsync<Cardholder>("/cardholders", body, null, headers);
        }

        public async Task<CardSavrResponse<Cardholder>> 
            AuthorizeCardholder(string grant, HttpRequestHeaders headers = null)
        {
            string path = "/cardholders/{0}/authorize";
            path = String.Format(path, grant);

            return await ApiGetAsync<Cardholder>(path, null, null, headers);
        }

        public async Task<CardSavrResponse<List<Cardholder>>> 
            UpdateCardholderAsync(object query, PropertyBag body, string newSafeKey = null, string safeKey = null, Paging paging = null, HttpRequestHeaders headers = null)
        {
            QueryDef qd = new QueryDef(query, body);
            string path = "/cardholders";
            if (headers == null) {
                headers = new HttpRequestMessage().Headers;
            }
            AddNewSafeKeyHeader(headers, newSafeKey);
            return await ApiMultiPutDelAsync<Cardholder>(path, null, qd, HttpMethod.Put, body, null, paging, headers);
        }

        public async Task<CardSavrResponse<List<Cardholder>>> 
            DeleteCardholderAsync(object query, Paging paging = null, HttpRequestHeaders headers = null)
        {
            QueryDef qd = new QueryDef(query);
            return await ApiMultiPutDelAsync<Cardholder>(
                "/cardholders", null, qd, HttpMethod.Delete, null, null, paging, headers);
        }

        /*========== PRIVATE IMPLEMENTATION ==========*/

        // Could be exposed publicly to give advanced users more flexibility.
        private async Task<CardSavrResponse<T>> ApiGetAsync<T>(
            string path, QueryDef qd, Paging paging = null, HttpRequestHeaders headers = null)
            where T: class
        {
            // allow an empty query: enumerates all entities.
            path = ApiUtil.AppendQueryString(path, qd);
            log.Debug($"path with query: \"{path}\"");

            using (HttpRequestMessage request = CreateRequest(HttpMethod.Get, path, null, headers))
            {
                if (paging != null && paging.GetCount() > 0)
                    request.Headers.Add("paging", paging.Stringify());

                using (HttpResponseMessage response = await SendAsync(request))
                {
                    return await ProcessResponseAsync<T>(request, response);
                }
            }
        }

        // Could be exposed publicly to give advanced users more flexibility.
        private async Task<CardSavrResponse<T>> ApiPostAsync<T>(
            string path, object body, string safeKey, HttpRequestHeaders headers = null)
            where T : class
        {
            using (HttpRequestMessage request = CreateRequest(HttpMethod.Post, path, body, headers))
            {
                AddSafeKeyHeader(request.Headers, safeKey);
                using (HttpResponseMessage response = await SendAsync(request))
                {
                    return await ProcessResponseAsync<T>(request, response);
                }
            }
        }

        // Could be exposed publicly to give advanced users more flexibility.
        private async Task<CardSavrResponse<T>> ApiPutDelAsync<T>(
            string path, string id, HttpMethod method, object body, string safeKey, HttpRequestHeaders headers = null)
            where T : class
        {
            string newPath = String.Format(path, id);
            using (HttpRequestMessage request = CreateRequest(method, newPath, body, headers))
            {
                AddSafeKeyHeader(request.Headers, safeKey);
                using (HttpResponseMessage response = await SendAsync(request))
                {
                    return await ProcessResponseAsync<T>(request, response);
                }
            }
        }

        // Could be exposed publicly to give advanced users more flexibility.
        private async Task<CardSavrResponse<List<T>>> ApiMultiPutDelAsync<T>(
            string path, string pathWithId, QueryDef qd, HttpMethod method, object body, 
            string safeKey, Paging paging = null, HttpRequestHeaders headers = null)
            where T : class
        {
            // by default we just tack the id on the end (we assume no trailing slash).
            pathWithId = pathWithId ?? path + "/{0}";

            CardSavrResponse<List<T>> result;
            if (qd.IsID)
            {
                // this is a singular operation, so just do it.
                CardSavrResponse<T> singleResponse = await ApiPutDelAsync<T>(
                    pathWithId, qd.ID, method, body, safeKey, headers);

                // then transform the result into a list.
                result = new CardSavrResponse<List<T>>()
                {
                    StatusCode = singleResponse.StatusCode,
                    Paging = singleResponse.Paging,
                    Body = new List<T>() { singleResponse.Body }
                };
            }
            else
            {
                // get all the things implied by the query parameters, the iterate through and perform
                // the operation on each thing successively.
                result = await ApiGetAsync<List<T>>(path, qd, paging, headers);
                foreach (T t in result.Body)
                {
                    // we have to use reflection on a generic type.
                    string id = Convert.ToString(t.GetType().GetProperty("id").GetValue(t));

                    // should probably have some error handling here so we can continue on failure.
                    await ApiPutDelAsync<T>(pathWithId, id, method, body, safeKey, headers);
                }
            }

            return result;
        }

        /// <summary>
        /// Creates an HttpRequestMessage by serializing and encrypting the body content (if
        /// present), populating the message with headers, and signing
        /// the request.
        /// </summary>
        /// <returns>A newly created HttpRequestMessage, ready for SendAsync().</returns>
        /// <param name="method">The HttpMethod to use.</param>
        /// <param name="path">The HTTP endpoint.</param>
        /// <param name="body">The body content, or null.</param>
        /// <param name="headers">HTTP headers to add to the message, or null.</param>
        private HttpRequestMessage CreateRequest(
            HttpMethod method, string path, object body, HttpRequestHeaders headers)
        {
            // create the request with default header setup. notice we copy default headers
            // before any caller-supplied headers so that client-headers can override.
            log.Info($"creating \"{method}\" request for path: {path}");
            Uri endpoint = new Uri(_data.BaseUri, path);
            HttpRequestMessage request = new HttpRequestMessage(method, endpoint);

            CopyRequestHeaders(request.Headers, DefaultRequestHeaders);
            CopyRequestHeaders(request.Headers, headers);
            if (_data.SessionToken != null)
                request.Headers.Add("x-cardsavr-session-jwt", _data.SessionToken);

            try {
                request.Headers.Add("trace", ApiUtil.BuildValidTraceHeader(_data.Trace, _data.UserName));
            } catch (JsonException ex){ 
                log.Error("INVALID custom trace header: " + _data.Trace);
                log.Error(ex.StackTrace);
            }

            // encrypt the body and/or serialize to JSON.
            string strBody = null;
            if (body != null)
            {
                // encrypt body, wrap in EncryptedBody, serialize to JSON.
                log.Debug("encrpyting request body.");
                strBody = JsonConvert.SerializeObject(EncryptBody(body));
            }
            SignRequest(request.Headers, endpoint, strBody);

            if (strBody != null)
            {
                // add the (possibly encrypted) body and appropriate headers.
                // remove any existing header, otherwise an exception will be thrown since
                // "Content-Type" cannot be multi-valued.
                request.Content = new StringContent(strBody);
                request.Content.Headers.Remove("Content-type");
                request.Content.Headers.Add("Content-type", "application/json");
            }
            
            return request;
        }

        /// <summary>
        /// Processes the HTTP response by reading the body content, decrypting it and verifying
        /// the signature. Also stores the sessionToken returned by /start the server. Returns a CardSavrResponse
        /// object containing the deserialized body content, and the content of the paging header
        /// if present.
        /// 
        /// Note that if wanted the generic parameter T can be "string" to retrieve the raw body 
        /// content without deserializing.
        /// </summary>
        /// <returns>The HTTP request corresponding to this response.</returns>
        /// <param name="request">The HttpRequestMessage corresponding to this response.</param>
        /// <param name="response">The HttpResponseMessage to process.</param>
        /// <typeparam name="T">The type of the deserialized body contents.</typeparam>
        private async Task<CardSavrResponse<T>> ProcessResponseAsync<T>(
            HttpRequestMessage request, HttpResponseMessage response)
            where T : class
        {
            // read response body as a string. if the response is an error, throw an exception.
            log.Info($"processing \"{request.Method}\" response for path: {request.RequestUri.PathAndQuery} {response.StatusCode}");
            string body = await response.Content.ReadAsStringAsync();
            CheckStatusAndMaybeThrow(body, response);
 
            VerifyResponseSignature(request.RequestUri, response, body);
 
            // the generic parameter constraint allows use of the "as" operator in this context.
            // this code sets "tstr" to the value of body if T is a string, and to null
            // otherwise. then if tstr is null (i.e., it is not a string), we can assume it 
            // represents a JSON-serializable object. basically, if we can treat the body 
            // (which is a string) as type T, just return the body. 
            T tstr = body as T;
            if (tstr != null || body == null)
            {
                log.Debug($"returning body as a string: \"{body}\"");
                return new CardSavrResponse<T>(response, tstr);
            }

            try
            {
                // encryption is on, so we're expecting an encrypted body.
                return new CardSavrResponse<T>(response, DecryptBody<T>(body));
            }
            catch (JsonSerializationException ex)
            {
                // this may happen when starting a session. encryption is expected, but
                // won't be done because we haven't completed the key exchange.
                // allow this to fall through so we deserialize to the expected type T
                // without decrypting.
                log.Error("possibly OK JSON serialization exception.", ex);
            }

            // no encryption; we should be able to deserialize the body directly.
            return new CardSavrResponse<T>(response, JsonConvert.DeserializeObject<T>(body));
        }

        /// <summary>
        /// Creates a signature for the specified path and body, and adds the necessary
        /// signing headers to the supplied HttpRequestHeaders object. A JSON body must
        /// have been already stringified, but NOT in base64 format.
        /// </summary>
        /// <param name="headers">Collection to which signing headers will be added.</param>
        /// <param name="path">The destination URL.</param>
        /// <param name="body">The request body, or null if none.</param>
        private void SignRequest(HttpRequestHeaders headers, Uri path, string body = null)
        {
            // create the nonce (i.e., the current time in milliseconds as a decimal string).
            string nonce = Convert.ToString(DateTime.Now.Millisecond, 10);

            // create auth string (to be placed in authorization header).
            string authorization = $"SWCH-HMAC-SHA256 Credentials={_data.AppName}";

            // create the string that needs to be signed.
            string toSign = Uri.UnescapeDataString(path.PathAndQuery) + authorization + nonce;
            if (body != null)
                toSign += body;

            // compute signature and add headers.
            string signature = HashUtil.HmacSign(toSign, GetEncryptionKey());
            headers.Add("authorization", authorization);
            headers.Add("nonce", nonce);
            headers.Add("signature", signature);
            log.Debug($"signed HTTP request; signature={signature}.");
        }

        private EncryptedBody EncryptBody(object body)
        {
            string jsonBody = JsonConvert.SerializeObject(body);
            return new EncryptedBody()
            {
                encryptedBody = Aes256.EncryptText(jsonBody, GetEncryptionKey())
            };
        }

        private T DecryptBody<T>(string body)
        {
            // do not check _data.Encrypt to determine if the response is encrpyted, cause that
            // won't work in the initial stages (e.g., start, login). instead, see if the 
            // deserialized object contains a property called "encryptedBody".
            EncryptedBody encBody = JsonConvert.DeserializeObject<EncryptedBody>(body);
            if (encBody == null || encBody.encryptedBody == null)
            {
                // deserialize directly into the expected type.
                log.Debug($"clear-text body: \"{body}\"");
                return JsonConvert.DeserializeObject<T>(body);
            }

            // we have an EncryptedBody object containing an encrypted response.
            // split the text string into cipher text and IV, then decrypt.
            log.Debug($"encrypted body: \"{body}\"");
            string[] parts = encBody.encryptedBody.Split('$');
            body = Aes256.DecryptText(parts[0], parts[1], GetEncryptionKey());
            log.Debug($"decrypted body: \"{body}\"");
            
            return JsonConvert.DeserializeObject<T>(body);
        }

        /// <summary>
        /// Check the status of an HTTP response, and throw an exception if its an error. 
        /// If we've received an error response, we try to decrypt the body so it can be 
        /// logged and included in the thrown exception.
        /// </summary>
        /// <param name="body">The received HTTP response body.</param>
        /// <param name="response">The HttpResponseMessage object.</param>
        private void CheckStatusAndMaybeThrow(string body, HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                log.Error($"failed request ({response.StatusCode}): body contents: \"{body}\"");
                try
                {
                    EncryptedBody enc = JsonConvert.DeserializeObject<EncryptedBody>(body);
                    if (enc != null && enc.encryptedBody != null)
                    {
                        // we have an EncryptedBody object containing an encrypted response.
                        // split the text string into cipher text and IV, then decrypt.
                        string[] parts = enc.encryptedBody.Split('$');
                        body = Aes256.DecryptText(parts[0], parts[1], GetEncryptionKey());
                        log.Error($"decrypted body: \"{body}\"");
                    }
                }
                catch (Exception)
                {
                    // ignore any exception that happens during the decryption attempt.
                }

                // include either the original or decrypted body as the exception message.
                throw new RequestException(body);
            }
        }

        private void VerifyResponseSignature(Uri uri, HttpResponseMessage response, string body)
        {
            string auth = ApiUtil.GetSingleHeaderValue(response.Headers, "authorization");
            string nonce = ApiUtil.GetSingleHeaderValue(response.Headers, "nonce");
            string toSign = $"{uri.PathAndQuery}{auth}{nonce}{body ?? ""}";
            string signature = ApiUtil.GetSingleHeaderValue(response.Headers, "signature");

            if (auth == null || auth == "") {
                log.Debug("Not an authorized response, probably merchants endpoint");

            } 
            else if (!HashUtil.HmacVerify(toSign, GetEncryptionKey(), signature))
            {
                log.Error($"failed to verify signature \"{signature}\"; auth=({auth}), nonce=({nonce})");
                throw new InvalidSignatureException($"{uri.PathAndQuery}, expected={signature}");
            }

            log.Debug($"verified response-signature \"{signature}\"");
        }

        private string MakePasswordKey(string userName, string password)
        {
            byte[] salt = HashUtil.Sha256Hash(userName);
            byte[] key = HashUtil.Sha256Pbkdf2(password, salt, 5000, 32);
            return Convert.ToBase64String(key);
        }

        private string GetEncryptionKey()
        {
            // when logging in we haven't yet computed the shared secret.
            // so use the client's static key instead.
            return _data.Ecdh.SharedSecret ?? _data.StaticKey;
        }

        /// <summary>
        /// Adds the safe-key header iff a valid safe-key value is passed. 
        /// Does NOT check for a valid safe-key.
        /// </summary>
        /// <param name="headers">The HttpHeaders to contain the safe-key header.</param>
        /// <param name="safeKey">The safe-key to process.</param>
        private void AddSafeKeyHeader(HttpRequestHeaders headers, string safeKey)
        {
            if (!string.IsNullOrEmpty(safeKey)) 
                headers.Add("cardholder-safe-key", Aes256.EncryptText(safeKey, GetEncryptionKey()));
        }
        private void AddNewSafeKeyHeader(HttpRequestHeaders headers, string newSafeKey)
        {
            if (!string.IsNullOrEmpty(newSafeKey))
                headers.Add("new-cardholder-safe-key", Aes256.EncryptText(newSafeKey, GetEncryptionKey()));
        }


        private void CopyRequestHeaders(HttpRequestHeaders dst, HttpRequestHeaders src)
        {
            if (src != null)
            {
                foreach (KeyValuePair<string, IEnumerable<string>> header in src)
                {
                    dst.Remove(header.Key);
                    dst.Add(header.Key, header.Value);
                }
            }
        }
    }
}
