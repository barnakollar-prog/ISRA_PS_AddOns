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
        public string BodypartValue { get; set; }    // ← ÚJ
        public string TempCompValue { get; set; }    // ← ÚJ

        public bool IsValid => Status == ValidationStatus.Valid;

        public ValidationResult()
        {
            Details = string.Empty;
            BodypartValue = string.Empty;    // ← ÚJ
            TempCompValue = string.Empty;    // ← ÚJ
        }

        public ValidationResult(string criterionName, bool isValid, string message, string details = "")
        {
            CriterionName = criterionName;
            Status = isValid ? ValidationStatus.Valid : ValidationStatus.Invalid;
            Message = message;
            Details = details ?? string.Empty;
            BodypartValue = string.Empty;    // ← ÚJ
            TempCompValue = string.Empty;    // ← ÚJ
        }

        public ValidationResult(string criterionName, ValidationStatus status, string message, string details = "")
        {
            CriterionName = criterionName;
            Status = status;
            Message = message;
            Details = details ?? string.Empty;
            BodypartValue = string.Empty;    // ← ÚJ
            TempCompValue = string.Empty;    // ← ÚJ
        }
    }
}
