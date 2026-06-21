using System;
using System.Linq;
using ISRA.Core.Domain;
using ISRA.Calculations.TempComp.Domain;

namespace ISRA.Calculations.TempComp.Validators
{
    /// <summary>
    /// Validates that temp comp poses cover the maximum and minimum J2-3 angles found in body poses.
    /// Criterion: At least 2 TC poses must reach body max, and at least 2 must reach body min.
    /// </summary>
    public class J23AngleCoverageValidator : TempCompValidator
    {
        public override string Name => "J2-3 Angle Coverage";

        public override IValidationResult Validate(TempCompValidationInput input)
        {
            if (input.BodyPoses == null || input.BodyPoses.Count == 0)
                return CreateResult(Name, false, "No body poses provided", "");

            if (input.TempCompPoses == null || input.TempCompPoses.Count == 0)
                return CreateResult(Name, false, "No temp comp poses provided", "");

            var config = input.RobotConfiguration;

            // Find body max and min J2-3 angles
            double bodyMax = double.MinValue;
            double bodyMin = double.MaxValue;

            foreach (var pose in input.BodyPoses)
            {
                double angle = config.CalculateJ23Angle(pose);
                if (angle > bodyMax) bodyMax = angle;
                if (angle < bodyMin) bodyMin = angle;
            }

            // Count how many TC poses cover the extremes
            int countMax = 0, countMin = 0;
            foreach (var pose in input.TempCompPoses)
            {
                double angle = config.CalculateJ23Angle(pose);
                if (angle >= bodyMax) countMax++;
                if (angle <= bodyMin) countMin++;
            }

            bool maxOK = countMax >= 2;
            bool minOK = countMin >= 2;
            bool isValid = maxOK && minOK;

            string message = $"Body range: {bodyMin:F1}° to {bodyMax:F1}°";
            string details = $"TC covering max: {countMax}, TC covering min: {countMin}";

            if (!maxOK && !minOK)
                details += " (Need ≥2 for both)";
            else if (!maxOK)
                details += " (Need ≥2 for max)";
            else if (!minOK)
                details += " (Need ≥2 for min)";

            return CreateResult(Name, isValid, message, details);
        }
    }
}
