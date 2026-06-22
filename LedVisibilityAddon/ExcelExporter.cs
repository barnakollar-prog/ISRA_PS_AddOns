using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using ISRA.Components.AccuSite.Trackers;
using ISRA.Components.AccuSite.Stars;
using ISRA.Calculations.AccuSite;
using LedVisibilityAddon.Presentation;

namespace LedVisibilityAddon
{
    public static class ExcelExporter
    {
        public static void Export(
            List<ExportRow> rows,
            string trackerName,
            GeometryCalculations.TriangleResult triangle = null)
        {
            // Get PS study name
            string studyName = "Unknown_Study";
            string path = Tecnomatix.Engineering.TxApplication.ActiveDocument.FinalDestination;
            if (!string.IsNullOrEmpty(path))
                studyName = Path.GetFileNameWithoutExtension(path);

            // Save dialog
            var dlg = new SaveFileDialog
            {
                Title = "Export to Excel",
                Filter = "Excel files (*.xlsx)|*.xlsx",
                FileName = string.Format("LED_Visibility_{0}_{1}",
                    studyName,
                    DateTime.Now.ToString("yyyyMMdd"))
            };

            if (dlg.ShowDialog() != DialogResult.OK) return;

            try
            {
                using (var pck = new ExcelPackage())
                {
                    var ws = pck.Workbook.Worksheets.Add("LED Visibility");

                    // ── Title ─────────────────────────────────────
                    ws.Cells[1, 1].Value = "LED Visibility Analysis Report";
                    ws.Cells[1, 1].Style.Font.Bold = true;
                    ws.Cells[1, 1].Style.Font.Size = 14;
                    ws.Cells[1, 1, 1, 7].Merge = true;

                    ws.Cells[2, 1].Value = "Tracker:";
                    ws.Cells[2, 1].Style.Font.Bold = true;
                    ws.Cells[2, 2].Value = trackerName;

                    ws.Cells[3, 1].Value = "Date:";
                    ws.Cells[3, 1].Style.Font.Bold = true;
                    ws.Cells[3, 2].Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    ws.Cells[4, 1].Value = "Project:";
                    ws.Cells[4, 1].Style.Font.Bold = true;
                    ws.Cells[4, 2].Value = studyName;

                    // ── Star results header ───────────────────────
                    int headerRow = 6;
                    var headers = new[]
                    {
                        "Star Name", "Local X (mm)", "Local Y (mm)",
                        "Local Z (mm)", "Zone", "Status", "Tracker"
                    };

                    for (int i = 0; i < headers.Length; i++)
                    {
                        var cell = ws.Cells[headerRow, i + 1];
                        cell.Value = headers[i];
                        cell.Style.Font.Bold = true;
                        cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(180, 0, 0));
                        cell.Style.Font.Color.SetColor(Color.White);
                        cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    }

                    // ── Star data rows ────────────────────────────
                    int inCount = 0, outCount = 0;
                    for (int i = 0; i < rows.Count; i++)
                    {
                        var row = rows[i];
                        int r = headerRow + 1 + i;

                        ws.Cells[r, 1].Value = row.StarName;
                        ws.Cells[r, 2].Value = Math.Round(row.LocalX, 1);
                        ws.Cells[r, 3].Value = Math.Round(row.LocalY, 1);
                        ws.Cells[r, 4].Value = Math.Round(row.LocalZ, 1);
                        ws.Cells[r, 5].Value = row.Zone;
                        ws.Cells[r, 6].Value = row.InsideFov ? "IN FOV" : "OUT";
                        ws.Cells[r, 7].Value = row.TrackerName;

                        var rowRange = ws.Cells[r, 1, r, 7];
                        rowRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        rowRange.Style.Fill.BackgroundColor.SetColor(
                            row.InsideFov
                                ? Color.FromArgb(198, 239, 206)
                                : Color.FromArgb(255, 199, 206));
                        rowRange.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        rowRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        rowRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        rowRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        rowRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

                        if (row.InsideFov) inCount++;
                        else outCount++;
                    }

                    int nextRow = headerRow + rows.Count + 2;

                    // ── Triangle section ──────────────────────────
                    if (triangle != null)
                    {
                        // Section header
                        ws.Cells[nextRow, 1].Value = "Triangle Analysis";
                        ws.Cells[nextRow, 1].Style.Font.Bold = true;
                        ws.Cells[nextRow, 1].Style.Font.Size = 12;
                        ws.Cells[nextRow, 1, nextRow, 7].Merge = true;
                        ws.Cells[nextRow, 1, nextRow, 7].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[nextRow, 1, nextRow, 7].Style.Fill.BackgroundColor.SetColor(
                            Color.FromArgb(68, 84, 106));
                        ws.Cells[nextRow, 1].Style.Font.Color.SetColor(Color.White);
                        nextRow++;

                        // Triangle headers
                        var triHeaders = new[] { "Measurement", "Value (mm)", "Note" };
                        for (int i = 0; i < triHeaders.Length; i++)
                        {
                            var cell = ws.Cells[nextRow, i + 1];
                            cell.Value = triHeaders[i];
                            cell.Style.Font.Bold = true;
                            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(180, 0, 0));
                            cell.Style.Font.Color.SetColor(Color.White);
                            cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        }
                        nextRow++;

                        // Side rows
                        var triRows = new[]
                        {
                            new { Label = "Side Star1-Star2", Value = triangle.SideAB, Note = "" },
                            new { Label = "Side Star2-Star3", Value = triangle.SideBC, Note = "" },
                            new { Label = "Side Star3-Star1", Value = triangle.SideCA, Note = "" },
                            new { Label = "Longest Side",     Value = triangle.LongestSide, Note = triangle.LongestSideName },
                        };

                        foreach (var tr in triRows)
                        {
                            ws.Cells[nextRow, 1].Value = tr.Label;
                            ws.Cells[nextRow, 2].Value = Math.Round(tr.Value, 1);
                            ws.Cells[nextRow, 3].Value = tr.Note;
                            var rng = ws.Cells[nextRow, 1, nextRow, 3];
                            rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            rng.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(242, 242, 242));
                            rng.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                            nextRow++;
                        }

                        // Height row – green or red
                        ws.Cells[nextRow, 1].Value = "▶  Triangle Height";
                        ws.Cells[nextRow, 1].Style.Font.Bold = true;
                        ws.Cells[nextRow, 2].Value = Math.Round(triangle.Height, 1);
                        ws.Cells[nextRow, 2].Style.Font.Bold = true;
                        ws.Cells[nextRow, 3].Value = triangle.IsValid ? "✔  OK (>= 500 mm)" : "✘  FAIL (< 500 mm)";
                        ws.Cells[nextRow, 3].Style.Font.Bold = true;

                        var heightRange = ws.Cells[nextRow, 1, nextRow, 3];
                        heightRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        heightRange.Style.Fill.BackgroundColor.SetColor(
                            triangle.IsValid
                                ? Color.FromArgb(0, 200, 0)
                                : Color.FromArgb(255, 80, 80));
                        heightRange.Style.Font.Color.SetColor(Color.White);
                        heightRange.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        nextRow += 2;
                    }

                    // ── Summary ───────────────────────────────────
                    ws.Cells[nextRow, 1].Value = "Summary";
                    ws.Cells[nextRow, 1].Style.Font.Bold = true;
                    ws.Cells[nextRow, 1].Style.Font.Size = 12;
                    nextRow++;

                    ws.Cells[nextRow, 1].Value = "Total Stars:";
                    ws.Cells[nextRow, 1].Style.Font.Bold = true;
                    ws.Cells[nextRow, 2].Value = rows.Count;
                    nextRow++;

                    ws.Cells[nextRow, 1].Value = "IN FOV:";
                    ws.Cells[nextRow, 1].Style.Font.Bold = true;
                    ws.Cells[nextRow, 2].Value = inCount;
                    ws.Cells[nextRow, 2].Style.Font.Color.SetColor(Color.Green);
                    nextRow++;

                    ws.Cells[nextRow, 1].Value = "OUT of FOV:";
                    ws.Cells[nextRow, 1].Style.Font.Bold = true;
                    ws.Cells[nextRow, 2].Value = outCount;
                    ws.Cells[nextRow, 2].Style.Font.Color.SetColor(Color.Red);

                    // ── Column widths ─────────────────────────────
                    ws.Column(1).Width = 25;
                    ws.Column(2).Width = 14;
                    ws.Column(3).Width = 22;
                    ws.Column(4).Width = 14;
                    ws.Column(5).Width = 18;
                    ws.Column(6).Width = 12;
                    ws.Column(7).Width = 25;

                    pck.SaveAs(new FileInfo(dlg.FileName));
                }

                MessageBox.Show("Export successful!\n" + dlg.FileName,
                    "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);

                System.Diagnostics.Process.Start(dlg.FileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Export error: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}