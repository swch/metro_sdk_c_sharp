using Alkami.Contracts;
using Strivve.MS.CardsavrProvider.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Strivve.MS.CardsavrProvider.Contracts.Responses
{
    /// <summary>
    /// TODO: add xml doc comments
    /// </summary>
    [DataContract(IsReference = true)]
    public class CustomObjectResponse : BaseResponse<CustomDataObject>
    {

    }
}