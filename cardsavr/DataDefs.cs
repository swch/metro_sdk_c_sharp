﻿using System;
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
            string value = ApiUtil.GetSingleHeaderValue(response.Headers, "paging");
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
    public class StartResult
    {
        public string sessionSalt { get; set; }
        public bool encryptionOn { get; set; }

        public StartResult()
        {
            // the correct default value.
            encryptionOn = true;
        }
    }

    public class Login
    {
        public string userName { get; set; }
        public string clientPublicKey { get; set; }
        public string signedSalt { get; set; }
    }

    public class LoginResult
    {
        public string serverPublicKey { get; set; }
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
        public int merchant_site_id { get; set; }
        public string username { get; set; }
        public string custom_display_text { get; set; }
        public bool? require_master_password { get; set; }
        public bool? auto_login { get; set; }
        public bool? show_payment_method { get; set; }
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
        public int id { get; set; }
        public int user_id { get; set; }
        public bool? is_primary { get; set; }
        public string address1 { get; set; }
        public string address2 { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string country { get; set; }
        public string zip { get; set; }
        public string zip_plus_four_code { get; set; }
        public string created_on { get; set; }
        public string last_updated_on { get; set; }
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
        public int id { get; set; }
        public int cardholder_id { get; set; }
        public int bin_id { get; set; }
        public int address_id { get; set; }
        public string pan { get; set; }
        public string par { get; set; }
        public string cvv { get; set; }
        public string expiration_month { get; set; }
        public string expiration_year { get; set; }
        public string card_color { get; set; }
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
        public string custom_data { get; set; }
        public string job_result_data { get; set; }
        public string site_name { get; set; }
        public string status { get; set; }
        public int time_elapsed { get; set; }
        public string completed_on { get; set; }
        public string created_on { get; set; }
        public string last_updated_on { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class MultipleSiteCardPlacementJob
    {
        public int id { get; set; }
        public int job_id { get; set; }
        public int card_id { get; set; }
        public string site_ids { get; set; }
        public string queue_name { get; set; }
        public bool do_not_queue { get; set; }
        public string failure_reason { get; set; }
        public int total_site_count { get; set; }
        public int successful_site_count { get; set; }
        public int failed_site_count { get; set; }
        public string processed_on { get; set; }
        public string completed_on { get; set; }
        public string expiration_date { get; set; }
        public string created_on { get; set; }
        public string last_updated_on { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class SingleSiteCardPlacementJob
    {
        public int id { get; set; }
        public int place_card_on_multiple_sites_job_id { get; set; }
        public int user_id { get; set; }
        public int card_id { get; set; }
        public int account_id { get; set; }
        public int site_id { get; set; }
        public string error_id { get; set; }
        public string card_placement_result_id { get; set; }
        public string status { get; set; }
        public string safe_blob { get; set; }
        public string safe_nonce { get; set; }
        public string queue_name { get; set; }
        public string job_result_data { get; set; }
        public string custom_data { get; set; }
        public string failure_reason { get; set; }
        public bool do_not_queue { get; set; }
        public int time_elapsed { get; set; }
        public string started_on { get; set; }
        public string completed_on { get; set; }
        public string expiration_date { get; set; }
        public string created_on { get; set; }
        public string last_updated_on { get; set; }
    }

    /*========== CUSTOMERS ==========*/

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Customer
    {
        public int id { get; set; }
        public string name { get; set; }
        public string abbreviation { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
        public string created_on { get; set; }
        public string last_updated_on { get; set; }
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
    public class MerchantSite
    {
        public int id { get; set; }
        public string site_name { get; set; }
        public string site_hostname { get; set; }
        public int alexa_rank { get; set; }
        public string tags { get; set; }
        public string image_list_normal { get; set; }
        public string image_list_normal_gray { get; set; }
        public string image_tile_normal { get; set; }
        public string image_tile_normal_gray { get; set; }
        public string alternate_site_hostnames { get; set; }
        public bool? manual_override_actions { get; set; }
        public bool? manual_override_images { get; set; }
        public string site_dominant_color { get; set; }
        public string created_on { get; set; }
        public string last_updated_on { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class MerchantSiteTag
    {
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string created_on { get; set; }
        public string last_updated_on { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class MerchantSiteToSiteTagLink
    {
        public int id { get; set; }
        public int merchant_site_id { get; set; }
        public int merchant_site_tag_id { get; set; }
        public int? rank { get; set; }
        public string created_on { get; set; }
        public string last_updated_on { get; set; }
    }

    /*========== USERS ==========*/

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class User
    {
        public int? id { get; set; }
        public string username { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string email { get; set; }
        public string phone_number { get; set; }
        public string TFA_SS { get; set; }
        public string TFA_SMS { get; set; }
        public string TFA_type { get; set; }
        public string TFA_frequency { get; set; }
        public string TFA_timeout { get; set; }
        public int? failed_attempts { get; set; }
        public bool? remember_username { get; set; }
        public bool? is_internal { get; set; }
        public bool? TFA { get; set; }
        public bool? is_locked { get; set; }
        public string proxy_login_token_expiration { get; set; }
        public string last_login_time { get; set; }
        public string role { get; set; }
        public string created_on { get; set; }
        public string last_updated_on { get; set; }

        // these properties used only on create/update and won't be returned
        // via a normal get request.
        public string password { get; set; }
        public string cardholder_safe_key { get; set; }
        public string proxy_login_token { get; set; }
    }
}
