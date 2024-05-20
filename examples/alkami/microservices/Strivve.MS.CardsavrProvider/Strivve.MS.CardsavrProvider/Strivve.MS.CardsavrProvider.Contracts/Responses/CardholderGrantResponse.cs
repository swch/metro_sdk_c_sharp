using Alkami.Contracts;
using Strivve.MS.CardsavrProvider.Data;
using System.Runtime.Serialization;

namespace Strivve.MS.CardsavrProvider.Contracts.Responses
{
    /// <summary>
    /// Our response object inherits a base object that takes a type argument of the data we want to return
    /// We can add additional properties to this class that may be helpful, it's up to the discretion of the developer
    /// </summary>
    [DataContract(IsReference = true)]
    public class CardholderGrantResponse : BaseResponse<CardholderGrant>
    {

    }
}
