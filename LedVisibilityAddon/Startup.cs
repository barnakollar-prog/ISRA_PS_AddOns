using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Tecnomatix.Engineering;
using Tecnomatix.Engineering.Ui;
using Tecnomatix.Engineering.DataTypes.Graphics;
using ISRA.Components.AccuSite.Trackers;
using ISRA.Components.AccuSite.Stars;
using ISRA.Calculations.AccuSite;

namespace LedVisibilityAddon
{
    public class LedVisibilityCommand : TxButtonCommand
    {
        public override string Category { get { return StringTable.CATEGORY; } }
        public override string Name { get { return StringTable.NAME; } }

        public override string Bitmap
        {
            get { return "star_515_0139_icon_16x16.bmp"; }
        }

        public override string LargeBitmap
        {
            get { return "star_515_0139_icon_32x32.png"; }
        }

        public override void Execute(object cmdParams)
        {
            var form = new VisibilityForm();
            form.Show();
        }
    }

    public class VisibilityForm : TxForm
    {
        private TxObjEditBoxCtrl pickerTracker;
        private TxObjEditBoxCtrl pickerStar1;
        private TxObjEditBoxCtrl pickerStar2;
        private TxObjEditBoxCtrl pickerStar3;
        private Button btnAnalyze;
        private Button btnExport;
        private Button btnHelp;
        private ListView lstResults;
        private List<TxComponent> _cylinderComponents = new List<TxComponent>();
        private NumericUpDown nudMaxAngle;

        // Tag constants for collapsible rows
        private const string TAG_EMITTER_DETAIL = "emitter_detail";
        private const string TAG_EMITTER_SUMMARY = "emitter_summary";
        private const string TAG_TRIANGLE_DETAIL = "triangle_detail";
        private const string TAG_TRIANGLE_SUMMARY = "triangle_summary";

        private readonly List<ITxDisplayableObject> _coloredObjects
            = new List<ITxDisplayableObject>();
        private TxComponent _triangleComponent = null;
        private TxComponent _emitterComponent = null;
        private readonly Tracker920_0005 _trackerFov = new Tracker920_0005();

        private static readonly TxColor ColorHighlight = new TxColor(255, 200, 0);
        private static readonly TxColor ColorOK = new TxColor(0, 200, 0);
        private static readonly TxColor ColorWarn = new TxColor(255, 165, 0);
        private static readonly TxColor ColorNOK = new TxColor(200, 0, 0);

        public VisibilityForm()
        {
            this.SemiModal = false;
            this.ShouldAutoPosition = true;
            this.ShouldCloseOnDocumentUnloading = true;
            this.FormClosing += OnFormClosing;
            BuildUI();
        }

        private void BuildUI()
        {
            this.Text = "Star Visibility Analyzer";
            this.Width = 680;
            this.Height = 780;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = true;
            this.MinimumSize = new Size(500, 680);
            this.StartPosition = FormStartPosition.CenterScreen;

            int lx = 10;
            int labelW = 65;
            int pickerW = 570;
            int rowH = 24;
            int y = 15;

            // ── Tracker ───────────────────────────────────────────
            var grpTracker = new GroupBox
            {
                Text = "Tracker (920-0005)",
                Left = lx,
                Top = y,
                Width = 645,
                Height = 58,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            grpTracker.Controls.Add(new Label
            {
                Text = "Tracker:",
                Left = 8,
                Top = 20,
                Width = labelW,
                Height = rowH,
                TextAlign = ContentAlignment.MiddleLeft
            });
            pickerTracker = new TxObjEditBoxCtrl
            {
                Left = labelW + 12,
                Top = 20,
                Width = pickerW,
                Height = rowH,
                ValidatorType = TxValidatorType.Component,
                PickLevel = TxPickLevel.Component,
                PickOnly = false,
                ListenToPick = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            pickerTracker.Picked += (s, e) =>
            {
                ApplyHighlightColor(pickerTracker.Object);
                pickerStar1.Focus();
            };
            grpTracker.Controls.Add(pickerTracker);
            y += 68;

            // ── Stars ─────────────────────────────────────────────
            var grpStars = new GroupBox
            {
                Text = "Stars (515-0139)",
                Left = lx,
                Top = y,
                Width = 645,
                Height = 118,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            int sy = 18;

            grpStars.Controls.Add(new Label
            {
                Text = "Star 1:",
                Left = 8,
                Top = sy + 2,
                Width = labelW,
                Height = rowH,
                TextAlign = ContentAlignment.MiddleLeft
            });
            pickerStar1 = new TxObjEditBoxCtrl
            {
                Left = labelW + 12,
                Top = sy,
                Width = pickerW,
                Height = rowH,
                ValidatorType = TxValidatorType.Component,
                PickLevel = TxPickLevel.Component,
                PickOnly = false,
                ListenToPick = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            pickerStar1.Picked += (s, e) =>
            {
                ApplyHighlightColor(pickerStar1.Object);
                pickerStar2.Focus();
            };
            grpStars.Controls.Add(pickerStar1);
            sy += 32;

            grpStars.Controls.Add(new Label
            {
                Text = "Star 2:",
                Left = 8,
                Top = sy + 2,
                Width = labelW,
                Height = rowH,
                TextAlign = ContentAlignment.MiddleLeft
            });
            pickerStar2 = new TxObjEditBoxCtrl
            {
                Left = labelW + 12,
                Top = sy,
                Width = pickerW,
                Height = rowH,
                ValidatorType = TxValidatorType.Component,
                PickLevel = TxPickLevel.Component,
                PickOnly = false,
                ListenToPick = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            pickerStar2.Picked += (s, e) =>
            {
                ApplyHighlightColor(pickerStar2.Object);
                pickerStar3.Focus();
            };
            grpStars.Controls.Add(pickerStar2);
            sy += 32;

            grpStars.Controls.Add(new Label
            {
                Text = "Star 3:",
                Left = 8,
                Top = sy + 2,
                Width = labelW,
                Height = rowH,
                TextAlign = ContentAlignment.MiddleLeft
            });
            pickerStar3 = new TxObjEditBoxCtrl
            {
                Left = labelW + 12,
                Top = sy,
                Width = pickerW,
                Height = rowH,
                ValidatorType = TxValidatorType.Component,
                PickLevel = TxPickLevel.Component,
                PickOnly = false,
                ListenToPick = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            pickerStar3.Picked += (s, e) =>
            {
                ApplyHighlightColor(pickerStar3.Object);
                btnAnalyze.Focus();
            };
            grpStars.Controls.Add(pickerStar3);
            y += 128;

            // ── Settings ──────────────────────────────────────────
            var grpSettings = new GroupBox
            {
                Text = "Settings",
                Left = lx,
                Top = y,
                Width = 645,
                Height = 48,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            grpSettings.Controls.Add(new Label
            {
                Text = "Max emitter angle (deg):",
                Left = 8,
                Top = 16,
                Width = 160,
                Height = rowH,
                TextAlign = ContentAlignment.MiddleLeft
            });
            nudMaxAngle = new NumericUpDown
            {
                Left = 172,
                Top = 16,
                Width = 70,
                Height = rowH,
                Minimum = 1,
                Maximum = 90,
                Value = 40,
                DecimalPlaces = 0
            };
            grpSettings.Controls.Add(nudMaxAngle);
            y += 58;

            // ── Analyze button ────────────────────────────────────
            btnAnalyze = new Button
            {
                Text = "Analyze",
                Left = lx,
                Top = y,
                Width = 645,
                Height = 36,
                BackColor = Color.FromArgb(180, 0, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            btnAnalyze.Click += OnAnalyze;
            y += 44;

            // ── Results ───────────────────────────────────────────
            var lblRes = new Label
            {
                Text = "Results: (double-click [+]/[-] to expand/collapse)",
                Left = lx,
                Top = y,
                Width = 400,
                Height = 20,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            y += 22;

            lstResults = new ListView
            {
                Left = lx,
                Top = y,
                Width = 645,
                Height = 260,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Font = new Font("Consolas", 9),
                Anchor = AnchorStyles.Top | AnchorStyles.Left |
                         AnchorStyles.Right | AnchorStyles.Bottom
            };
            lstResults.Columns.Add("Object", 200);
            lstResults.Columns.Add("Local X", 65);
            lstResults.Columns.Add("Local Y", 65);
            lstResults.Columns.Add("Local Z", 65);
            lstResults.Columns.Add("Note", 100);
            lstResults.Columns.Add("Status", 130);
            lstResults.DoubleClick += OnListDoubleClick;
            y += 268;

            // ── Export button ─────────────────────────────────────
            btnExport = new Button
            {
                Text = "Export to Excel",
                Left = lx,
                Top = y,
                Width = 645,
                Height = 32,
                BackColor = Color.FromArgb(0, 120, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            btnExport.Click += OnExport;
            y += 36;

            // ── Help button ───────────────────────────────────────
            btnHelp = new Button
            {
                Text = "Help / About",
                Left = lx,
                Top = y,
                Width = 645,
                Height = 28,
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            btnHelp.Click += (s, e) => HelpAbout.ShowAbout();

            this.Controls.Add(grpTracker);
            this.Controls.Add(grpStars);
            this.Controls.Add(grpSettings);
            this.Controls.Add(btnAnalyze);
            this.Controls.Add(lblRes);
            this.Controls.Add(lstResults);
            this.Controls.Add(btnExport);
            this.Controls.Add(btnHelp);
        }

        // ── Double click to expand/collapse ───────────────────────
        private void OnListDoubleClick(object sender, EventArgs e)
        {
            if (lstResults.SelectedItems.Count == 0) return;
            var selected = lstResults.SelectedItems[0];

            // Check if this is a collapsible summary row
            var ct = selected.Tag as CollapsibleTag;
            if (ct == null) return;

            int summaryIndex = lstResults.Items.IndexOf(selected);
            bool isExpanded = selected.Text.StartsWith("[-]");

            if (isExpanded)
            {
                // Collapse - remove detail rows after summary
                selected.Text = selected.Text.Replace("[-]", "[+]");
                int removeFrom = summaryIndex + 1;
                while (removeFrom < lstResults.Items.Count)
                {
                    var item = lstResults.Items[removeFrom];
                    string itemTag = item.Tag as string;
                    if (itemTag == TAG_EMITTER_DETAIL || itemTag == TAG_TRIANGLE_DETAIL)
                        lstResults.Items.RemoveAt(removeFrom);
                    else
                        break;
                }
            }
            else
            {
                // Expand - insert cloned detail rows
                selected.Text = selected.Text.Replace("[+]", "[-]");
                int insertAt = summaryIndex + 1;
                foreach (var detail in ct.DetailItems)
                {
                    var clone = (ListViewItem)detail.Clone();
                    lstResults.Items.Insert(insertAt++, clone);
                }
            }
        }

        // ── Analyze ───────────────────────────────────────────────
        private void OnAnalyze(object sender, EventArgs e)
        {
            lstResults.Items.Clear();

            var tracker = pickerTracker.Object;
            if (tracker == null)
            {
                MessageBox.Show("Please select the Tracker.", "Missing Input",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var trackerLoc = tracker as ITxLocatableObject;
            if (trackerLoc == null) return;

            TxTransformation trackerWorld = trackerLoc.AbsoluteLocation;
            double maxAngle = (double)nudMaxAngle.Value;

            var starPickers = new[] { pickerStar1, pickerStar2, pickerStar3 };
            bool anyResult = false;
            bool allOK = true;
            var localPositions = new TxVector[3];
            var worldPositions = new TxVector[3];

            GeometryCalculations.DeleteTriangleVisualization(ref _triangleComponent);
            DeleteEmitterVisualization();
            CollisionCheck.DeleteCylinderVisualizations(_cylinderComponents);

            for (int i = 0; i < starPickers.Length; i++)
            {
                var starObj = starPickers[i].Object;
                if (starObj == null) continue;
                anyResult = true;

                var starLoc = starObj as ITxLocatableObject;
                if (starLoc == null) continue;

                TxVector ledWorldPos = star_515_0139.GetLedWorldPosition(starLoc);
                TxVector localPt = _trackerFov.ToLocalCoordinates(
                    ledWorldPos, trackerWorld);
                localPositions[i] = localPt;
                worldPositions[i] = ledWorldPos;

                // Criterion 1: Position zone
                var zone = _trackerFov.GetPositionZone(localPt);
                string zoneLabel = _trackerFov.GetPositionZoneLabel(localPt);

                // Criterion 2: Emitter visibility
                var emitterResult = GeometryCalculations.CheckStarEmitterVisibility(
                    starLoc, trackerWorld, maxAngle);

                // Criterion 3: Line of sight check
                var losResult = CollisionCheck.CheckLineOfSight(
                    starLoc, trackerWorld, _cylinderComponents);

                bool starOK = zone != PositionZone.NOK &&
                              emitterResult.IsValid &&
                              losResult.IsValid;
                // Color in PS
                var displayable = starObj as ITxDisplayableObject;
                if (displayable != null)
                {
                    try
                    {
                        TxColor col;
                        if (zone == PositionZone.NOK ||
                            !emitterResult.IsValid)
                            col = ColorNOK;
                        else if (zone == PositionZone.Warning)
                            col = ColorWarn;
                        else
                            col = ColorOK;

                        displayable.Color = col;
                        if (!_coloredObjects.Contains(displayable))
                            _coloredObjects.Add(displayable);
                        TxApplication.RefreshDisplay();
                    }
                    catch { }
                }

                string shortName = starObj.Name.Length > 20
                    ? "..." + starObj.Name.Substring(starObj.Name.Length - 17)
                    : starObj.Name;

                // ── Star header row ───────────────────────────────
                var headerItem = new ListViewItem(shortName);
                headerItem.SubItems.Add(string.Format("{0:F0}", localPt.X));
                headerItem.SubItems.Add(string.Format("{0:F0}", localPt.Y));
                headerItem.SubItems.Add(string.Format("{0:F0}", localPt.Z));
                headerItem.SubItems.Add(_trackerFov.GetZoneName(localPt));
                headerItem.SubItems.Add(zoneLabel);
                headerItem.BackColor = zone == PositionZone.NOK
                    ? Color.LightCoral
                    : zone == PositionZone.Warning
                        ? Color.LightYellow
                        : Color.LightGreen;
                lstResults.Items.Add(headerItem);

                // ── Emitter summary row (collapsible) ─────────────
                var emitters = star_515_0139.GetEmitters();
                var cameras = tracker_920_0005.GetCameras();

                string emitterSummaryText = string.Format(
                    "{0}  Emitters: {1}/4 visible (min 3)",
                    emitterResult.IsValid ? "[+]" : "[-]",
                    emitterResult.VisibleEmitterCount);
                // Line of sight summary row
                var losSummaryText = string.Format(
                    "{0}  Line of Sight: {1}",
                    losResult.IsValid ? "[+]" : "[-]",
                    losResult.Label);

                var losDetails = new List<ListViewItem>();
                foreach (var camRes in losResult.CameraResults)
                {
                    var di = new ListViewItem(
                        string.Format("    {0}", camRes.CameraName));
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
                lstResults.Items.Add(losSummaryItem);

                if (!losResult.IsValid)
                {
                    foreach (var detail in losDetails)
                        lstResults.Items.Add(detail);
                    losSummaryItem.Text = losSummaryItem.Text.Replace("[+]", "[-]");
                }
                // Build detail rows
                var detailItems = new List<ListViewItem>();
                for (int e2 = 0; e2 < emitters.Length; e2++)
                {
                    for (int c = 0; c < cameras.Length; c++)
                    {
                        var res = emitterResult.Results[e2, c];
                        var detailItem = new ListViewItem(
                            string.Format("    {0} / {1}",
                                emitters[e2].Name, cameras[c].Name));
                        detailItem.SubItems.Add("");
                        detailItem.SubItems.Add("");
                        detailItem.SubItems.Add("");
                        detailItem.SubItems.Add(string.Format(
                            "Max: {0} deg", maxAngle));
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

                lstResults.Items.Add(emitterSummaryItem);

                // If NOK expand immediately
                if (!emitterResult.IsValid)
                {
                    foreach (var detail in detailItems)
                        lstResults.Items.Add(detail);
                    emitterSummaryItem.Text = emitterSummaryItem.Text
                        .Replace("[+]", "[-]");
                }
            }

            if (!anyResult)
            {
                MessageBox.Show("Please select at least one Star.", "Missing Input",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // ── Triangle ──────────────────────────────────────────
            GeometryCalculations.DeleteTriangleVisualization(ref _triangleComponent);

            if (allOK &&
                localPositions[0] != null &&
                localPositions[1] != null &&
                localPositions[2] != null)
            {
                var tri = GeometryCalculations.CalculateTriangleHeight(
                    localPositions[0], localPositions[1], localPositions[2]);

                // Triangle summary row
                string triSummaryText = string.Format(
                    "{0}  Triangle Height: {1:F0} mm (min 500)",
                    tri.IsValid ? "[+]" : "[-]",
                    tri.Height);

                // Triangle detail rows
                var triDetails = new List<ListViewItem>();
                var triRows = new[]
                {
                    new { Label = "Side Star1-Star2", Value = tri.SideAB },
                    new { Label = "Side Star2-Star3", Value = tri.SideBC },
                    new { Label = "Side Star3-Star1", Value = tri.SideCA },
                    new { Label = "Longest side",     Value = tri.LongestSide },
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
                triSummaryItem.SubItems.Add(tri.IsValid ? "OK" : "FAIL");
                triSummaryItem.BackColor = tri.IsValid
                    ? Color.FromArgb(0, 220, 0)
                    : Color.LightCoral;
                triSummaryItem.ForeColor = tri.IsValid ? Color.White : Color.Black;
                triSummaryItem.Tag = new CollapsibleTag
                {
                    TagType = TAG_TRIANGLE_SUMMARY,
                    DetailItems = triDetails
                };
                lstResults.Items.Add(triSummaryItem);

                // If FAIL expand immediately
                if (!tri.IsValid)
                {
                    foreach (var detail in triDetails)
                        lstResults.Items.Add(detail);
                    triSummaryItem.Text = triSummaryItem.Text
                        .Replace("[+]", "[-]");
                }

                // PS visualization
                try
                {
                    if (worldPositions[0] != null &&
                        worldPositions[1] != null &&
                        worldPositions[2] != null)
                    {
                        _triangleComponent = GeometryCalculations
                            .CreateTriangleVisualization(
                                worldPositions[0], worldPositions[1],
                                worldPositions[2], tri.IsValid);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Triangle visualization error: " + ex.Message,
                        "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else if (anyResult && !allOK)
            {
                var skip = new ListViewItem("Triangle: not calculated");
                skip.SubItems.Add(""); skip.SubItems.Add("");
                skip.SubItems.Add(""); skip.SubItems.Add("");
                skip.SubItems.Add("Not all stars OK");
                skip.BackColor = Color.LightYellow;
                lstResults.Items.Add(skip);
            }

            foreach (ColumnHeader col in lstResults.Columns)
                col.Width = -2;
        }

        // ── Emitter visualization ─────────────────────────────────
        private void DeleteEmitterVisualization()
        {
            try
            {
                if (_emitterComponent != null && _emitterComponent.IsValid())
                    _emitterComponent.Delete();
                _emitterComponent = null;
            }
            catch { }
        }

        // ── Export ────────────────────────────────────────────────
        private void OnExport(object sender, EventArgs e)
        {
            if (lstResults.Items.Count == 0)
            {
                MessageBox.Show("No results to export. Please run the analysis first.",
                    "No Data", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var rows = new List<ExportRow>();
            string trackerName = pickerTracker.Object != null
                ? pickerTracker.Object.Name : "Unknown";

            var pickers = new[] { pickerStar1, pickerStar2, pickerStar3 };
            foreach (var picker in pickers)
            {
                var starObj = picker.Object;
                if (starObj == null) continue;

                var starLoc = starObj as ITxLocatableObject;
                if (starLoc == null) continue;

                var trackerLoc = pickerTracker.Object as ITxLocatableObject;
                if (trackerLoc == null) continue;

                TxTransformation trackerWorld = trackerLoc.AbsoluteLocation;
                TxVector ledWorldPos = star_515_0139.GetLedWorldPosition(starLoc);
                TxVector localPt = _trackerFov.ToLocalCoordinates(
                    ledWorldPos, trackerWorld);

                var zone = _trackerFov.GetPositionZone(localPt);
                var emitterResult = GeometryCalculations.CheckStarEmitterVisibility(
                    starLoc, trackerWorld, (double)nudMaxAngle.Value);

                bool ok = zone != PositionZone.NOK &&
                          emitterResult.IsValid;

                rows.Add(new ExportRow
                {
                    StarName = starObj.Name,
                    LocalX = localPt.X,
                    LocalY = localPt.Y,
                    LocalZ = localPt.Z,
                    Zone = _trackerFov.GetZoneName(localPt),
                    InsideFov = ok,
                    TrackerName = trackerName
                });
            }

            GeometryCalculations.TriangleResult triResult = null;
            var trackerLoc2 = pickerTracker.Object as ITxLocatableObject;
            if (trackerLoc2 != null && rows.Count == 3 &&
                rows.TrueForAll(r => r.InsideFov))
            {
                TxTransformation tw = trackerLoc2.AbsoluteLocation;
                var pts = new TxVector[3];
                for (int i = 0; i < 3; i++)
                {
                    var sl = pickers[i].Object as ITxLocatableObject;
                    if (sl != null)
                        pts[i] = _trackerFov.ToLocalCoordinates(
                            star_515_0139.GetLedWorldPosition(sl), tw);
                }
                if (pts[0] != null && pts[1] != null && pts[2] != null)
                    triResult = GeometryCalculations.CalculateTriangleHeight(
                        pts[0], pts[1], pts[2]);
            }

            ExcelExporter.Export(rows, trackerName, triResult);
        }

        // ── Color helpers ─────────────────────────────────────────
        private void ApplyHighlightColor(ITxObject obj)
        {
            if (obj == null) return;
            var displayable = obj as ITxDisplayableObject;
            if (displayable == null) return;
            try
            {
                displayable.Color = ColorHighlight;
                if (!_coloredObjects.Contains(displayable))
                    _coloredObjects.Add(displayable);
                TxApplication.RefreshDisplay();
            }
            catch { }
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            GeometryCalculations.DeleteTriangleVisualization(ref _triangleComponent);
            DeleteEmitterVisualization();
            CollisionCheck.DeleteCylinderVisualizations(_cylinderComponents);
            foreach (var obj in _coloredObjects)
            {
                try { obj.RestoreColor(); }
                catch { }
            }
            _coloredObjects.Clear();
            try { TxApplication.RefreshDisplay(); }
            catch { }
        }
    }

    /// <summary>
    /// Helper class to store collapsible row data.
    /// </summary>
    public class CollapsibleTag
    {
        public string TagType { get; set; }
        public List<ListViewItem> DetailItems { get; set; }
    }
}