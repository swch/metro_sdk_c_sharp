using Alkami.Contracts;
using Strivve.MS.CardSavrProviderService.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Strivve.MS.CardSavrProviderService.Contracts.Responses
{
    /// <summary>
    /// TODO: add xml doc comments
    /// </summary>
    [DataContract(IsReference = true)]
    public class CustomObjectResponse : BaseResponse<CustomDataObject>
    {

    }
}