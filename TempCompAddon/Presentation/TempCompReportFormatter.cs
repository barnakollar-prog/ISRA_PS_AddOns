using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ISRA.Core.Domain;
using ISRA.Core.Utilities;
using ISRA.Calculations.TempComp.Domain;
using ISRA.Calculations.TempComp.Domain.Results;
using ISRA.Calculations.TempComp.RobotConfiguration;

namespace TempCompAddon.Presentation
{
    /// <summary>
    /// Formats TempComp analysis results for display in ListViews.
    /// </summary>
    public class TempCompReportFormatter
    {
        /// <summary>
        /// Formats validation results into a ListView.
        /// </summary>
        public void FormatValidationResults(AnalysisReport report, ListView listView)
        {
            listView.Items.Clear();

            if (report == null || report.ValidationResults == null)
                return;

            foreach (var result in report.ValidationResults)
            {
                var item = new ListViewItem(result.CriterionName);
                item.SubItems.Add(result.BodypartValue ?? "");
                item.SubItems.Add(result.TempCompValue ?? "");
                item.SubItems.Add(result.Message ?? "");
                item.SubItems.Add(result.IsValid ? "OK" : "NOK");

                item.BackColor = result.IsValid ? ColorPalette.OKLight : ColorPalette.NOKLight;

                listView.Items.Add(item);
            }

            // Auto-resize columns
            foreach (ColumnHeader col in listView.Columns)
                col.Width = -2;
        }

        /// <summary>
        /// Formats nearest TC point results into a ListView.
        /// </summary>
        public void FormatNearestTcResults(
    List<NearestTcResult> results,
    ListView listView,
    double threshold,
    IRobotConfiguration config)
        {
            listView.Items.Clear();

            if (results == null)
                return;

            foreach (var r in results)
            {
                var b = r.BodyPose;
                var t = r.NearestTcPose;

                var item = new ListViewItem(b.Name);
                item.UseItemStyleForSubItems = false;

                // Body pose columns
                item.SubItems.Add(b.PathName ?? "");                                          // ← ÚJ
                item.SubItems.Add(string.Format("{0:F2}", config.CalculateJ23Angle(b)));
                item.SubItems.Add(string.Format("{0:F2}", config.NormalizeAngle180(b.J4)));
                item.SubItems.Add(string.Format("{0:F2}", b.J5));
                item.SubItems.Add(string.Format("{0:F2}", config.NormalizeAngle180(b.J6)));

                if (t != null)
                {
                    double d23 = config.CalculateJ23Angle(t) - config.CalculateJ23Angle(b);
                    double d4 = config.NormalizeAngle180(t.J4 - b.J4);
                    double d5 = t.J5 - b.J5;
                    double d6 = config.NormalizeAngle180(t.J6 - b.J6);

                    item.SubItems.Add(t.Name);
                    item.SubItems.Add(t.PathName ?? "");                                      // ← ÚJ

                    var s23 = item.SubItems.Add(string.Format("{0:F2}", config.CalculateJ23Angle(t)));
                    s23.BackColor = ColorPalette.GetDifferenceColor(d23, threshold);

                    var s4 = item.SubItems.Add(string.Format("{0:F2}", config.NormalizeAngle180(t.J4)));
                    s4.BackColor = ColorPalette.GetDifferenceColor(d4, threshold);

                    var s5 = item.SubItems.Add(string.Format("{0:F2}", t.J5));
                    s5.BackColor = ColorPalette.GetDifferenceColor(d5, threshold);

                    var s6 = item.SubItems.Add(string.Format("{0:F2}", config.NormalizeAngle180(t.J6)));
                    s6.BackColor = ColorPalette.GetDifferenceColor(d6, threshold);

                    double maxDiff = Math.Max(Math.Max(Math.Abs(d23), Math.Abs(d4)),
                                              Math.Max(Math.Abs(d5), Math.Abs(d6)));

                    var sd = item.SubItems.Add(string.Format("{0:F2}", maxDiff));
                    sd.BackColor = ColorPalette.GetDifferenceColor(maxDiff, threshold);
                }
                else
                {
                    item.SubItems.Add("N/A");
                    item.SubItems.Add("");                                                     // ← ÚJ placeholder
                    for (int k = 0; k < 5; k++) item.SubItems.Add("-");
                }

                listView.Items.Add(item);
            }

            // Auto-resize columns
            foreach (ColumnHeader col in listView.Columns)
                col.Width = -2;
        }
        /// <summary>
        /// Formats gap analysis results into a DataGridView.
        /// </summary>
        public void FormatGapAnalysis(GapAnalysisResult result, DataGridView grid)
        {
            grid.Rows.Clear();
            grid.Columns.Clear();

            if (result == null) return;

            var axes = result.AllAxes;

            // Add columns — Point then Value for each axis
            foreach (var axis in axes)
            {
                var colPoint = new DataGridViewTextBoxColumn();
                colPoint.HeaderText = axis.AxisName + " Point";
                colPoint.Width = 130;
                colPoint.SortMode = DataGridViewColumnSortMode.NotSortable;
                grid.Columns.Add(colPoint);

                var colVal = new DataGridViewTextBoxColumn();
                colVal.HeaderText = axis.AxisName;
                colVal.Width = 70;
                colVal.SortMode = DataGridViewColumnSortMode.NotSortable;
                grid.Columns.Add(colVal);
            }

            // Calculate rows per axis (values + gap cells)
            int maxRows = 0;
            var rowsPerAxis = new List<List<(string Label, double? Value, bool IsGap)>>();

            foreach (var axis in axes)
            {
                var rows = new List<(string Label, double? Value, bool IsGap)>();
                for (int i = 0; i < axis.SortedValues.Count; i++)
                {
                    var entry = axis.SortedValues[i];
                    string label = $"{entry.PathName} - {entry.PoseName}";
                    rows.Add((label, entry.Value, false));

                    if (axis.GapIndices.Contains(i))
                        rows.Add(("", null, true)); // gap cell
                }
                rowsPerAxis.Add(rows);
                if (rows.Count > maxRows) maxRows = rows.Count;
            }

            // Add data rows
            for (int r = 0; r < maxRows; r++)
                grid.Rows.Add();

            for (int axisIdx = 0; axisIdx < axes.Count; axisIdx++)
            {
                var rows = rowsPerAxis[axisIdx];
                int pointCol = axisIdx * 2;
                int valCol = axisIdx * 2 + 1;

                for (int r = 0; r < rows.Count; r++)
                {
                    var entry = rows[r];
                    var pointCell = grid.Rows[r].Cells[pointCol];
                    var valCell = grid.Rows[r].Cells[valCol];

                    if (entry.IsGap)
                    {
                        pointCell.Value = "";
                        valCell.Value = "";
                        pointCell.Style.BackColor = ColorPalette.NOKLight;
                        valCell.Style.BackColor = ColorPalette.NOKLight;
                    }
                    else
                    {
                        pointCell.Value = entry.Label;
                        valCell.Value = string.Format("{0:F2}", entry.Value);
                    }
                }
            }

            // Status row at bottom
            grid.Rows.Add();
            int statusRow = grid.Rows.Count - 1;
            grid.Rows[statusRow].DefaultCellStyle.Font =
                new Font(grid.Font, FontStyle.Bold);

            for (int axisIdx = 0; axisIdx < axes.Count; axisIdx++)
            {
                var axis = axes[axisIdx];
                int pointCol = axisIdx * 2;
                int valCol = axisIdx * 2 + 1;

                // Point col: Max gap label
                var pointCell = grid.Rows[statusRow].Cells[pointCol];
                pointCell.Value = axis.Threshold > 0
                    ? $"Threshold: {axis.Threshold:F0}°"
                    : "No threshold";
                pointCell.Style.BackColor = Color.LightGray;

                // Val col: OK/NOK
                var valCell = grid.Rows[statusRow].Cells[valCol];
                if (axis.Threshold <= 0)
                {
                    valCell.Value = $"{axis.MaxGap:F1}°";
                    valCell.Style.BackColor = Color.LightGray;
                }
                else
                {
                    valCell.Value = axis.IsValid
                        ? $"OK ({axis.MaxGap:F1}°)"
                        : $"NOK ({axis.MaxGap:F1}°)";
                    valCell.Style.BackColor = axis.IsValid
                        ? ColorPalette.OKLight
                        : ColorPalette.NOKLight;
                }
            }
        }
        /// <summary>
        /// Formats raw pose data into a ListView with envelope highlighting.
        /// </summary>
        public void FormatRawData(
            List<RobotPose> bodyPoses,
            List<RobotPose> tcPoses,
            ListView bodyListView,
            ListView tcListView,
            IRobotConfiguration config,
            double threshold)
        {
            bodyListView.Items.Clear();
            tcListView.Items.Clear();

            if (bodyPoses == null || tcPoses == null)
                return;

            double bodyMax23 = double.MinValue, bodyMin23 = double.MaxValue;
            foreach (var b in bodyPoses)
            {
                double a = config.CalculateJ23Angle(b);
                if (a > bodyMax23) bodyMax23 = a;
                if (a < bodyMin23) bodyMin23 = a;
            }

            double tcMax1 = double.MinValue, tcMax2 = double.MinValue;
            double tcMin1 = double.MaxValue, tcMin2 = double.MaxValue;
            foreach (var t in tcPoses)
            {
                double a = config.CalculateJ23Angle(t);
                if (a > tcMax1) { tcMax2 = tcMax1; tcMax1 = a; }
                else if (a > tcMax2) tcMax2 = a;
                if (a < tcMin1) { tcMin2 = tcMin1; tcMin1 = a; }
                else if (a < tcMin2) tcMin2 = a;
            }

            foreach (var body in bodyPoses)
            {
                var item = new ListViewItem(body.Name ?? "");
                item.UseItemStyleForSubItems = false;
                item.SubItems.Add(body.PathName ?? "");
                item.SubItems.Add(string.Format("{0:F2}", body.J1));
                item.SubItems.Add(string.Format("{0:F2}", body.J2));
                item.SubItems.Add(string.Format("{0:F2}", body.J3));
                item.SubItems.Add(string.Format("{0:F2}", body.J4));
                item.SubItems.Add(string.Format("{0:F2}", body.J5));
                item.SubItems.Add(string.Format("{0:F2}", body.J6));

                double bodyA = config.CalculateJ23Angle(body);
                var sub = item.SubItems.Add(string.Format("{0:F2}", bodyA));
                if (bodyA == bodyMax23)
                    sub.BackColor = ColorPalette.OKLight;
                else if (bodyA == bodyMin23)
                    sub.BackColor = ColorPalette.Info;

                bodyListView.Items.Add(item);
            }

            foreach (var tc in tcPoses)
            {
                var item = new ListViewItem(tc.Name ?? "");
                item.UseItemStyleForSubItems = false;
                item.SubItems.Add(tc.PathName ?? "");
                item.SubItems.Add(string.Format("{0:F2}", tc.J1));
                item.SubItems.Add(string.Format("{0:F2}", tc.J2));
                item.SubItems.Add(string.Format("{0:F2}", tc.J3));
                item.SubItems.Add(string.Format("{0:F2}", tc.J4));
                item.SubItems.Add(string.Format("{0:F2}", tc.J5));
                item.SubItems.Add(string.Format("{0:F2}", tc.J6));

                double tcA = config.CalculateJ23Angle(tc);
                var sub = item.SubItems.Add(string.Format("{0:F2}", tcA));

                if (tcA >= tcMax2)
                {
                    if (tcA >= bodyMax23)
                        sub.BackColor = ColorPalette.OKLight;
                    else if (bodyMax23 - tcA < threshold)
                        sub.BackColor = ColorPalette.WarningLight;
                    else
                        sub.BackColor = ColorPalette.NOKLight;
                }
                else if (tcA <= tcMin2)
                {
                    if (tcA <= bodyMin23)
                        sub.BackColor = ColorPalette.Info;
                    else if (tcA - bodyMin23 < threshold)
                        sub.BackColor = ColorPalette.WarningLight;
                    else
                        sub.BackColor = ColorPalette.NOKLight;
                }

                tcListView.Items.Add(item);
            }

            foreach (ColumnHeader col in bodyListView.Columns)
                col.Width = -2;

            foreach (ColumnHeader col in tcListView.Columns)
                col.Width = -2;
        }
    }
}
