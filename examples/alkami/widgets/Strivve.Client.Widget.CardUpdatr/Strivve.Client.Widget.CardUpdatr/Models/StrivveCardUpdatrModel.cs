using Alkami.Client.Framework.Mvc;
using Alkami.MicroServices.CardManagementProviders.Data;

namespace Strivve.Client.Widget.CardUpdatr.Models
{
    public class StrivveCardUpdatrModel : BaseModel
    {
        /// <summary>
        /// Cardholder grant
        /// </summary>
        public string Grant { get; set; }

        /// <summary>
        /// Card ID
        /// </summary>
        public string CardID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string UserDisplayName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Card Card { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string CardNumberDisplay { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public string ExpirationDateDisplay { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public BillingAddress Address { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Phone { get; set; }

    }
}
