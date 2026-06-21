using ISRA.Core.Domain;
using ISRA.Calculations.TempComp.Domain;

namespace ISRA.Calculations.TempComp.Validators
{
    /// <summary>
    /// Validates that temp comp poses have symmetric J5 distribution (positive and negative).
    /// Criterion: At least half of TC poses should have negative J5, and at least half should have positive J5.
    /// </summary>
    public class J5SymmetryValidator : TempCompValidator
    {
        public override string Name => "J5 Symmetry";

        public override IValidationResult Validate(TempCompValidationInput input)
        {
            if (input.TempCompPoses == null || input.TempCompPoses.Count == 0)
                return CreateResult(Name, false, "No temp comp poses provided", "");

            int negCount = 0, posCount = 0;
            foreach (var pose in input.TempCompPoses)
            {
                if (pose.J5 < 0) negCount++;
                else if (pose.J5 > 0) posCount++;
            }

            int total = input.TempCompPoses.Count;
            int half = total / 2;

            bool isValid = negCount >= half && posCount >= half;

            string message = $"Negative: {negCount}, Positive: {posCount} (Total: {total})";
            string details = isValid 
                ? "J5 distribution is symmetric" 
                : $"Need at least {half} of each sign";

            return CreateResult(Name, isValid, message, details);
        }
    }
}
