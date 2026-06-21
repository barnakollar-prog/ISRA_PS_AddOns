using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ISRA.Components.AccuSite.Trackers;
using ISRA.Components.AccuSite.Stars;
using ISRA.Calculations.AccuSite;
using LedVisibilityAddon.Presentation;

namespace LedVisibilityAddon.Presentation
{
    /// <summary>
    /// Formats LED Visibility analysis results for display in ListView.
    /// Handles collapsible rows with expand/collapse logic.
    /// </summary>
    public class LedVisibilityReportFormatter
    {
        // Tag constants for collapsible rows
        private const string TAG_EMITTER_DETAIL = "emitter_detail";
        private const string TAG_EMITTER_SUMMARY = "emitter_summary";
        private const string TAG_TRIANGLE_DETAIL = "triangle_detail";
        private const string TAG_TRIANGLE_SUMMARY = "triangle_summary";

        /// <summary>
        /// Format all star results into ListView items.
        /// </summary>
        public void FormatStarResults(List<StarAnalysisResult> results, ListView listView)
        {
            if (results == null || listView == null) return;

            var emitters = Star515_0139.GetEmitters();
            var cameras = Tracker920_0005.GetCameras();

            foreach (var result in results)
            {
                string shortName = result.StarName.Length > 20
                    ? "..." + result.StarName.Substring(result.StarName.Length - 17)
                    : result.StarName;

                // ── Star header row ───────────────────────────────
                var headerItem = new ListViewItem(shortName);
                headerItem.SubItems.Add(string.Format("{0:F0}", result.LocalPosition.X));
                headerItem.SubItems.Add(string.Format("{0:F0}", result.LocalPosition.Y));
                headerItem.SubItems.Add(string.Format("{0:F0}", result.LocalPosition.Z));
                headerItem.SubItems.Add(result.ZoneName);
                headerItem.SubItems.Add(result.ZoneLabel);
                headerItem.BackColor = result.Zone == PositionZone.NOK
                    ? Color.LightCoral
                    : result.Zone == PositionZone.Warning
                        ? Color.LightYellow
                        : Color.LightGreen;
                listView.Items.Add(headerItem);

                // ── Line of Sight summary row (collapsible) ───────
                FormatLineOfSightSummary(result, listView);

                // ── Emitter summary row (collapsible) ─────────────
                FormatEmitterSummary(result, emitters, cameras, listView);
            }
        }

        /// <summary>
        /// Format line of sight check results.
        /// </summary>
        private void FormatLineOfSightSummary(StarAnalysisResult result, ListView listView)
        {
            var losResult = result.LineOfSightResult;

            string losSummaryText = string.Format(
                "{0}  Line of Sight: {1}",
                losResult.IsValid ? "[+]" : "[-]",
                losResult.Label);

            var losDetails = new List<ListViewItem>();
            foreach (var camRes in losResult.CameraResults)
            {
                var di = new ListViewItem(string.Format("    {0}", camRes.CameraName));
                di.SubItems.Add(""); di.SubItems.Add("");
                di.SubItems.Add(""); di.SubItems.Add("");
                di.SubItems.Add(camRes.Label);
                di.Tag = TAG_EMITTER_DETAIL;
                di.BackColor = camRes.IsBlocked
                    ? Color.FromArgb(255, 235, 235)
                    : Color.FromArgb(235, 255, 235);
                losDetails.Add(di);
            }

            var losSummaryItem = new ListViewItem(losSummaryText);
            losSummaryItem.SubItems.Add(""); losSummaryItem.SubItems.Add("");
            losSummaryItem.SubItems.Add(""); losSummaryItem.SubItems.Add("");
            losSummaryItem.SubItems.Add(losResult.IsValid ? "OK" : "BLOCKED");
            losSummaryItem.BackColor = losResult.IsValid
                ? Color.FromArgb(220, 255, 220)
                : Color.FromArgb(255, 220, 220);
            losSummaryItem.Tag = new CollapsibleTag
            {
                TagType = TAG_EMITTER_DETAIL,
                DetailItems = losDetails
            };
            listView.Items.Add(losSummaryItem);

            // Expand if NOK
            if (!losResult.IsValid)
            {
                foreach (var detail in losDetails)
                    listView.Items.Add(detail);
                losSummaryItem.Text = losSummaryItem.Text.Replace("[+]", "[-]");
            }
        }

        /// <summary>
        /// Format emitter visibility results.
        /// </summary>
        private void FormatEmitterSummary(
            StarAnalysisResult result,
            EmitterData[] emitters,
            CameraData[] cameras,
            ListView listView)
        {
            var emitterResult = result.EmitterResult;

            string emitterSummaryText = string.Format(
                "{0}  Emitters: {1}/4 visible (min 3)",
                emitterResult.IsValid ? "[+]" : "[-]",
                emitterResult.VisibleEmitterCount);

            // Build detail rows
            var detailItems = new List<ListViewItem>();
            for (int e = 0; e < emitters.Length; e++)
            {
                for (int c = 0; c < cameras.Length; c++)
                {
                    var res = emitterResult.Results[e, c];
                    var detailItem = new ListViewItem(
                        string.Format("    {0} / {1}",
                            emitters[e].Name, cameras[c].Name));
                    detailItem.SubItems.Add("");
                    detailItem.SubItems.Add("");
                    detailItem.SubItems.Add("");
                    detailItem.SubItems.Add(string.Format(
                        "Max: {0} deg", result.MaxAngle));
                    detailItem.SubItems.Add(res.Label);
                    detailItem.Tag = TAG_EMITTER_DETAIL;
                    detailItem.BackColor = res.IsVisible
                        ? Color.FromArgb(235, 255, 235)
                        : Color.FromArgb(255, 235, 235);
                    detailItems.Add(detailItem);
                }
            }

            var emitterSummaryItem = new ListViewItem(emitterSummaryText);
            emitterSummaryItem.SubItems.Add("");
            emitterSummaryItem.SubItems.Add("");
            emitterSummaryItem.SubItems.Add("");
            emitterSummaryItem.SubItems.Add(string.Format(
                "E:{0}{1}{2}{3}",
                emitterResult.EmitterVisibleFromAllCameras[0] ? "1" : "-",
                emitterResult.EmitterVisibleFromAllCameras[1] ? "2" : "-",
                emitterResult.EmitterVisibleFromAllCameras[2] ? "3" : "-",
                emitterResult.EmitterVisibleFromAllCameras[3] ? "4" : "-"));
            emitterSummaryItem.SubItems.Add(
                emitterResult.IsValid ? "OK" : "NOK");
            emitterSummaryItem.BackColor = emitterResult.IsValid
                ? Color.FromArgb(220, 255, 220)
                : Color.FromArgb(255, 220, 220);
            emitterSummaryItem.Tag = new CollapsibleTag
            {
                TagType = TAG_EMITTER_SUMMARY,
                DetailItems = detailItems
            };

            listView.Items.Add(emitterSummaryItem);

            // Expand if NOK
            if (!emitterResult.IsValid)
            {
                foreach (var detail in detailItems)
                    listView.Items.Add(detail);
                emitterSummaryItem.Text = emitterSummaryItem.Text.Replace("[+]", "[-]");
            }
        }

        /// <summary>
        /// Format triangle analysis result.
        /// </summary>
        public void FormatTriangleResult(TriangleAnalysisResult result, ListView listView)
        {
            if (result == null || listView == null) return;

            string triSummaryText = string.Format(
                "{0}  Triangle Height: {1:F0} mm (min 500)",
                result.IsValid ? "[+]" : "[-]",
                result.Height);

            // Triangle detail rows
            var triDetails = new List<ListViewItem>();
            var triRows = new[]
            {
                new { Label = "Side Star1-Star2", Value = result.SideAB },
                new { Label = "Side Star2-Star3", Value = result.SideBC },
                new { Label = "Side Star3-Star1", Value = result.SideCA },
                new { Label = "Longest side",     Value = result.LongestSide },
            };
            foreach (var tr in triRows)
            {
                var di = new ListViewItem("  " + tr.Label);
                di.SubItems.Add(string.Format("{0:F0}", tr.Value));
                di.SubItems.Add("mm");
                di.SubItems.Add(""); di.SubItems.Add(""); di.SubItems.Add("");
                di.Tag = TAG_TRIANGLE_DETAIL;
                di.BackColor = Color.FromArgb(245, 245, 245);
                triDetails.Add(di);
            }

            var triSummaryItem = new ListViewItem(triSummaryText);
            triSummaryItem.SubItems.Add(""); triSummaryItem.SubItems.Add("");
            triSummaryItem.SubItems.Add(""); triSummaryItem.SubItems.Add("");
            triSummaryItem.SubItems.Add(result.IsValid ? "OK" : "FAIL");
            triSummaryItem.BackColor = result.IsValid
                ? Color.FromArgb(0, 220, 0)
                : Color.LightCoral;
            triSummaryItem.ForeColor = result.IsValid ? Color.White : Color.Black;
            triSummaryItem.Tag = new CollapsibleTag
            {
                TagType = TAG_TRIANGLE_SUMMARY,
                DetailItems = triDetails
            };
            listView.Items.Add(triSummaryItem);

            // Expand if FAIL
            if (!result.IsValid)
            {
                foreach (var detail in triDetails)
                    listView.Items.Add(detail);
                triSummaryItem.Text = triSummaryItem.Text.Replace("[+]", "[-]");
            }
        }

        /// <summary>
        /// Format a message indicating triangle was skipped.
        /// </summary>
        public void FormatTriangleSkipped(string reason, ListView listView)
        {
            if (listView == null) return;

            var skip = new ListViewItem("Triangle: not calculated");
            skip.SubItems.Add(""); skip.SubItems.Add("");
            skip.SubItems.Add(""); skip.SubItems.Add("");
            skip.SubItems.Add(reason);
            skip.BackColor = Color.LightYellow;
            listView.Items.Add(skip);
        }

        /// <summary>
        /// Handle expand/collapse logic for a clicked summary row.
        /// </summary>
        public void ToggleCollapsibleRow(ListViewItem clickedItem, ListView listView)
        {
            if (clickedItem.Tag is CollapsibleTag tag)
            {
                bool isExpanded = clickedItem.Text.StartsWith("[-]");
                int clickedIndex = listView.Items.IndexOf(clickedItem);

                if (isExpanded)
                {
                    // Collapse: remove detail rows
                    clickedItem.Text = clickedItem.Text.Replace("[-]", "[+]");
                    for (int i = listView.Items.Count - 1; i > clickedIndex; i--)
                    {
                        if (listView.Items[i].Tag is string detailTag &&
                            detailTag == tag.TagType)
                        {
                            listView.Items.RemoveAt(i);
                        }
                    }
                }
                else
                {
                    // Expand: insert detail rows
                    clickedItem.Text = clickedItem.Text.Replace("[+]", "[-]");
                    int insertAt = clickedIndex + 1;
                    foreach (var detail in tag.DetailItems)
                    {
                        var clone = (ListViewItem)detail.Clone();
                        listView.Items.Insert(insertAt++, clone);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Tag class for collapsible row metadata.
    /// </summary>
    public class CollapsibleTag
    {
        public string TagType { get; set; }
        public List<ListViewItem> DetailItems { get; set; }
    }
}
