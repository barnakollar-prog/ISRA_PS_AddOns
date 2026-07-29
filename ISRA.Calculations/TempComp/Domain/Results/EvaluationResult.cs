using System.Collections.Generic;

namespace ISRA.Calculations.TempComp.Domain.Results
{
    /// <summary>
    /// Single evaluation finding for one axis/criterion.
    /// </summary>
    public class EvaluationFinding
    {
        /// <summary>Axis name (J2-3, J4, J5, J6) or criterion name</summary>
        public string Axis { get; set; }

        /// <summary>Short issue description</summary>
        public string Issue { get; set; }

        /// <summary>Suggested TC modification with concrete values</summary>
        public string TcSuggestion { get; set; }

        /// <summary>Optional: Body point that causes the extreme range</summary>
        public string BodySuggestion { get; set; }

        /// <summary>Severity: Critical / Warning / Info</summary>
        public string Severity { get; set; }
    }

    /// <summary>
    /// Complete evaluation result containing all findings and a summary.
    /// </summary>
    public class EvaluationResult
    {
        public List<EvaluationFinding> Findings { get; set; }

        /// <summary>Overall text summary</summary>
        public string Summary { get; set; }

        /// <summary>True if all criteria pass</summary>
        public bool IsValid { get; set; }

        public EvaluationResult()
        {
            Findings = new List<EvaluationFinding>();
        }
    }
}