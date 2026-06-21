using ISRA.Core.Domain;
using ISRA.Calculations.AccuSite.Domain;

namespace ISRA.Calculations.AccuSite.Validators
{
    /// <summary>
    /// Abstract base class for AccuSite validation criteria.
    /// </summary>
    public abstract class AccuSiteValidator : IValidator<VisibilityAnalysisInput, IValidationResult>
    {
        /// <summary>
        /// Name of the validation criterion.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Performs the validation check.
        /// </summary>
        /// <param name="input">Visibility analysis input</param>
        /// <returns>Validation result</returns>
        public abstract IValidationResult Validate(VisibilityAnalysisInput input);

        /// <summary>
        /// Helper: Creates a validation result.
        /// </summary>
        protected ValidationResult CreateResult(string criterionName, bool isValid, string message, string details = "")
        {
            return new ValidationResult(criterionName, isValid, message, details);
        }
    }
}
