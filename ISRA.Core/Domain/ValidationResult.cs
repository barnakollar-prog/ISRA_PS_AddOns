namespace ISRA.Core.Domain
{
    /// <summary>
    /// Base implementation of IValidationResult.
    /// </summary>
    public class ValidationResult : IValidationResult
    {
        public string CriterionName { get; set; }
        public ValidationStatus Status { get; set; }
        public string Message { get; set; }
        public string Details { get; set; }

        public bool IsValid => Status == ValidationStatus.Valid;

        public ValidationResult()
        {
            Details = string.Empty;
        }

        public ValidationResult(string criterionName, bool isValid, string message, string details = "")
        {
            CriterionName = criterionName;
            Status = isValid ? ValidationStatus.Valid : ValidationStatus.Invalid;
            Message = message;
            Details = details ?? string.Empty;
        }

        public ValidationResult(string criterionName, ValidationStatus status, string message, string details = "")
        {
            CriterionName = criterionName;
            Status = status;
            Message = message;
            Details = details ?? string.Empty;
        }
    }
}
