using System;
using ISRA.Core.Domain;
using ISRA.Calculations.TempComp.Domain;

namespace ISRA.Calculations.TempComp.Validators
{
    /// <summary>
    /// Validates that temp comp poses span a sufficient range of J2-3 angles.
    /// Criterion: J2-3 range must be at least 75 degrees.
    /// </summary>
    public class J23RangeValidator : TempCompValidator
    {
        private const double MinRequiredRange = 75.0;

        public override string Name => "J2-3 Range ≥ 75°";

        public override IValidationResult Validate(TempCompValidationInput input)
        {
            if (input.TempCompPoses == null || input.TempCompPoses.Count == 0)
                return CreateResult(Name, false, "No temp comp poses provided", "");

            var config = input.RobotConfiguration;

            double min = double.MaxValue;
            double max = double.MinValue;

            foreach (var pose in input.TempCompPoses)
            {
                double angle = config.CalculateJ23Angle(pose);
                if (angle < min) min = angle;
                if (angle > max) max = angle;
            }

            double range = max - min;
            bool isValid = range >= MinRequiredRange;

            string message = $"TC range: {range:F1}°";
            string details = isValid 
                ? $"Range {range:F1}° meets minimum {MinRequiredRange}°" 
                : $"Range {range:F1}° is below minimum {MinRequiredRange}°";

            return CreateResult(Name, isValid, message, details);
        }
    }
}
