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
                item.SubItems.Add(result.Message);
                item.SubItems.Add(""); // Placeholder for TC column if needed
                item.SubItems.Add(result.Details);
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
                    for (int k = 0; k < 5; k++) item.SubItems.Add("-");
                }

                listView.Items.Add(item);
            }

            // Auto-resize columns
            foreach (ColumnHeader col in listView.Columns)
                col.Width = -2;
        }

        /// <summary>
        /// Formats raw pose data into a ListView with envelope highlighting.
        /// </summary>
        public void FormatRawData(
            List<RobotPose> bodyPoses,
            List<RobotPose> tcPoses,
            ListView listView,
            IRobotConfiguration config,
            double threshold)
        {
            listView.Items.Clear();

            if (bodyPoses == null || tcPoses == null)
                return;

            // Calculate body J2-3 envelope
            double bodyMax23 = double.MinValue, bodyMin23 = double.MaxValue;
            foreach (var b in bodyPoses)
            {
                double a = config.CalculateJ23Angle(b);
                if (a > bodyMax23) bodyMax23 = a;
                if (a < bodyMin23) bodyMin23 = a;
            }

            // Find TC J2-3: 2 largest and 2 smallest values
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

            int maxRows = Math.Max(bodyPoses.Count, tcPoses.Count);
            for (int i = 0; i < maxRows; i++)
            {
                var body = i < bodyPoses.Count ? bodyPoses[i] : null;
                var tc = i < tcPoses.Count ? tcPoses[i] : null;

                var item = new ListViewItem(body != null ? body.Name : "");
                item.UseItemStyleForSubItems = false;

                // Body Path column
                item.SubItems.Add(body != null ? body.PathName ?? "" : "");

                // Body columns
                item.SubItems.Add(body != null ? string.Format("{0:F2}", body.J1) : "");
                item.SubItems.Add(body != null ? string.Format("{0:F2}", body.J2) : "");
                item.SubItems.Add(body != null ? string.Format("{0:F2}", body.J3) : "");
                item.SubItems.Add(body != null ? string.Format("{0:F2}", config.NormalizeAngle180(body.J4)) : "");
                item.SubItems.Add(body != null ? string.Format("{0:F2}", body.J5) : "");
                item.SubItems.Add(body != null ? string.Format("{0:F2}", config.NormalizeAngle180(body.J6)) : "");

                // Body J2-3 cell with highlighting
                if (body != null)
                {
                    double bodyA = config.CalculateJ23Angle(body);
                    var sub = item.SubItems.Add(string.Format("{0:F2}", bodyA));
                    if (bodyA == bodyMax23)
                        sub.BackColor = ColorPalette.OKLight;
                    else if (bodyA == bodyMin23)
                        sub.BackColor = ColorPalette.Info;
                }
                else item.SubItems.Add("");

                // TC columns
                item.SubItems.Add(tc != null ? tc.Name : "");

                // TC Path column
                item.SubItems.Add(tc != null ? tc.PathName ?? "" : "");
                item.SubItems.Add(tc != null ? string.Format("{0:F2}", tc.J1) : "");
                item.SubItems.Add(tc != null ? string.Format("{0:F2}", tc.J2) : "");
                item.SubItems.Add(tc != null ? string.Format("{0:F2}", tc.J3) : "");
                item.SubItems.Add(tc != null ? string.Format("{0:F2}", config.NormalizeAngle180(tc.J4)) : "");
                item.SubItems.Add(tc != null ? string.Format("{0:F2}", tc.J5) : "");
                item.SubItems.Add(tc != null ? string.Format("{0:F2}", config.NormalizeAngle180(tc.J6)) : "");

                // TC J2-3 cell with coverage highlighting
                if (tc != null)
                {
                    double tcA = config.CalculateJ23Angle(tc);
                    var sub = item.SubItems.Add(string.Format("{0:F2}", tcA));

                    if (tcA >= tcMax2) // Top 2 largest
                    {
                        if (tcA >= bodyMax23)
                            sub.BackColor = ColorPalette.OKLight;
                        else if (bodyMax23 - tcA < threshold)
                            sub.BackColor = ColorPalette.WarningLight;
                        else
                            sub.BackColor = ColorPalette.NOKLight;
                    }
                    else if (tcA <= tcMin2) // Bottom 2 smallest
                    {
                        if (tcA <= bodyMin23)
                            sub.BackColor = ColorPalette.Info;
                        else if (tcA - bodyMin23 < threshold)
                            sub.BackColor = ColorPalette.WarningLight;
                        else
                            sub.BackColor = ColorPalette.NOKLight;
                    }
                }
                else item.SubItems.Add("");

                listView.Items.Add(item);
            }

            // Auto-resize columns
            foreach (ColumnHeader col in listView.Columns)
                col.Width = -2;
        }
    }
}
