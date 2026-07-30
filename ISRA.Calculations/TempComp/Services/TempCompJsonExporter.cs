using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ISRA.Calculations.TempComp.Domain.Results;
using ISRA.Core.Domain;
using ISRA.Calculations.TempComp.RobotConfiguration;

namespace ISRA.Calculations.TempComp.Services
{
    /// <summary>
    /// Exports TempComp analysis results to a compact JSON string
    /// suitable for Copilot CLI prompt input.
    /// Only NOK validation results and gap issues are included.
    /// </summary>
    public class TempCompJsonExporter
    {
        public string ExportForEvaluation(
            AnalysisReport report,
            GapAnalysisResult gapResult,
            string robotType)
        {
            var sb = new StringBuilder();
            sb.AppendLine("{");
            sb.AppendLine($"  \"robot_type\": \"{robotType}\",");

            // ── Validation Issues (NOK only) ──────────────────
            sb.AppendLine("  \"validation_issues\": [");
            var nokResults = report.ValidationResults
                .Where(r => !r.IsValid)
                .ToList();

            for (int i = 0; i < nokResults.Count; i++)
            {
                var r = nokResults[i];
                string comma = i < nokResults.Count - 1 ? "," : "";
                sb.AppendLine("    {");
                sb.AppendLine($"      \"criterion\": \"{Escape(r.CriterionName)}\",");
                sb.AppendLine($"      \"bodypart\": \"{Escape(r.BodypartValue)}\",");
                sb.AppendLine($"      \"temp_comp\": \"{Escape(r.TempCompValue)}\",");
                sb.AppendLine($"      \"details\": \"{Escape(r.Message)}\"");
                sb.AppendLine("    }" + comma);
            }
            sb.AppendLine("  ],");

            // ── Gap Issues (threshold exceeded only) ──────────
            sb.AppendLine("  \"gap_issues\": [");
            var gapIssues = gapResult.AllAxes
                .Where(a => a.Threshold > 0 && !a.IsValid)
                .ToList();

            for (int i = 0; i < gapIssues.Count; i++)
            {
                var axis = gapIssues[i];
                string comma = i < gapIssues.Count - 1 ? "," : "";

                // Find the largest gap
                double maxGap = 0;
                double lowerVal = 0, upperVal = 0;
                string lowerPoint = "", upperPoint = "";
                for (int j = 0; j < axis.SortedValues.Count - 1; j++)
                {
                    double gap = axis.SortedValues[j + 1].Value - axis.SortedValues[j].Value;
                    if (gap > maxGap)
                    {
                        maxGap = gap;
                        lowerVal = axis.SortedValues[j].Value;
                        upperVal = axis.SortedValues[j + 1].Value;
                        lowerPoint = $"{axis.SortedValues[j].PathName} - {axis.SortedValues[j].PoseName}";
                        upperPoint = $"{axis.SortedValues[j + 1].PathName} - {axis.SortedValues[j + 1].PoseName}";
                    }
                }

                sb.AppendLine("    {");
                sb.AppendLine($"      \"axis\": \"{axis.AxisName}\",");
                sb.AppendLine($"      \"max_gap_deg\": {maxGap:F1},");
                sb.AppendLine($"      \"threshold_deg\": {axis.Threshold:F0},");
                sb.AppendLine($"      \"gap_between\": \"{lowerVal:F1} deg ({Escape(lowerPoint)}) and {upperVal:F1} deg ({Escape(upperPoint)})\"");
                sb.AppendLine("    }" + comma);
            }
            sb.AppendLine("  ]");

            sb.AppendLine("}");
            return sb.ToString();
        }

        private static string Escape(string s)
        {
            if (s == null) return "";
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }
    }
}