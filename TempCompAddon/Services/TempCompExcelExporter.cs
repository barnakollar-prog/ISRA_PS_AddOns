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
            ws.Cells[row, 1].Value = "Validator";
            ws.Cells[row, 2].Value = "Status";
            ws.Cells[row, 3].Value = "Message";
            ws.Cells[row, 4].Value = "Details";

            using (var range = ws.Cells[row, 1, row, 4])
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
                ws.Cells[row, 2].Value = result.IsValid ? "PASS" : "FAIL";
                ws.Cells[row, 2].Style.Font.Bold = true;
                ws.Cells[row, 2].Style.Font.Color.SetColor(result.IsValid ? Color.Green : Color.Red);
                ws.Cells[row, 3].Value = result.Message;
                ws.Cells[row, 4].Value = result.Details;

                using (var range = ws.Cells[row, 1, row, 4])
                {
                    range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }
                row++;
            }

            // Auto-fit columns
            ws.Column(1).AutoFit();
            ws.Column(2).AutoFit();
            ws.Column(3).AutoFit();
            ws.Column(4).AutoFit();
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
            ws.Cells[row, 2].Value = "J2-3";
            ws.Cells[row, 3].Value = "J4";
            ws.Cells[row, 4].Value = "J5";
            ws.Cells[row, 5].Value = "J6";
            ws.Cells[row, 6].Value = "TC Point";
            ws.Cells[row, 7].Value = "TC J2-3";
            ws.Cells[row, 8].Value = "TC J4";
            ws.Cells[row, 9].Value = "TC J5";
            ws.Cells[row, 10].Value = "TC J6";
            ws.Cells[row, 11].Value = "Max Diff";

            using (var range = ws.Cells[row, 1, row, 11])
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

                // Calculate J2-3 angles
                double bodyJ23 = data.RobotConfiguration.CalculateJ23Angle(bodyPose);
                double tcJ23 = data.RobotConfiguration.CalculateJ23Angle(tcPose);

                // Normalize angles
                double bodyJ4 = data.RobotConfiguration.NormalizeAngle180(bodyPose.J4);
                double bodyJ6 = data.RobotConfiguration.NormalizeAngle180(bodyPose.J6);
                double tcJ4 = data.RobotConfiguration.NormalizeAngle180(tcPose.J4);
                double tcJ6 = data.RobotConfiguration.NormalizeAngle180(tcPose.J6);

                // Calculate differences
                double d23 = tcJ23 - bodyJ23;
                double d4 = data.RobotConfiguration.NormalizeAngle180(tcPose.J4 - bodyPose.J4);
                double d5 = tcPose.J5 - bodyPose.J5;
                double d6 = data.RobotConfiguration.NormalizeAngle180(tcPose.J6 - bodyPose.J6);
                double maxDiff = Math.Max(Math.Max(Math.Abs(d23), Math.Abs(d4)),
                                          Math.Max(Math.Abs(d5), Math.Abs(d6)));

                // Body Point
                ws.Cells[row, 1].Value = bodyPose.Name;

                // Body J2-3
                ws.Cells[row, 2].Value = bodyJ23;
                ws.Cells[row, 2].Style.Numberformat.Format = "0.00";

                // Body J4
                ws.Cells[row, 3].Value = bodyJ4;
                ws.Cells[row, 3].Style.Numberformat.Format = "0.00";

                // Body J5
                ws.Cells[row, 4].Value = bodyPose.J5;
                ws.Cells[row, 4].Style.Numberformat.Format = "0.00";

                // Body J6
                ws.Cells[row, 5].Value = bodyJ6;
                ws.Cells[row, 5].Style.Numberformat.Format = "0.00";

                // TC Point
                ws.Cells[row, 6].Value = tcPose.Name;

                // TC J2-3
                ws.Cells[row, 7].Value = tcJ23;
                ws.Cells[row, 7].Style.Numberformat.Format = "0.00";
                if (Math.Abs(d23) > data.MaxAngleThreshold)
                {
                    ws.Cells[row, 7].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[row, 7].Style.Fill.BackgroundColor.SetColor(Color.LightCoral);
                }

                // TC J4
                ws.Cells[row, 8].Value = tcJ4;
                ws.Cells[row, 8].Style.Numberformat.Format = "0.00";
                if (Math.Abs(d4) > data.MaxAngleThreshold)
                {
                    ws.Cells[row, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[row, 8].Style.Fill.BackgroundColor.SetColor(Color.LightCoral);
                }

                // TC J5
                ws.Cells[row, 9].Value = tcPose.J5;
                ws.Cells[row, 9].Style.Numberformat.Format = "0.00";
                if (Math.Abs(d5) > data.MaxAngleThreshold)
                {
                    ws.Cells[row, 9].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[row, 9].Style.Fill.BackgroundColor.SetColor(Color.LightCoral);
                }

                // TC J6
                ws.Cells[row, 10].Value = tcJ6;
                ws.Cells[row, 10].Style.Numberformat.Format = "0.00";
                if (Math.Abs(d6) > data.MaxAngleThreshold)
                {
                    ws.Cells[row, 10].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[row, 10].Style.Fill.BackgroundColor.SetColor(Color.LightCoral);
                }

                // Max Diff
                ws.Cells[row, 11].Value = maxDiff;
                ws.Cells[row, 11].Style.Numberformat.Format = "0.00";
                if (maxDiff > data.MaxAngleThreshold)
                {
                    ws.Cells[row, 11].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[row, 11].Style.Fill.BackgroundColor.SetColor(Color.LightCoral);
                    ws.Cells[row, 11].Style.Font.Bold = true;
                }

                using (var range = ws.Cells[row, 1, row, 11])
                {
                    range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }

                row++;
            }

            // Auto-fit columns
            for (int col = 1; col <= 11; col++)
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
