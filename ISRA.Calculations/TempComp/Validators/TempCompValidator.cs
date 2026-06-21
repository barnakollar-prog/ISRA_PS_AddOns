using ISRA.Core.Domain;
using ISRA.Calculations.TempComp.Domain;
using ISRA.Calculations.TempComp.RobotConfiguration;

namespace ISRA.Calculations.TempComp.Validators
{
    /// <summary>
    /// Abstract base class for TempComp validation criteria.
    /// </summary>
    public abstract class TempCompValidator : IValidator<TempCompValidationInput, IValidationResult>
    {
        /// <summary>
        /// Name of the validation criterion.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Performs the validation check.
        /// </summary>
        /// <param name="input">Validation input containing body poses, temp comp poses, and robot configuration</param>
        /// <returns>Validation result</returns>
        public abstract IValidationResult Validate(TempCompValidationInput input);

        /// <summary>
        /// Helper: Creates a validation result.
        /// </summary>
        protected ValidationResult CreateResult(string criterionName, bool isValid, string message, string details = "")
        {
            return new ValidationResult(criterionName, isValid, message, details);
        }
    }
}
