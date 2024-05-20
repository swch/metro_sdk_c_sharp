using Alkami.Data.Validations;
using Strivve.MS.CardsavrProvider.Contracts.Filters;
using System.Collections.Generic;

namespace Strivve.MS.CardsavrProvider.Data.Validations
{
    /// This is a basic example of a validation that we've provided in this template.  Each validator
	/// implements the abstract class <![CDATA[EntityValidatorImpl<T>]]> and passes in the request object to be validated.
	/// What you will see implemented below is an override of our ValidateInternal method.  This method can be
	/// used to validate any number of members in the request object and will return a list of validation results.
	/// Each result can be as specific as desired for the member being validated and will be shown in the
	/// ValidationResults of the response object.
    public class CustomDataObjectFilterValidator : EntityValidatorImpl<CustomDataObjectFilter>
    {
        /// <summary>
        /// Override the ValidateInternal method to define custom validations for this particular type of request.
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        protected override List<ValidationResult> ValidateInternal(CustomDataObjectFilter src)
        {
            var results = new List<ValidationResult>();

            if (src.Ids == null)
            {
                results.Add(new ValidationResult()
                {
                    ErrorCode = ErrorCode.ValidationError,
                    Field = "Filter",
                    Message = "At minimum an empty filter is required. An empty one has been created by default.",
                    Severity = Severity.Warning,
                    SubCode = SubCode.MalformedRequest
                });
            }

            return results;
        }
    }
}
