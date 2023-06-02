using System.Runtime.Serialization;

namespace Strivve.MS.CardSavrProviderService.Data
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract(IsReference = false)]
    public class CardholderAddress
    {
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string FirstName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string LastName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string Email { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string AddressLine1 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string AddressLine2 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string City { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string State { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string Country { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string PostalCode { get; set; }
    }
}
