using System;
using ISRA.Core.Domain;
using ISRA.Calculations.TempComp.Domain;

namespace ISRA.Calculations.TempComp.Validators
{
    /// <summary>
    /// Validates that temp comp poses cover the maximum axis values found in body poses.
    /// Supports J4, J5, and J6 validation.
    /// </summary>
    public class AxisMaxCoverageValidator : TempCompValidator
    {
        private readonly string _axisName;
        private readonly Func<RobotPose, double> _selector;
        private readonly bool _useAbsoluteValue;

        public override string Name { get; }

        /// <summary>
        /// Creates an axis coverage validator.
        /// </summary>
        /// <param name="axisName">Axis name (e.g., "J4", "J5", "J6")</param>
        /// <param name="selector">Function to extract the axis value from a pose</param>
        /// <param name="useAbsoluteValue">If true, uses absolute value for comparison</param>
        public AxisMaxCoverageValidator(string axisName, Func<RobotPose, double> selector, bool useAbsoluteValue = true)
        {
            _axisName = axisName;
            _selector = selector;
            _useAbsoluteValue = useAbsoluteValue;
            Name = $"{axisName} Max Coverage";
        }

        public override IValidationResult Validate(TempCompValidationInput input)
        {
            if (input.BodyPoses == null || input.BodyPoses.Count == 0)
                return CreateResult(Name, false, "No body poses provided", "");

            if (input.TempCompPoses == null || input.TempCompPoses.Count == 0)
                return CreateResult(Name, false, "No temp comp poses provided", "");

            var config = input.RobotConfiguration;

            // Find body max (positive) and min (negative)
            double bodyMaxPositive = double.MinValue;
            double bodyMinNegative = double.MaxValue;
            foreach (var pose in input.BodyPoses)
            {
                double value = _selector(pose);
                if (value > bodyMaxPositive) bodyMaxPositive = value;
                if (value < bodyMinNegative) bodyMinNegative = value;
            }

            // Count TC points covering positive max and negative min
            int countPositive = 0;
            int countNegative = 0;
            double tcMaxPositive = double.MinValue;
            double tcMinNegative = double.MaxValue;
            foreach (var pose in input.TempCompPoses)
            {
                double value = _selector(pose);
                if (value > tcMaxPositive) tcMaxPositive = value;
                if (value < tcMinNegative) tcMinNegative = value;
                if (value >= bodyMaxPositive) countPositive++;
                if (value <= bodyMinNegative) countNegative++;
            }

            bool coveredPositive = countPositive >= 2;
            bool coveredNegative = countNegative >= 2;
            bool covered = coveredPositive && coveredNegative;

            string posStr = coveredPositive
                ? $"Pos: {countPositive} pts OK"
                : $"Pos: {countPositive} pt(s) NOK";
            string negStr = coveredNegative
                ? $"Neg: {countNegative} pts OK"
                : $"Neg: {countNegative} pt(s) NOK";

            string details = $"{posStr}, {negStr}";

            string bodypartStr = $"Body max: +{bodyMaxPositive:F1}° / {bodyMinNegative:F1}°";
            string tempCompStr = $"TC max: +{tcMaxPositive:F1}° / {tcMinNegative:F1}°";

            return CreateResult(
                Name, covered,
                details,
                "",
                bodypartStr,
                tempCompStr);
        }

        /// <summary>
        /// Factory method for J4 validator (uses absolute value and normalizes angle).
        /// </summary>
        public static AxisMaxCoverageValidator CreateJ4Validator()
        {
            return new AxisMaxCoverageValidator("J4", 
                pose => pose.J4, 
                useAbsoluteValue: true);
        }

        /// <summary>
        /// Factory method for J5 validator (uses absolute value).
        /// </summary>
        public static AxisMaxCoverageValidator CreateJ5Validator()
        {
            return new AxisMaxCoverageValidator("J5", 
                pose => pose.J5, 
                useAbsoluteValue: true);
        }

        /// <summary>
        /// Factory method for J6 validator (uses absolute value and normalizes angle).
        /// </summary>
        public static AxisMaxCoverageValidator CreateJ6Validator()
        {
            return new AxisMaxCoverageValidator("J6", 
                pose => pose.J6, 
                useAbsoluteValue: true);
        }
    }
}
