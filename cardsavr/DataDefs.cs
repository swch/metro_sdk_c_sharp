using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;

using Newtonsoft.Json;


namespace Switch.CardSavr.Http
{
    /*========== GENERAL ==========*/

    // abstract base class only to define the logger (it can't be defined inside a generic class).
    public abstract class CardSavrResponseBase
    {
        // class logger.
        protected static readonly log4net.ILog log = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }

    /// <summary>
    /// CardsavrHttpClient response class. An instance of this class is returned by 
    /// all CardsavrHttpClient API requests.
    /// </summary>
    public sealed class CardSavrResponse<T> : CardSavrResponseBase
        where T : class
    {
        public CardSavrResponse()
        {
            StatusCode = HttpStatusCode.Unused;
        }

        public CardSavrResponse(HttpResponseMessage response)
        {
            StatusCode = response.StatusCode;
            string value = ApiUtil.GetSingleHeaderValue(response.Headers, "x-cardsavr-paging");
            if (value != null)
            {
                log.Debug($"found server paging header: \"{value}\"");
                Paging = Paging.FromHeader(value);
            }
        }

        public CardSavrResponse(HttpResponseMessage response, T body)
            : this(response)
        {
            Body = body;
        }

        /// <summary>
        /// The actual status code returned with the HTTP response. This will always be a success code.
        /// </summary>
        /// <value>The HTTP status code.</value>
        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// The HTTP response body as a typed object.
        /// </summary>
        /// <value>The actual type is dependent on which method was called.</value>
        public T Body { get; set; }

        /// <summary>
        /// Represents the paging header returned from the server as a Paging object. Will be 
        /// null except when used with methods that accept a Paging parameter;
        /// </summary>
        /// <value>The paging parameters returned from the server.</value>
        public Paging Paging { get; set; }

        public static implicit operator CardSavrResponse<T>(CardSavrResponse<List<Cardholder>> v)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// A PropertyBag is used primarily for create and update operations since the properties
    /// submitted to those methods is a limited subset of what is returned from a "get".
    /// It is also used for simple server results (e.g., that returned from update-password).
    /// 
    /// If using an instance of this object in a loop, be sure to call Clear() each time to
    /// remove the contents from the previous loop iteration.
    /// </summary>
    //
    // NOTE: the following property is NOT needed here and will in fact cause the dictionary
    // to serialize improperly, resulting in errors.
    // [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class PropertyBag: Dictionary<string, object>
    {
        // try to limit casting in client applications.
        public int GetInt(string name)
        {
            return (int)this[name];
        }

        // try to limit casting in client applications.
        public string GetString(string name)
        {
            return this[name].ToString();
        }
    }

    /*========== SESSION MANAGEMENT ==========*/

    /// <summary>
    /// Returned by the start-session API.
    /// </summary>

    public class ClientLogin {
        public Cardholder cardholder { get; set; }
        public Card card { get; set; }
        public Address address { get; set; }
        public String grant { get; set; }
    }

    public class Login {
        public string client_public_key { get; set; }
        public string password_proof { get; set; }
        public string username { get; set; }
    }

    public class LoginResult
    {
        public User user { get; set; }
        public int user_id { get; set; }
        public string session_token { get; set; }
        public string server_public_key { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class CredentialGrant
    {
        public string credential_grant { get; set; }
    }

    /*========== ACCOUNTS ==========*/

    /// <summary>
    /// Note the use of nullable value-types, which can be set to null like any reference type.
    /// When a property value is null, it will not be included in an update/delete request.
    /// </summary>
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Account
    {
        public int id { get; set; }
        public int cardholder_id { get; set; }
        public string site_hostname { get; set; }
        public string merchant_site_id { get; set; }
        public string username { get; set; }
        public string last_login { get; set; }
        public string last_password_update { get; set; }
        public string last_saved_card { get; set; }
        public string created_on { get; set; }
        public string last_updated_on { get; set; }
    }

    /*========== ADDRESSES ==========*/

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Address
    {
        public int? id { get; set; }
        public int cardholder_id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public bool? is_primary { get; set; }
        public string address1 { get; set; }
        public string address2 { get; set; }
        public string city { get; set; }
        public string subnational { get; set; }
        public string country { get; set; }
        public string postal_code { get; set; }
        public string created_on { get; set; }
        public string last_updated_on { get; set; }
        public string phone_number { get; set; }
        public string email { get; set; }
    }

    /*========== BINS ==========*/

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class BIN
    {
        public int id { get; set; }
        public string country_code { get; set; }
        public string level { get; set; }
        public string type { get; set; }
        public string brand { get; set; }
        public string issuer { get; set; }
        public bool? is_deprecated { get; set; }
    }

    /*========== CARDS ==========*/

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Card
    {
        public int? id { get; set; }
        public int cardholder_id { get; set; }
        public int? bin_id { get; set; }
        public int address_id { get; set; }
        public string pan { get; set; }
        public string par { get; set; }
        public string customer_key { get; set; }
        public string cvv { get; set; }
        public string expiration_month { get; set; }
        public string expiration_year { get; set; }
        public string name_on_card { get; set; }
        public string created_on { get; set; }
        public string last_updated_on { get; set; }
 
    }

    /*========== CARD PLACEMENT JOBS ==========*/

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class CardPlacementResult
    {
        public int id { get; set; }
        public int bin_id { get; set; }
        public int site_id { get; set; }
        public string parent_transaction_id { get; set; }
        public int filter_cycle { get; set; }
        public object custom_data { get; set; }
        public string job_result_data { get; set; }
        public string site_name { get; set; }
        public string status { get; set; }
        public int time_elapsed { get; set; }
        public string completed_on { get; set; }
        public string created_on { get; set; }
        public string last_updated_on { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class SingleSiteJob
    {
        public int id { get; set; }
        public int cardholder_id { get; set; }
        public int card_id { get; set; }
        public int account_id { get; set; }
        public int site_id { get; set; }
        public string error_id { get; set; }
        public string card_placement_result_id { get; set; }
        public string type { get; set; }
        public string status { get; set; }
        public string safe_blob { get; set; }
        public string safe_nonce { get; set; }
        public string queue_name { get; set; }
        public string job_result_data { get; set; }
        public object custom_data { get; set; }
        public string failure_reason { get; set; }
        public string message_address { get; }
        public string current_state { get; set; }
        public bool notification_sent { get; set; }
        public int run_count { get; set; }
        public bool do_not_queue { get; set; }
        public int time_elapsed { get; set; }
        public string started_on { get; set; }
        public string completed_on { get; set; }
        public string expiration_date { get; set; }
        public string created_on { get; set; }
        public string last_updated_on { get; set; }
        public string job_ready_on { get; set; }
    }

    /*========== INTEGRATORS ==========*/

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Integrator
    {
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string integrator_type { get; set; }
        public string current_key { get; set; }
        public string next_key { get; set; }
        public string last_key { get; set; }
        public string created_on { get; set; }
        public string last_updated_on { get; set; }
        public string next_rotation_on { get; set; }
    }

    /*========== MERCHANT SITES ==========*/

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class FinancialInsitution
    {
        public int id { get; set; }
        public string name { get; set; }
        public string lookup_key { get; set; }
        public string alternate_lookup_key { get; set; }
        public object custom_data { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class MerchantSite
    {
        public int id { get; set; }
        public string name { get; set; }
        //public string note { get; }
        public string host { get; set; }
        public string queue_name { get; }   
        public string[] tags { get; }
        public string job_type { get; }   
        public string interface_type { get; }   
        public string login_page { get; }
        public string forgot_password_page { get; }
        public string credit_card_page { get; }        
        public string[] required_form_fields { get; }
        public Image[] images { get; }
        public AccountLink[] account_link { get; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class AccountLink
    {
        public string key_name;
        public string label;
        public string type;
        public Boolean storaable;
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Image
    {
        public string url { get; set; }
        public int width { get; set; }
        public bool grayscale { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class LoginLabels
    {
        public string username_label { get; set; }
        public string password_label { get; set; }
        public string mfa_label { get; set; }
    }

    /*========== USERS ==========*/

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class User
    {
        public int? id { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string next_password { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string last_login_time { get; set; }
        public bool? is_locked { get; set; }
        public string email { get; set; }
        public bool is_password_update_required { get; set; }
        public string role { get; set; }
        public string phone_number { get; set; }
        public dynamic custom_data { get; set; }
        public string next_rotation_on { get; set; }
        public string created_on { get; set; }
        public string last_updated_on { get; set; }
    }
    /*========== USERS ==========*/

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Cardholder
    {
        public int? id { get; set; }
        public int financial_institution_id { get; set; }
        public string cuid { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string meta_key { get; }
        public string email { get; set; }
        public dynamic custom_data { get; set; }
        public string next_rotation_on { get; set; }
        public string created_on { get; set; }
        public string last_updated_on { get; set; }
        public string grant { get; set; }
     }
}
