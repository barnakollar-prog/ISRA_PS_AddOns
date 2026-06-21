using ISRA.Core.Domain;
using ISRA.Calculations.AccuSite.Domain;
using ISRA.Calculations.AccuSite.Validators;
using System.Collections.Generic;

namespace ISRA.Calculations.AccuSite.Services
{
    /// <summary>
    /// Main analyzer service for AccuSite visibility analysis.
    /// Orchestrates all visibility validation checks.
    /// </summary>
    public class VisibilityAnalyzer : IAnalyzer<VisibilityAnalysisInput, AnalysisReport>
    {
        private readonly List<AccuSiteValidator> _validators;

        public VisibilityAnalyzer()
        {
            _validators = new List<AccuSiteValidator>
            {
                new EmitterVisibilityValidator()
                // Additional validators can be added here as they're implemented
            };
        }

        /// <summary>
        /// Performs complete visibility analysis.
        /// </summary>
        public AnalysisReport Analyze(VisibilityAnalysisInput input)
        {
            var report = new AnalysisReport();

            foreach (var validator in _validators)
            {
                var result = validator.Validate(input);
                report.AddResult(result);
            }

            return report;
        }
    }
}
