using Alkami.Data.Validations;
using Strivve.MS.CardSavrProviderService.Contracts.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strivve.MS.CardSavrProviderService.Data.Validations
{
    /// This is a basic example of a validation that we've provided in this template.  Each validator
	/// implements the abstract class EntityValidatorImpl<T> and passes in the request object to be validated.
	/// What you will see implemented below is an override of our ValidateInternal method.  This method can be
	/// used to validate any number of members in the request object and will return a list of validation results.
	/// Each result can be as specific as desired for the member being validated and will be shown in the
	/// ValidationResults of the response object.
    public class AddOrUpdateSomethingRequestValidator : EntityValidatorImpl<AddOrUpdateSomethingRequest>
    {
        /// <summary>
        /// Override the ValidateInternal method to define custom validations for this particular type of request.
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        protected override List<ValidationResult> ValidateInternal(AddOrUpdateSomethingRequest src)
        {
            var results = new List<ValidationResult>();

            if (src?.ItemList?.Count > 1 || src?.ItemList?.Count < 1)
            {
                results.Add(new ValidationResult()
                {
                    ErrorCode = ErrorCode.ValidationError,
                    Field = "ItemList",
                    Message = "The update item list must have a single value to update.",
                    Severity = Severity.Error,
                    SubCode = SubCode.BadRequest
                });
            }

            return results;
        }
    }
}
