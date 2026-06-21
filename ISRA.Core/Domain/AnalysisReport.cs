using System.Collections.Generic;
using System.Linq;

namespace ISRA.Core.Domain
{
    /// <summary>
    /// Represents a complete analysis report containing multiple validation results.
    /// </summary>
    public class AnalysisReport
    {
        /// <summary>
        /// Collection of individual validation results.
        /// </summary>
        public List<IValidationResult> ValidationResults { get; set; }

        /// <summary>
        /// True if all validations passed.
        /// </summary>
        public bool IsValid => ValidationResults != null && ValidationResults.All(r => r.IsValid);

        /// <summary>
        /// Count of passed validations.
        /// </summary>
        public int PassedCount => ValidationResults?.Count(r => r.IsValid) ?? 0;

        /// <summary>
        /// Count of failed validations.
        /// </summary>
        public int FailedCount => ValidationResults?.Count(r => !r.IsValid) ?? 0;

        /// <summary>
        /// Total count of validations.
        /// </summary>
        public int TotalCount => ValidationResults?.Count ?? 0;

        public AnalysisReport()
        {
            ValidationResults = new List<IValidationResult>();
        }

        public AnalysisReport(List<IValidationResult> results)
        {
            ValidationResults = results ?? new List<IValidationResult>();
        }

        /// <summary>
        /// Adds a validation result to the report.
        /// </summary>
        public void AddResult(IValidationResult result)
        {
            if (ValidationResults == null)
                ValidationResults = new List<IValidationResult>();
            ValidationResults.Add(result);
        }
    }
}
