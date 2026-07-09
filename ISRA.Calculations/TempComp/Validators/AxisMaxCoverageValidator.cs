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

            // Find body max (for comparison: absolute/normalized; for display: actual value)
            double bodyMaxAbs = double.MinValue;
            double bodyMaxDisplay = double.MinValue;  // ← visszakerül J4/J6-hoz
            double bodyMaxPositive = double.MinValue;
            double bodyMinNegative = double.MaxValue;
            foreach (var pose in input.BodyPoses)
            {
                double value = _selector(pose);
                if (_axisName == "J4" || _axisName == "J6")
                    value = config.NormalizeAngle180(value);
                double absValue = _useAbsoluteValue ? Math.Abs(value) : value;
                if (absValue > bodyMaxAbs)
                {
                    bodyMaxAbs = absValue;
                    bodyMaxDisplay = value;  // ← J4/J6-hoz
                }
                if (_axisName == "J5" && value > bodyMaxPositive) bodyMaxPositive = value;
                if (_axisName == "J5" && value < bodyMinNegative) bodyMinNegative = value;
            }

            // Find TC max and check if it covers body max
            double tcMaxAbs = double.MinValue;
            double tcMaxDisplay = double.MinValue;    // ← visszakerül J4/J6-hoz
            double tcMaxPositive = double.MinValue;
            double tcMinNegative = double.MaxValue;
            int coveringCount = 0;
            foreach (var pose in input.TempCompPoses)
            {
                double value = _selector(pose);
                if (_axisName == "J4" || _axisName == "J6")
                    value = config.NormalizeAngle180(value);
                double absValue = _useAbsoluteValue ? Math.Abs(value) : value;
                if (absValue > tcMaxAbs)
                {
                    tcMaxAbs = absValue;
                    tcMaxDisplay = value;  // ← J4/J6-hoz
                }
                if (_axisName == "J5" && value > tcMaxPositive) tcMaxPositive = value;
                if (_axisName == "J5" && value < tcMinNegative) tcMinNegative = value;
                if (absValue >= bodyMaxAbs) coveringCount++;
            }

            // Build display strings
            string bodypartStr = _axisName == "J5"
                ? $"Body max: +{bodyMaxPositive:F1}° / {bodyMinNegative:F1}°"
                : $"Body max: {bodyMaxDisplay:F1}°";

            string tempCompStr = _axisName == "J5"
                ? $"TC max: +{tcMaxPositive:F1}° / {tcMinNegative:F1}°"
                : $"TC max: {tcMaxDisplay:F1}°";
            bool covered = coveringCount >= 2;
            return CreateResult(
                Name, covered,
                covered
                    ? $"TC covers body maximum ({coveringCount} points)"
                    : $"TC coverage insufficient: {coveringCount} point(s) reach body max (need ≥2)",
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
