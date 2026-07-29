using System;
using System.Collections.Generic;
using System.Linq;
using ISRA.Calculations.TempComp.Domain;
using ISRA.Calculations.TempComp.Domain.Results;
using ISRA.Calculations.TempComp.RobotConfiguration;
using ISRA.Core.Domain;

namespace ISRA.Calculations.TempComp.Services
{
    /// <summary>
    /// Rule-based evaluation service that analyzes TempComp validation
    /// and gap analysis results to generate concrete optimization suggestions.
    /// </summary>
    public class TempCompEvaluationService
    {
        public EvaluationResult Evaluate(
            AnalysisReport report,
            GapAnalysisResult gapResult,
            List<RobotPose> bodyPoses,
            List<RobotPose> tcPoses,
            IRobotConfiguration config)
        {
            var result = new EvaluationResult();
            result.IsValid = report.IsValid && gapResult.AllAxes.All(a => a.Threshold <= 0 || a.IsValid);

            // Evaluate validation criteria
            foreach (var vr in report.ValidationResults)
            {
                if (vr.IsValid) continue;

                switch (vr.CriterionName)
                {
                    case "J2-3 Angle Coverage":
                        EvaluateJ23Coverage(vr, bodyPoses, tcPoses, config, result.Findings);
                        break;
                    case "J2-3 Range":
                        EvaluateJ23Range(vr, tcPoses, config, result.Findings);
                        break;
                    case "J5 Symmetry":
                        EvaluateJ5Symmetry(vr, tcPoses, result.Findings);
                        break;
                    case "J4 Max Coverage":
                        EvaluateAxisCoverage("J4", vr, bodyPoses, tcPoses,
                            p => p.J4, result.Findings);
                        break;
                    case "J5 Max Coverage":
                        EvaluateAxisCoverage("J5", vr, bodyPoses, tcPoses,
                            p => p.J5, result.Findings);
                        break;
                    case "J6 Max Coverage":
                        EvaluateAxisCoverage("J6", vr, bodyPoses, tcPoses,
                            p => p.J6, result.Findings);
                        break;
                }
            }

            // Evaluate gap analysis
            foreach (var axis in gapResult.AllAxes)
            {
                if (axis.Threshold <= 0 || axis.IsValid) continue;
                EvaluateGap(axis, tcPoses, result.Findings);
            }

            // Build summary
            result.Summary = BuildSummary(result);

            return result;
        }

        // ── J2-3 Angle Coverage ──────────────────────────────
        private void EvaluateJ23Coverage(
            IValidationResult vr,
            List<RobotPose> bodyPoses,
            List<RobotPose> tcPoses,
            IRobotConfiguration config,
            List<EvaluationFinding> findings)
        {
            double bodyMax = bodyPoses.Max(p => config.CalculateJ23Angle(p));
            double bodyMin = bodyPoses.Min(p => config.CalculateJ23Angle(p));

            // Find body pose causing the extreme
            var bodyMaxPose = bodyPoses.OrderByDescending(p => config.CalculateJ23Angle(p)).First();
            var bodyMinPose = bodyPoses.OrderBy(p => config.CalculateJ23Angle(p)).First();

            // Find TC poses closest to body extremes
            var tcByAngle = tcPoses.OrderByDescending(p => config.CalculateJ23Angle(p)).ToList();
            int countMax = tcByAngle.Count(p => config.CalculateJ23Angle(p) >= bodyMax);
            int countMin = tcPoses.Count(p => config.CalculateJ23Angle(p) <= bodyMin);

            if (countMax < 2)
            {
                var bestTc = tcByAngle.First();
                double tcAngle = config.CalculateJ23Angle(bestTc);
                findings.Add(new EvaluationFinding
                {
                    Axis = "J2-3",
                    Issue = $"Only {countMax}/2 TC points reach body max ({bodyMax:F1}°)",
                    TcSuggestion = $"Increase J2-3 angle of '{bestTc.PathName} - {bestTc.Name}' " +
                                   $"from {tcAngle:F1}° to at least {bodyMax:F1}° " +
                                   $"(gap: {bodyMax - tcAngle:F1}°)",
                    BodySuggestion = $"Or: check if '{bodyMaxPose.PathName} - {bodyMaxPose.Name}' " +
                                     $"({bodyMax:F1}°) can be measured from alternative sensor position",
                    Severity = "Critical"
                });
            }

            if (countMin < 2)
            {
                var bestTc = tcPoses.OrderBy(p => config.CalculateJ23Angle(p)).First();
                double tcAngle = config.CalculateJ23Angle(bestTc);
                findings.Add(new EvaluationFinding
                {
                    Axis = "J2-3",
                    Issue = $"Only {countMin}/2 TC points reach body min ({bodyMin:F1}°)",
                    TcSuggestion = $"Decrease J2-3 angle of '{bestTc.PathName} - {bestTc.Name}' " +
                                   $"from {tcAngle:F1}° to at most {bodyMin:F1}° " +
                                   $"(gap: {tcAngle - bodyMin:F1}°)",
                    BodySuggestion = $"Or: check if '{bodyMinPose.PathName} - {bodyMinPose.Name}' " +
                                     $"({bodyMin:F1}°) can be measured from alternative sensor position",
                    Severity = "Critical"
                });
            }
        }

        // ── J2-3 Range ───────────────────────────────────────
        private void EvaluateJ23Range(
            IValidationResult vr,
            List<RobotPose> tcPoses,
            IRobotConfiguration config,
            List<EvaluationFinding> findings)
        {
            double tcMax = tcPoses.Max(p => config.CalculateJ23Angle(p));
            double tcMin = tcPoses.Min(p => config.CalculateJ23Angle(p));
            double range = tcMax - tcMin;

            var maxPose = tcPoses.OrderByDescending(p => config.CalculateJ23Angle(p)).First();
            var minPose = tcPoses.OrderBy(p => config.CalculateJ23Angle(p)).First();

            findings.Add(new EvaluationFinding
            {
                Axis = "J2-3",
                Issue = $"TC J2-3 range is {range:F1}° (minimum required: 75°)",
                TcSuggestion = $"Increase range by {75 - range:F1}°: " +
                               $"increase '{maxPose.PathName} - {maxPose.Name}' above {tcMax + (75 - range) / 2:F1}° " +
                               $"or decrease '{minPose.PathName} - {minPose.Name}' below {tcMin - (75 - range) / 2:F1}°",
                BodySuggestion = null,
                Severity = "Critical"
            });
        }

        // ── J5 Symmetry ──────────────────────────────────────
        private void EvaluateJ5Symmetry(
            IValidationResult vr,
            List<RobotPose> tcPoses,
            List<EvaluationFinding> findings)
        {
            int negCount = tcPoses.Count(p => p.J5 < 0);
            int posCount = tcPoses.Count(p => p.J5 > 0);
            int total = tcPoses.Count;
            int needed = total / 2;

            string suggestion = negCount < posCount
                ? $"Too few negative J5 values ({negCount}/{needed}). " +
                  $"Modify TC points with largest positive J5 to negative values."
                : $"Too few positive J5 values ({posCount}/{needed}). " +
                  $"Modify TC points with largest negative J5 to positive values.";

            findings.Add(new EvaluationFinding
            {
                Axis = "J5",
                Issue = $"J5 not symmetric: {negCount} negative, {posCount} positive (total: {total})",
                TcSuggestion = suggestion,
                BodySuggestion = null,
                Severity = "Warning"
            });
        }

        // ── Axis Max Coverage (J4/J5/J6) ─────────────────────
        private void EvaluateAxisCoverage(
            string axisName,
            IValidationResult vr,
            List<RobotPose> bodyPoses,
            List<RobotPose> tcPoses,
            Func<RobotPose, double> selector,
            List<EvaluationFinding> findings)
        {
            double bodyMaxPos = bodyPoses.Max(selector);
            double bodyMinNeg = bodyPoses.Min(selector);

            var bodyMaxPose = bodyPoses.OrderByDescending(selector).First();
            var bodyMinPose = bodyPoses.OrderBy(selector).First();

            int countPos = tcPoses.Count(p => selector(p) >= bodyMaxPos);
            int countNeg = tcPoses.Count(p => selector(p) <= bodyMinNeg);

            if (countPos < 2)
            {
                var bestTc = tcPoses.OrderByDescending(selector).First();
                double tcVal = selector(bestTc);
                findings.Add(new EvaluationFinding
                {
                    Axis = axisName,
                    Issue = $"Only {countPos}/2 TC points reach body positive max ({bodyMaxPos:F1}°)",
                    TcSuggestion = $"Increase {axisName} of '{bestTc.PathName} - {bestTc.Name}' " +
                                   $"from {tcVal:F1}° to at least {bodyMaxPos:F1}° " +
                                   $"(needs +{bodyMaxPos - tcVal:F1}°)",
                    BodySuggestion = $"Or: check if '{bodyMaxPose.PathName} - {bodyMaxPose.Name}' " +
                                     $"({bodyMaxPos:F1}°) can be measured from alternative sensor position",
                    Severity = "Critical"
                });
            }

            if (countNeg < 2)
            {
                var bestTc = tcPoses.OrderBy(selector).First();
                double tcVal = selector(bestTc);
                findings.Add(new EvaluationFinding
                {
                    Axis = axisName,
                    Issue = $"Only {countNeg}/2 TC points reach body negative min ({bodyMinNeg:F1}°)",
                    TcSuggestion = $"Decrease {axisName} of '{bestTc.PathName} - {bestTc.Name}' " +
                                   $"from {tcVal:F1}° to at most {bodyMinNeg:F1}° " +
                                   $"(needs {bodyMinNeg - tcVal:F1}°)",
                    BodySuggestion = $"Or: check if '{bodyMinPose.PathName} - {bodyMinPose.Name}' " +
                                     $"({bodyMinNeg:F1}°) can be measured from alternative sensor position",
                    Severity = "Critical"
                });
            }
        }

        // ── Gap Analysis ─────────────────────────────────────
        private void EvaluateGap(
            AxisGapResult axis,
            List<RobotPose> tcPoses,
            List<EvaluationFinding> findings)
        {
            // Find the largest gap
            double maxGap = 0;
            int maxGapIdx = -1;
            for (int i = 0; i < axis.SortedValues.Count - 1; i++)
            {
                double gap = axis.SortedValues[i + 1].Value - axis.SortedValues[i].Value;
                if (gap > maxGap)
                {
                    maxGap = gap;
                    maxGapIdx = i;
                }
            }

            if (maxGapIdx < 0) return;

            var lower = axis.SortedValues[maxGapIdx];
            var upper = axis.SortedValues[maxGapIdx + 1];
            double midpoint = (lower.Value + upper.Value) / 2.0;

            // Find TC pose closest to midpoint
            var closestTc = tcPoses
                .OrderBy(p => Math.Abs(GetAxisValue(p, axis.AxisName) - lower.Value))
                .FirstOrDefault();

            string closestName = closestTc != null
                ? $"'{closestTc.PathName} - {closestTc.Name}'"
                : "a TC point";

            findings.Add(new EvaluationFinding
            {
                Axis = axis.AxisName,
                Issue = $"Gap of {maxGap:F1}° detected between {lower.Value:F1}° " +
                        $"({lower.PathName} - {lower.PoseName}) " +
                        $"and {upper.Value:F1}° ({upper.PathName} - {upper.PoseName})",
                TcSuggestion = $"Modify {closestName} to approximately {midpoint:F1}° " +
                               $"to close the {maxGap:F1}° gap (threshold: {axis.Threshold:F0}°)",
                BodySuggestion = null,
                Severity = maxGap > axis.Threshold * 2 ? "Critical" : "Warning"
            });
        }

        private double GetAxisValue(RobotPose pose, string axisName)
        {
            switch (axisName)
            {
                case "J2": return pose.J2;
                case "J3": return pose.J3;
                case "J4": return pose.J4;
                case "J5": return pose.J5;
                case "J6": return pose.J6;
                default: return 0;
            }
        }

        // ── Summary ──────────────────────────────────────────
        private string BuildSummary(EvaluationResult result)
        {
            if (result.IsValid)
                return "All validation criteria and gap thresholds are met. " +
                       "The TempComp program adequately covers the Bodypart measurement range.";

            int critical = result.Findings.Count(f => f.Severity == "Critical");
            int warnings = result.Findings.Count(f => f.Severity == "Warning");

            return $"Analysis found {result.Findings.Count} issue(s): " +
                   $"{critical} critical, {warnings} warning(s). " +
                   $"Review the suggestions below to optimize the TempComp program. " +
                   $"Modifications can be made on TC points (sensor position adjustment) " +
                   $"or on Body points (alternative measurement position).";
        }
    }
}