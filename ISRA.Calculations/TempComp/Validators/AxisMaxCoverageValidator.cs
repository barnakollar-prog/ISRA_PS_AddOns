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

            // Find body max
            double bodyMax = double.MinValue;
            foreach (var pose in input.BodyPoses)
            {
                double value = _selector(pose);
                if (_useAbsoluteValue) value = Math.Abs(value);
                if (value > bodyMax) bodyMax = value;
            }

            // Find TC max and check if it covers body max
            double tcMax = double.MinValue;
            bool covered = false;
            foreach (var pose in input.TempCompPoses)
            {
                double value = _selector(pose);
                if (_useAbsoluteValue) value = Math.Abs(value);
                if (value > tcMax) tcMax = value;
                if (value >= bodyMax) covered = true;
            }

            string message = $"Body max: {bodyMax:F1}°, TC max: {tcMax:F1}°";
            string details = covered 
                ? "TC covers body maximum" 
                : $"TC does not reach body maximum (gap: {bodyMax - tcMax:F1}°)";

            return CreateResult(Name, covered, message, details);
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
