namespace ISRA.Core.Domain
{
    /// <summary>
    /// Represents the status of a validation check.
    /// </summary>
    public enum ValidationStatus
    {
        /// <summary>Validation passed all criteria</summary>
        Valid,

        /// <summary>Validation failed one or more criteria</summary>
        Invalid,

        /// <summary>Validation passed with warnings</summary>
        Warning,

        /// <summary>Validation could not be performed</summary>
        NotApplicable
    }
}
