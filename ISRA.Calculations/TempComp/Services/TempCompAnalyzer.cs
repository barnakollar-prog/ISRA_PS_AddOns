using System.Collections.Generic;
using ISRA.Core.Domain;
using ISRA.Calculations.TempComp.Domain;
using ISRA.Calculations.TempComp.Domain.Results;
using ISRA.Calculations.TempComp.Validators;

namespace ISRA.Calculations.TempComp.Services
{
    /// <summary>
    /// Main analyzer service that orchestrates all TempComp validation checks.
    /// </summary>
    public class TempCompAnalyzer : IAnalyzer<TempCompValidationInput, AnalysisReport>
    {
        private readonly List<TempCompValidator> _validators;

        public TempCompAnalyzer()
        {
            _validators = new List<TempCompValidator>
            {
                new J23AngleCoverageValidator(),
                new J23RangeValidator(),
                new J5SymmetryValidator(),
                AxisMaxCoverageValidator.CreateJ4Validator(),
                AxisMaxCoverageValidator.CreateJ5Validator(),
                AxisMaxCoverageValidator.CreateJ6Validator()
            };
        }

        /// <summary>
        /// Performs complete TempComp validation analysis.
        /// </summary>
        /// <param name="input">Validation input containing body poses, temp comp poses, and robot configuration</param>
        /// <returns>Analysis report with all validation results</returns>
        public AnalysisReport Analyze(TempCompValidationInput input)
        {
            var report = new AnalysisReport();

            foreach (var validator in _validators)
            {
                var result = validator.Validate(input);
                report.AddResult(result);
            }

            return report;
        }

        /// <summary>
        /// Calculates nearest TC points for all body poses.
        /// </summary>
        public List<NearestTcResult> CalculateNearestTcPoints(TempCompValidationInput input)
        {
            var calculator = new DistanceCalculator(input.RobotConfiguration);
            return calculator.FindNearestForAll(input.BodyPoses, input.TempCompPoses);
        }

        /// <summary>
        /// Calculates statistical summary for a collection of poses.
        /// </summary>
        public PoseStatistics CalculateStatistics(List<RobotPose> poses)
        {
            var calculator = new PoseStatisticsCalculator();
            return calculator.Calculate(poses);
        }
    }
}
