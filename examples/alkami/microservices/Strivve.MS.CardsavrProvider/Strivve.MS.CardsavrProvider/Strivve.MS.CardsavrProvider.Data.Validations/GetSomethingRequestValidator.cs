using Alkami.Data.Validations;
using Strivve.MS.CardsavrProvider.Contracts.Filters;
using Strivve.MS.CardsavrProvider.Contracts.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strivve.MS.CardsavrProvider.Data.Validations
{
    /// This is a basic example of a validation that we've provided in this template.  Each validator
	/// implements the abstract class EntityValidatorImpl<T> and passes in the request object to be validated.
	/// What you will see implemented below is an override of our ValidateInternal method.  This method can be
	/// used to validate any number of members in the request object and will return a list of validation results.
	/// Each result can be as specific as desired for the member being validated and will be shown in the
	/// ValidationResults of the response object.
    public class GetSomethingRequestValidator : EntityValidatorImpl<GetSomethingRequest>
    {
        /// <summary>
        /// Override the ValidateInternal method to define custom validations for this particular type of request.
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        protected override List<ValidationResult> ValidateInternal(GetSomethingRequest src)
        {
            var results = new List<ValidationResult>();

            if (src.Filter?.Ids == null)
            {
                // Force corrections within the validations
                src.Filter = new CustomDataObjectFilter
                {
                    Ids = new List<long>()
                };

                results.Add(new ValidationResult()
                {
                    ErrorCode = ErrorCode.ValidationError,
                    Field = "Filter",
                    Message = "At minimum an empty filter is required. An empty one has been created by default.",
                    Severity = Severity.Warning,
                    SubCode = SubCode.MalformedRequest
                });
            }

            if (src.MaxResults > 20)
            {
                // Force corrections within the validations
                src.MaxResults = 20;

                // Explain what has been corrected, why. Maybe we have some hard limit, or we never want to return more than 20 SSN's. 
                results.Add(new ValidationResult()
                {
                    ErrorCode = ErrorCode.ValidationError,
                    Field = "MaxResults",
                    Message = "The max results should not be greater than what we're comfortable sending back. Results limited to 20.",
                    Severity = Severity.Warning,
                    SubCode = SubCode.ValueOutOfRange
                });
            }

            if (src.Filter.Ids.Count == 0)
            {
                results.Add(new ValidationResult
                {
                    ErrorCode = ErrorCode.ValidationError,
                    Field = "Filter",
                    Message = "The filter contains no Id's, results will be limited to the default MaxResults (20).",
                    Severity = Severity.Warning,
                    SubCode = SubCode.InvalidFormat
                });
            }

            return results;
        }
    }
}
