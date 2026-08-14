using ISRA.Core.Utilities;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;

namespace ISRA.Calculations.AccuSite
{
    /// <summary>
    /// Result of MP Excel import operation.
    /// </summary>
    public class MpImportResult
    {
        public bool Success { get; set; }
        public int LevelCount { get; set; }
        public int SkippedCount { get; set; }
        public string OutputPath { get; set; }
        public string ErrorMessage { get; set; }
        public List<string> Warnings { get; set; } = new List<string>();
    }

    /// <summary>
    /// Imports MP feature data from the project Excel file (MP list sheet).
    /// Reads Sensor Level (column C) and Algorithm (column I).
    /// If a level has multiple algorithms, the strictest aiming profile wins.
    /// Writes output as JSON for runtime use by MpFeatureLookup.
    ///
    /// Excel structure (1-based columns):
    ///   Col A (0): Nr. / sorszám
    ///   Col B (1): MP name
    ///   Col C (2): Sensor Level = ID used in PS location name (mp_<Level>_<MPname>)
    ///   Col I (8): Algorithm (e.g. "Scanned Edge", "Scanned Patch")
    ///   Data starts at row 8 (0-based row index 7).
    /// </summary>
    public static class MpFeatureExcelImporter
    {
        private const int ColLevel = 2; // C (0-based)
        private const int ColAlgorithm = 8; // I (0-based)
        private const int DataStartRow = 7; // row index 7 = Excel row 8 (0-based)
        private const string SheetName = "MP list";

        /// <summary>
        /// Imports from Excel and writes JSON to outputJsonPath.
        /// </summary>
        public static MpImportResult Import(string excelPath, string outputJsonPath)
        {
            var result = new MpImportResult();

            if (!File.Exists(excelPath))
            {
                result.Success = false;
                result.ErrorMessage = string.Format("Excel file not found: {0}", excelPath);
                return result;
            }

            try
            {
              

                // Collect: level → strictest profile
                var levelProfiles = new Dictionary<int, AimingGuidelinesProfile>();
                int skipped = 0;

                using (var package = new ExcelPackage(new FileInfo(excelPath)))
                {
                    ExcelWorksheet ws = package.Workbook.Worksheets[SheetName];
                    if (ws == null)
                    {
                        // Fallback: use first sheet
                        ws = package.Workbook.Worksheets[0];
                        result.Warnings.Add(string.Format(
                            "Sheet '{0}' not found — using first sheet '{1}'.",
                            SheetName, ws.Name));
                    }

                    int lastRow = ws.Dimension?.End.Row ?? DataStartRow;

                    for (int r = DataStartRow; r <= lastRow; r++)
                    {
                        // Read Sensor Level (C)
                        var levelCell = ws.Cells[r + 1, ColLevel + 1].Value; // EPPlus is 1-based
                        if (levelCell == null) { skipped++; continue; }

                        int level;
                        if (!TryParseLevel(levelCell, out level)) { skipped++; continue; }

                        // Read Algorithm (I)
                        var algCell = ws.Cells[r + 1, ColAlgorithm + 1].Value;
                        if (algCell == null) { skipped++; continue; }

                        string algorithm = algCell.ToString().Trim();
                        if (string.IsNullOrEmpty(algorithm)) { skipped++; continue; }

                        // Look up aiming profile
                        var profile = AimingGuidelinesCatalog.GetByAlgorithm(algorithm);
                        if (profile == null)
                        {
                            result.Warnings.Add(string.Format(
                                "Row {0}: unknown algorithm '{1}' — skipped.", r + 1, algorithm));
                            skipped++;
                            continue;
                        }

                        // Merge if level already seen (strictest wins)
                        if (!levelProfiles.ContainsKey(level))
                            levelProfiles[level] = profile;
                        else
                            levelProfiles[level] = levelProfiles[level].MergeStrictest(profile);
                    }
                }

                // Write JSON
                string json = BuildJson(levelProfiles);
                File.WriteAllText(outputJsonPath, json, Encoding.UTF8);

                result.Success = true;
                result.LevelCount = levelProfiles.Count;
                result.SkippedCount = skipped;
                result.OutputPath = outputJsonPath;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
            }

            return result;
        }

        // ── Helpers ───────────────────────────────────────────────

        private static bool TryParseLevel(object cellValue, out int level)
        {
            level = -1;
            if (cellValue == null) return false;

            // EPPlus returns doubles for numeric cells
            if (cellValue is double d)
            {
                level = (int)d;
                return level > 0;
            }

            return int.TryParse(cellValue.ToString(), out level) && level > 0;
        }

        /// <summary>
        /// Builds a compact JSON array from the level→profile dictionary.
        /// Format: [{"level":1,"algorithm":"Scanned Edge"}, ...]
        /// </summary>
        private static string BuildJson(Dictionary<int, AimingGuidelinesProfile> levelProfiles)
        {
            var sb = new StringBuilder();
            sb.AppendLine("[");

            int i = 0;
            foreach (var kvp in levelProfiles)
            {
                sb.Append("  {");
                sb.AppendFormat("\"level\":{0},\"algorithm\":\"{1}\"",
                    kvp.Key, kvp.Value.AlgorithmName.Replace("\"", "\\\""));
                sb.Append("}");
                if (i < levelProfiles.Count - 1) sb.Append(",");
                sb.AppendLine();
                i++;
            }

            sb.Append("]");
            return sb.ToString();
        }
    }
}