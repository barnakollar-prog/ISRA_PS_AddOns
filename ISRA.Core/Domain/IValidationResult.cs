namespace ISRA.Core.Domain
{
    /// <summary>
    /// Represents the result of a validation check.
    /// </summary>
    public interface IValidationResult
    {
        /// <summary>
        /// Name of the validation criterion.
        /// </summary>
        string CriterionName { get; }

        /// <summary>
        /// Status of the validation (Valid, Invalid, Warning, NotApplicable).
        /// </summary>
        ValidationStatus Status { get; }

        /// <summary>
        /// Human-readable message describing the result.
        /// </summary>
        string Message { get; }

        /// <summary>
        /// Additional details about the validation result (optional).
        /// </summary>
        string Details { get; }

        /// <summary>
        /// True if validation passed (Status == Valid).
        /// </summary>
        bool IsValid { get; }

        string BodypartValue { get; }    // ← NEW
        string TempCompValue { get; }    // ← NEW
    }
}
