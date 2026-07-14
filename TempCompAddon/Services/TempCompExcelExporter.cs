using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using ISRA.Core.Domain;
using ISRA.Calculations.TempComp.Domain;
using ISRA.Calculations.TempComp.Domain.Results;
using TempCompAddon.Presentation;

namespace TempCompAddon.Services
{
    /// <summary>
    /// Service for exporting TempComp analysis results to Excel.
    /// Generates 3 worksheets: Validation Results, Nearest TC, and Raw Data.
    /// </summary>
    public static class TempCompExcelExporter
    {
        /// <summary>
        /// Exports TempComp analysis data to an Excel file with 3 sheets.
        /// </summary>
        public static void Export(TempCompExportData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            // Get PS study name for filename
            string studyName = "Unknown_Study";
            string path = Tecnomatix.Engineering.TxApplication.ActiveDocument.FinalDestination;
            if (!string.IsNullOrEmpty(path))
                studyName = Path.GetFileNameWithoutExtension(path);

            // Show save dialog
            var dlg = new SaveFileDialog
            {
                Title = "Export TempComp Analysis to Excel",
                Filter = "Excel files (*.xlsx)|*.xlsx",
                FileName = string.Format("TempComp_Analysis_{0}_{1}",
                    studyName,
                    DateTime.Now.ToString("yyyyMMdd"))
            };

            if (dlg.ShowDialog() != DialogResult.OK) return;

            try
            {
                using (var package = new ExcelPackage())
                {
                    // Sheet 1: Validation Results
                    CreateValidationSheet(package, data);

                    // Sheet 2: Nearest TC
                    CreateNearestTcSheet(package, data);

                    // Sheet 3: Raw Data
                    CreateRawDataSheet(package, data);

                    // Sheet 4: Gap Analysis
                    if (data.GapAnalysis != null)
                        CreateGapAnalysisSheet(package, data);

                    // Save file
                    package.SaveAs(new FileInfo(dlg.FileName));
                }

                MessageBox.Show($"Export completed successfully!\n\nFile saved to:\n{dlg.FileName}",
                    "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export failed:\n\n{ex.Message}",
                    "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Creates the Validation Results worksheet.
        /// </summary>
        private static void CreateValidationSheet(ExcelPackage package, TempCompExportData data)
        {
            var ws = package.Workbook.Worksheets.Add("Validation Results");

            int row = 1;

            // Title
            ws.Cells[row, 1].Value = "TempComp Validation Results";
            ws.Cells[row, 1].Style.Font.Bold = true;
            ws.Cells[row, 1].Style.Font.Size = 14;
            ws.Cells[row, 1, row, 4].Merge = true;
            row += 2;

            // Summary
            ws.Cells[row, 1].Value = "Summary";
            ws.Cells[row, 1].Style.Font.Bold = true;
            row++;

            ws.Cells[row, 1].Value = "Total Validations:";
            ws.Cells[row, 2].Value = data.ValidationReport.TotalCount;
            row++;

            ws.Cells[row, 1].Value = "Passed:";
            ws.Cells[row, 2].Value = data.ValidationReport.PassedCount;
            ws.Cells[row, 2].Style.Font.Color.SetColor(Color.Green);
            ws.Cells[row, 2].Style.Font.Bold = true;
            row++;

            ws.Cells[row, 1].Value = "Failed:";
            ws.Cells[row, 2].Value = data.ValidationReport.FailedCount;
            ws.Cells[row, 2].Style.Font.Color.SetColor(Color.Red);
            ws.Cells[row, 2].Style.Font.Bold = true;
            row++;

            ws.Cells[row, 1].Value = "Overall Status:";
            ws.Cells[row, 2].Value = data.ValidationReport.IsValid ? "PASS" : "FAIL";
            ws.Cells[row, 2].Style.Font.Bold = true;
            ws.Cells[row, 2].Style.Font.Color.SetColor(data.ValidationReport.IsValid ? Color.Green : Color.Red);
            row += 2;

            // Validation details header
            ws.Cells[row, 1].Value = "Validation Details";
            ws.Cells[row, 1].Style.Font.Bold = true;
            ws.Cells[row, 1].Style.Font.Size = 12;
            row += 2;

            // Column headers
            ws.Cells[row, 1].Value = "Criterion";
            ws.Cells[row, 2].Value = "Bodypart";
            ws.Cells[row, 3].Value = "Temp Comp";
            ws.Cells[row, 4].Value = "Message";
            ws.Cells[row, 5].Value = "Status";

            using (var range = ws.Cells[row, 1, row, 5])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }
            row++;

            // Validation results
            foreach (var result in data.ValidationReport.ValidationResults)
            {
                ws.Cells[row, 1].Value = result.CriterionName;
                ws.Cells[row, 2].Value = result.BodypartValue ?? "";
                ws.Cells[row, 3].Value = result.TempCompValue ?? "";
                ws.Cells[row, 4].Value = result.Message ?? "";
                ws.Cells[row, 5].Value = result.IsValid ? "PASS" : "FAIL";
                ws.Cells[row, 5].Style.Font.Bold = true;
                ws.Cells[row, 5].Style.Font.Color.SetColor(result.IsValid ? Color.Green : Color.Red);

                Color rowColor = result.IsValid ? Color.FromArgb(198, 239, 206) : Color.FromArgb(255, 199, 206);
                using (var range = ws.Cells[row, 1, row, 5])
                {
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(rowColor);
                    range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }
                row++;
            }

            // Auto-fit columns
            ws.Column(1).AutoFit();
            ws.Column(2).AutoFit();
            ws.Column(3).AutoFit();
            ws.Column(4).AutoFit();
            ws.Column(5).AutoFit();
        }

        /// <summary>
        /// Creates the Gap Analysis worksheet with sorted TC and Body values per axis,
        /// gap highlighting, status rows, and a line chart for each axis
        /// showing TC vs Body measurement point distribution.
        /// </summary>
        private static void CreateGapAnalysisSheet(ExcelPackage package, TempCompExportData data)
        {
            var ws = package.Workbook.Worksheets.Add("Gap Analysis");
            var gap = data.GapAnalysis;
            var axes = gap.AllAxes;

            // Axis names and corresponding body values
            var bodyValueSelectors = new System.Func<RobotPose, double>[]
            {
        p => p.J2,
        p => p.J3,
        p => p.J4,
        p => p.J5,
        p => p.J6,
        p => data.RobotConfiguration.CalculateJ23Angle(p)
            };

            // Chart colors: TC = blue, Body = red
            var tcColor = Color.FromArgb(68, 114, 196);
            var bpColor = Color.FromArgb(255, 0, 0);

            int startRow = 1;

            for (int axisIdx = 0; axisIdx < axes.Count; axisIdx++)
            {
                var axis = axes[axisIdx];
                var bodySelector = bodyValueSelectors[axisIdx];

                // ── Data table ──────────────────────────────────────
                int dataStartRow = startRow;
                int col = 1;

                // Headers
                ws.Cells[dataStartRow, col].Value = axis.AxisName + " TC Point";
                ws.Cells[dataStartRow, col + 1].Value = axis.AxisName + " TC Value";
                ws.Cells[dataStartRow, col + 2].Value = axis.AxisName + " Body Point";
                ws.Cells[dataStartRow, col + 3].Value = axis.AxisName + " Body Value";

                using (var hdr = ws.Cells[dataStartRow, col, dataStartRow, col + 3])
                {
                    hdr.Style.Font.Bold = true;
                    hdr.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    hdr.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                }

                // TC data (sorted)
                for (int i = 0; i < axis.SortedValues.Count; i++)
                {
                    var entry = axis.SortedValues[i];
                    ws.Cells[dataStartRow + 1 + i, col].Value = $"{entry.PathName} - {entry.PoseName}";
                    ws.Cells[dataStartRow + 1 + i, col + 1].Value = entry.Value;
                    ws.Cells[dataStartRow + 1 + i, col + 1].Style.Numberformat.Format = "0.00";

                    // Gap row color
                    if (axis.GapIndices.Contains(i))
                    {
                        ws.Cells[dataStartRow + 1 + i, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[dataStartRow + 1 + i, col].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 199, 206));
                        ws.Cells[dataStartRow + 1 + i, col + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[dataStartRow + 1 + i, col + 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 199, 206));
                    }
                }

                // Body data (sorted)
                var bodyValues = new List<(double Value, string Name)>();
                foreach (var pose in data.BodyPoses)
                    bodyValues.Add((bodySelector(pose), pose.Name));
                bodyValues.Sort((a, b) => a.Value.CompareTo(b.Value));

                for (int i = 0; i < bodyValues.Count; i++)
                {
                    ws.Cells[dataStartRow + 1 + i, col + 2].Value = bodyValues[i].Name;
                    ws.Cells[dataStartRow + 1 + i, col + 3].Value = bodyValues[i].Value;
                    ws.Cells[dataStartRow + 1 + i, col + 3].Style.Numberformat.Format = "0.00";
                }

                int tcCount = axis.SortedValues.Count;
                int bodyCount = bodyValues.Count;
                int dataRows = Math.Max(tcCount, bodyCount);

                // Status row
                int statusRow = dataStartRow + 1 + dataRows;
                ws.Cells[statusRow, col].Value = axis.Threshold > 0
                    ? $"Threshold: {axis.Threshold:F0}°"
                    : "No threshold";
                ws.Cells[statusRow, col + 1].Value = axis.Threshold > 0
                    ? (axis.IsValid ? $"OK ({axis.MaxGap:F1}°)" : $"NOK ({axis.MaxGap:F1}°)")
                    : $"Max gap: {axis.MaxGap:F1}°";
                ws.Cells[statusRow, col].Style.Font.Bold = true;
                ws.Cells[statusRow, col + 1].Style.Font.Bold = true;

                Color statusColor = axis.Threshold <= 0 ? Color.LightGray
                    : axis.IsValid ? Color.FromArgb(198, 239, 206)
                    : Color.FromArgb(255, 199, 206);

                using (var sr = ws.Cells[statusRow, col, statusRow, col + 1])
                {
                    sr.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    sr.Style.Fill.BackgroundColor.SetColor(statusColor);
                }

                // Auto-fit data columns
                for (int c = col; c <= col + 3; c++)
                    ws.Column(c).AutoFit();

                // ── Chart ────────────────────────────────────────────
                var chart = ws.Drawings.AddChart(
                    $"Chart_{axis.AxisName}",
                    OfficeOpenXml.Drawing.Chart.eChartType.Line);

                chart.Title.Text = $"{axis.AxisName} — TC vs Body";
                chart.Title.Font.Bold = true;

                // TC series
                var tcSeries = chart.Series.Add(
                    ws.Cells[dataStartRow + 1, col + 1, dataStartRow + tcCount, col + 1],
                    ws.Cells[dataStartRow + 1, col + 1, dataStartRow + tcCount, col + 1]);
                tcSeries.Header = "TC";

                // Body series
                var bodySeries = chart.Series.Add(
                    ws.Cells[dataStartRow + 1, col + 3, dataStartRow + bodyCount, col + 3],
                    ws.Cells[dataStartRow + 1, col + 3, dataStartRow + bodyCount, col + 3]);
                bodySeries.Header = "Body";

                // Chart position (right of data, stacked vertically)
                chart.SetPosition(startRow - 1, 0, 5, 0);
                chart.SetSize(600, 300);

                startRow += dataRows + 6;
            }
        }

        /// <summary>
        /// Creates the Nearest TC worksheet.
        /// </summary>
        private static void CreateNearestTcSheet(ExcelPackage package, TempCompExportData data)
        {
            var ws = package.Workbook.Worksheets.Add("Nearest TC");

            int row = 1;

            // Title
            ws.Cells[row, 1].Value = "Nearest Temp Comp Points";
            ws.Cells[row, 1].Style.Font.Bold = true;
            ws.Cells[row, 1].Style.Font.Size = 14;
            ws.Cells[row, 1, row, 9].Merge = true;
            row += 2;

            // Summary
            ws.Cells[row, 1].Value = "Total Body Points:";
            ws.Cells[row, 2].Value = data.NearestTcResults.Count;
            row++;

            ws.Cells[row, 1].Value = "Max Angle Threshold:";
            ws.Cells[row, 2].Value = data.MaxAngleThreshold;
            ws.Cells[row, 3].Value = "degrees";
            row += 2;

            // Column headers
            ws.Cells[row, 1].Value = "Body Point";
            ws.Cells[row, 2].Value = "Body Path";      // ← ÚJ
            ws.Cells[row, 3].Value = "J2-3";
            ws.Cells[row, 4].Value = "J4";
            ws.Cells[row, 5].Value = "J5";
            ws.Cells[row, 6].Value = "J6";
            ws.Cells[row, 7].Value = "TC Point";
            ws.Cells[row, 8].Value = "TC Path";        // ← ÚJ
            ws.Cells[row, 9].Value = "TC J2-3";
            ws.Cells[row, 10].Value = "TC J4";
            ws.Cells[row, 11].Value = "TC J5";
            ws.Cells[row, 12].Value = "TC J6";
            ws.Cells[row, 13].Value = "Max Diff";

            using (var range = ws.Cells[row, 1, row, 13])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }
            row++;

            // Data rows
            foreach (var result in data.NearestTcResults)
            {
                var bodyPose = result.BodyPose;
                var tcPose = result.NearestTcPose;

                double bodyJ23 = data.RobotConfiguration.CalculateJ23Angle(bodyPose);
                double bodyJ4 = data.RobotConfiguration.NormalizeAngle180(bodyPose.J4);
                double bodyJ6 = data.RobotConfiguration.NormalizeAngle180(bodyPose.J6);

                ws.Cells[row, 1].Value = bodyPose.Name;
                ws.Cells[row, 2].Value = bodyPose.PathName ?? "";              // ← ÚJ

                ws.Cells[row, 3].Value = bodyJ23;
                ws.Cells[row, 3].Style.Numberformat.Format = "0.00";
                ws.Cells[row, 4].Value = bodyJ4;
                ws.Cells[row, 4].Style.Numberformat.Format = "0.00";
                ws.Cells[row, 5].Value = bodyPose.J5;
                ws.Cells[row, 5].Style.Numberformat.Format = "0.00";
                ws.Cells[row, 6].Value = bodyJ6;
                ws.Cells[row, 6].Style.Numberformat.Format = "0.00";

                if (tcPose != null)
                {
                    double tcJ23 = data.RobotConfiguration.CalculateJ23Angle(tcPose);
                    double tcJ4 = data.RobotConfiguration.NormalizeAngle180(tcPose.J4);
                    double tcJ6 = data.RobotConfiguration.NormalizeAngle180(tcPose.J6);

                    double d23 = tcJ23 - bodyJ23;
                    double d4 = data.RobotConfiguration.NormalizeAngle180(tcPose.J4 - bodyPose.J4);
                    double d5 = tcPose.J5 - bodyPose.J5;
                    double d6 = data.RobotConfiguration.NormalizeAngle180(tcPose.J6 - bodyPose.J6);
                    double maxDiff = Math.Max(Math.Max(Math.Abs(d23), Math.Abs(d4)),
                                              Math.Max(Math.Abs(d5), Math.Abs(d6)));

                    ws.Cells[row, 7].Value = tcPose.Name;
                    ws.Cells[row, 8].Value = tcPose.PathName ?? "";            // ← ÚJ

                    ws.Cells[row, 9].Value = tcJ23;
                    ws.Cells[row, 9].Style.Numberformat.Format = "0.00";
                    if (Math.Abs(d23) > data.MaxAngleThreshold)
                    {
                        ws.Cells[row, 9].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[row, 9].Style.Fill.BackgroundColor.SetColor(Color.LightCoral);
                    }

                    ws.Cells[row, 10].Value = tcJ4;
                    ws.Cells[row, 10].Style.Numberformat.Format = "0.00";
                    if (Math.Abs(d4) > data.MaxAngleThreshold)
                    {
                        ws.Cells[row, 10].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[row, 10].Style.Fill.BackgroundColor.SetColor(Color.LightCoral);
                    }

                    ws.Cells[row, 11].Value = tcPose.J5;
                    ws.Cells[row, 11].Style.Numberformat.Format = "0.00";
                    if (Math.Abs(d5) > data.MaxAngleThreshold)
                    {
                        ws.Cells[row, 11].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[row, 11].Style.Fill.BackgroundColor.SetColor(Color.LightCoral);
                    }

                    ws.Cells[row, 12].Value = tcJ6;
                    ws.Cells[row, 12].Style.Numberformat.Format = "0.00";
                    if (Math.Abs(d6) > data.MaxAngleThreshold)
                    {
                        ws.Cells[row, 12].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[row, 12].Style.Fill.BackgroundColor.SetColor(Color.LightCoral);
                    }

                    ws.Cells[row, 13].Value = maxDiff;
                    ws.Cells[row, 13].Style.Numberformat.Format = "0.00";
                    if (maxDiff > data.MaxAngleThreshold)
                    {
                        ws.Cells[row, 13].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[row, 13].Style.Fill.BackgroundColor.SetColor(Color.LightCoral);
                        ws.Cells[row, 13].Style.Font.Bold = true;
                    }
                }
                else
                {
                    ws.Cells[row, 7].Value = "N/A";
                }

                using (var range = ws.Cells[row, 1, row, 13])
                {
                    range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }
                row++;
            }

            // Auto-fit columns
            for (int col = 1; col <= 13; col++)
            {
                ws.Column(col).AutoFit();
            }
        }

        /// <summary>
        /// Creates the Raw Data worksheet with all poses.
        /// </summary>
        private static void CreateRawDataSheet(ExcelPackage package, TempCompExportData data)
        {
            var ws = package.Workbook.Worksheets.Add("Raw Data");

            int row = 1;

            // Title
            ws.Cells[row, 1].Value = "Raw Pose Data";
            ws.Cells[row, 1].Style.Font.Bold = true;
            ws.Cells[row, 1].Style.Font.Size = 14;
            ws.Cells[row, 1, row, 8].Merge = true;
            row += 2;

            // ========== Body Poses Section ==========
            ws.Cells[row, 1].Value = "Body Measurement Poses";
            ws.Cells[row, 1].Style.Font.Bold = true;
            ws.Cells[row, 1].Style.Font.Size = 12;
            ws.Cells[row, 1].Style.Font.Color.SetColor(Color.DarkBlue);
            row++;

            // Statistics
            if (data.BodyStatistics != null)
            {
                ws.Cells[row, 1].Value = "Count:";
                ws.Cells[row, 2].Value = data.BodyPoses.Count;
                row++;

                ws.Cells[row, 1].Value = "J2 Range:";
                ws.Cells[row, 2].Value = $"{data.BodyStatistics.J2_Min:F2}° to {data.BodyStatistics.J2_Max:F2}°";
                row++;

                ws.Cells[row, 1].Value = "J3 Range:";
                ws.Cells[row, 2].Value = $"{data.BodyStatistics.J3_Min:F2}° to {data.BodyStatistics.J3_Max:F2}°";
                row += 2;
            }

            // Body poses header
            ws.Cells[row, 1].Value = "Pose Name";
            ws.Cells[row, 2].Value = "Path";
            ws.Cells[row, 3].Value = "J1";
            ws.Cells[row, 4].Value = "J2";
            ws.Cells[row, 5].Value = "J3";
            ws.Cells[row, 6].Value = "J4";
            ws.Cells[row, 7].Value = "J5";
            ws.Cells[row, 8].Value = "J6";

            using (var range = ws.Cells[row, 1, row, 8])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
                range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }
            row++;

            // Body poses data
            foreach (var pose in data.BodyPoses)
            {
                ws.Cells[row, 1].Value = pose.Name;
                ws.Cells[row, 2].Value = pose.PathName;
                ws.Cells[row, 3].Value = pose.J1;
                ws.Cells[row, 3].Style.Numberformat.Format = "0.00";
                ws.Cells[row, 4].Value = pose.J2;
                ws.Cells[row, 4].Style.Numberformat.Format = "0.00";
                ws.Cells[row, 5].Value = pose.J3;
                ws.Cells[row, 5].Style.Numberformat.Format = "0.00";
                ws.Cells[row, 6].Value = pose.J4;
                ws.Cells[row, 6].Style.Numberformat.Format = "0.00";
                ws.Cells[row, 7].Value = pose.J5;
                ws.Cells[row, 7].Style.Numberformat.Format = "0.00";
                ws.Cells[row, 8].Value = pose.J6;
                ws.Cells[row, 8].Style.Numberformat.Format = "0.00";

                using (var range = ws.Cells[row, 1, row, 8])
                {
                    range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }
                row++;
            }

            row += 2;

            // ========== Temp Comp Poses Section ==========
            ws.Cells[row, 1].Value = "Temp Comp Measurement Poses";
            ws.Cells[row, 1].Style.Font.Bold = true;
            ws.Cells[row, 1].Style.Font.Size = 12;
            ws.Cells[row, 1].Style.Font.Color.SetColor(Color.DarkGreen);
            row++;

            // Statistics
            if (data.TempCompStatistics != null)
            {
                ws.Cells[row, 1].Value = "Count:";
                ws.Cells[row, 2].Value = data.TempCompPoses.Count;
                row++;

                ws.Cells[row, 1].Value = "J2 Range:";
                ws.Cells[row, 2].Value = $"{data.TempCompStatistics.J2_Min:F2}° to {data.TempCompStatistics.J2_Max:F2}°";
                row++;

                ws.Cells[row, 1].Value = "J3 Range:";
                ws.Cells[row, 2].Value = $"{data.TempCompStatistics.J3_Min:F2}° to {data.TempCompStatistics.J3_Max:F2}°";
                row += 2;
            }

            // TC poses header
            ws.Cells[row, 1].Value = "Pose Name";
            ws.Cells[row, 2].Value = "Path";
            ws.Cells[row, 3].Value = "J1";
            ws.Cells[row, 4].Value = "J2";
            ws.Cells[row, 5].Value = "J3";
            ws.Cells[row, 6].Value = "J4";
            ws.Cells[row, 7].Value = "J5";
            ws.Cells[row, 8].Value = "J6";

            using (var range = ws.Cells[row, 1, row, 8])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
                range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }
            row++;

            // TC poses data
            foreach (var pose in data.TempCompPoses)
            {
                ws.Cells[row, 1].Value = pose.Name;
                ws.Cells[row, 2].Value = pose.PathName;
                ws.Cells[row, 3].Value = pose.J1;
                ws.Cells[row, 3].Style.Numberformat.Format = "0.00";
                ws.Cells[row, 4].Value = pose.J2;
                ws.Cells[row, 4].Style.Numberformat.Format = "0.00";
                ws.Cells[row, 5].Value = pose.J3;
                ws.Cells[row, 5].Style.Numberformat.Format = "0.00";
                ws.Cells[row, 6].Value = pose.J4;
                ws.Cells[row, 6].Style.Numberformat.Format = "0.00";
                ws.Cells[row, 7].Value = pose.J5;
                ws.Cells[row, 7].Style.Numberformat.Format = "0.00";
                ws.Cells[row, 8].Value = pose.J6;
                ws.Cells[row, 8].Style.Numberformat.Format = "0.00";

                using (var range = ws.Cells[row, 1, row, 8])
                {
                    range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }
                row++;
            }

            // Auto-fit columns
            for (int col = 1; col <= 8; col++)
            {
                ws.Column(col).AutoFit();
            }
        }
    }
}
