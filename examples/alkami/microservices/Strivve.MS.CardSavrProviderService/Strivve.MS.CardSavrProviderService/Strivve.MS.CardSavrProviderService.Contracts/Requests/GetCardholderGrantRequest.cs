using Alkami.Contracts;
using Strivve.MS.CardSavrProviderService.Data;
using System.Runtime.Serialization;

namespace Strivve.MS.CardSavrDelegate.Contracts.Requests
{
    /// <summary>
    ///
    /// </summary>
    [DataContract(IsReference = true)]
    public class GetCardholderGrantRequest : BaseRequest
    {
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string CardNumber { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string ExpirationMonth { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember] 
        public string ExpirationYear { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public CardholderAddress CHAddress { get; set; }
    }
}
