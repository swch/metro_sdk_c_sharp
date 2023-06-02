using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Strivve.MS.CardSavrDelegate.Data
{
    /// <summary>
    /// TODO: add xml doc comments
    /// </summary>
    [DataContract(IsReference = true)]
    public class CardholderGrant
    {
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string Grant { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string CUID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string CardId { get; set; }
    }
}