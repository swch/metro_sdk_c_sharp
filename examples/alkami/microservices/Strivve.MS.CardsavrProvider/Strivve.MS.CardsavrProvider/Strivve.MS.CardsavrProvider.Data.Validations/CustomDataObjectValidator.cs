using Alkami.Data.Validations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strivve.MS.CardsavrProvider.Data.Validations
{
    /// This is a basic example of a validation that we've provided in this template.  Each validator
	/// implements the abstract class <![CDATA[EntityValidatorImpl<T>]]> and passes in the request object to be validated.
	/// What you will see implemented below is an override of our ValidateInternal method.  This method can be
	/// used to validate any number of members in the request object and will return a list of validation results.
	/// Each result can be as specific as desired for the member being validated and will be shown in the
	/// ValidationResults of the response object.
    public class CustomDataObjectValidator : EntityValidatorImpl<CustomDataObject>
    {
        /// <summary>
        /// Override the ValidateInternal method to define custom validations for this particular type of request.
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        protected override List<ValidationResult> ValidateInternal(CustomDataObject src)
        {
            var results = new List<ValidationResult>();

            if (src == null)
            {
                results.Add(new ValidationResult()
                {
                    ErrorCode = ErrorCode.ValidationError,
                    Field = "CustomDataObject",
                    Message = "The source object is null, something has gone terribly wrong.",
                    Severity = Severity.Error,
                });
            }
            else
            {
                if (src.ChildrenObjects == null)
                {
                    src.ChildrenObjects = new List<CustomChildObject>();
                    results.Add(new ValidationResult()
                    {
                        ErrorCode = ErrorCode.ValidationError,
                        Field = "Children",
                        Message = "You should have children objects here",
                        Severity = Severity.Warning,
                    });
                }

                if (src.OneOfYourObjects == null)
                {
                    results.Add(new ValidationResult()
                    {
                        ErrorCode = ErrorCode.ValidationError,
                        Field = "OneOfYourObjects",
                        Message = "One of your objects is null. I can put a long descriptive message here for the logs, but the client should see the error code and subcode",
                        Severity = Severity.Error,
                    });
                }
            }

            return results;
        }
    }
}
