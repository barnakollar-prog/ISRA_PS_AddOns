using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Tecnomatix.Engineering;
using Tecnomatix.Engineering.DataTypes.Graphics;
using Tecnomatix.Engineering.Ui;
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

        private readonly List<ITxDisplayableObject> _coloredObjects
            = new List<ITxDisplayableObject>();
        private TxComponent _triangleComponent = null;
        private readonly tracker_920_0005 _trackerFov = new tracker_920_0005();

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
            this.Height = 720;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = true;
            this.MinimumSize = new Size(500, 620);
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

            // Star 1
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

            // Star 2
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

            // Star 3
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
                Text = "Results:",
                Left = lx,
                Top = y,
                Width = 100,
                Height = 20,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            y += 22;

            lstResults = new ListView
 

            {
                Left = lx,
                Top = y,
                Width = 645,
                Height = 220,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Font = new Font("Consolas", 9),
                Anchor = AnchorStyles.Top | AnchorStyles.Left |
                         AnchorStyles.Right | AnchorStyles.Bottom
            };
            lstResults.Columns.Add("Object", 160);
            lstResults.Columns.Add("Local X", 65);
            lstResults.Columns.Add("Local Y", 65);
            lstResults.Columns.Add("Local Z", 65);
            lstResults.Columns.Add("Zone / Note", 100);
            lstResults.Columns.Add("Orientation", 95);
            lstResults.Columns.Add("Position", 95);
            lstResults.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            y += 228;

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
            this.Controls.Add(btnAnalyze);
            this.Controls.Add(lblRes);
            this.Controls.Add(lstResults);
            this.Controls.Add(btnExport);
            this.Controls.Add(btnHelp);
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
            if (trackerLoc == null)
            {
                MessageBox.Show("Tracker is not locatable.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            TxTransformation trackerWorld = trackerLoc.AbsoluteLocation;

            var starPickers = new[] { pickerStar1, pickerStar2, pickerStar3 };
            bool anyResult = false;
            bool allOK = true;
            var localPositions = new TxVector[3];
            var worldPositions = new TxVector[3];

            // ── Per star analysis ─────────────────────────────────
            for (int i = 0; i < starPickers.Length; i++)
            {
                var starObj = starPickers[i].Object;
                if (starObj == null) continue;
                anyResult = true;

                var starLoc = starObj as ITxLocatableObject;
                if (starLoc == null) continue;

                TxTransformation starWorld = starLoc.AbsoluteLocation;
                TxVector ledWorldPos = star_515_0139.GetLedWorldPosition(starLoc);
                TxVector localPt = _trackerFov.ToLocalCoordinates(ledWorldPos, trackerWorld);
                localPositions[i] = localPt;
                worldPositions[i] = ledWorldPos;

                // Criterion 1: Z vector orientation
                var angleResult = GeometryCalculations.CalculateStarTrackerAngle(
                    starWorld, trackerWorld);


                // Criterion 2: Position zone
                var zone = _trackerFov.GetPositionZone(localPt);
                string zoneLabel = _trackerFov.GetPositionZoneLabel(localPt);

                bool starOK = angleResult.IsValid &&
                    zone != tracker_920_0005.PositionZone.NOK;
                if (!starOK) allOK = false;

                // Color in PS
                var displayable = starObj as ITxDisplayableObject;
                if (displayable != null)
                {
                    try
                    {
                        TxColor col;
                        if (!angleResult.IsValid ||
                            zone == tracker_920_0005.PositionZone.NOK)
                            col = ColorNOK;
                        else if (zone == tracker_920_0005.PositionZone.Warning)
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

                // Star header row
                var headerItem = new ListViewItem(shortName);
                headerItem.SubItems.Add(string.Format("{0:F0}", localPt.X));
                headerItem.SubItems.Add(string.Format("{0:F0}", localPt.Y));
                headerItem.SubItems.Add(string.Format("{0:F0}", localPt.Z));
                headerItem.SubItems.Add(_trackerFov.GetZoneName(localPt));
                headerItem.SubItems.Add("");
                headerItem.SubItems.Add(zoneLabel);

                if (!angleResult.IsValid ||
                    zone == tracker_920_0005.PositionZone.NOK)
                    headerItem.BackColor = Color.LightCoral;
                else if (zone == tracker_920_0005.PositionZone.Warning)
                    headerItem.BackColor = Color.LightYellow;
                else
                    headerItem.BackColor = Color.LightGreen;

                lstResults.Items.Add(headerItem);

                // Orientation XZ row
                var xzItem = new ListViewItem("  Orient. XZ (Y-rot)");
                xzItem.SubItems.Add("");
                xzItem.SubItems.Add("");
                xzItem.SubItems.Add("");
                xzItem.SubItems.Add("Max: 25 deg");
                xzItem.SubItems.Add(angleResult.LabelXZ);
                xzItem.SubItems.Add("");
                xzItem.BackColor = angleResult.IsValidXZ
                    ? Color.FromArgb(220, 255, 220)
                    : Color.FromArgb(255, 220, 220);
                lstResults.Items.Add(xzItem);

                // Orientation YZ row
                var yzItem = new ListViewItem("  Orient. YZ (X-rot)");
                yzItem.SubItems.Add("");
                yzItem.SubItems.Add("");
                yzItem.SubItems.Add("");
                yzItem.SubItems.Add("Max: 40 deg");
                yzItem.SubItems.Add(angleResult.LabelYZ);
                yzItem.SubItems.Add("");
                yzItem.BackColor = angleResult.IsValidYZ
                    ? Color.FromArgb(220, 255, 220)
                    : Color.FromArgb(255, 220, 220);
                lstResults.Items.Add(yzItem);
            }

            if (!anyResult)
            {
                MessageBox.Show("Please select at least one Star.", "Missing Input",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // ── Triangle calculation ──────────────────────────────
            GeometryCalculations.DeleteTriangleVisualization(ref _triangleComponent);

            if (allOK &&
                localPositions[0] != null &&
                localPositions[1] != null &&
                localPositions[2] != null)
            {
                var tri = GeometryCalculations.CalculateTriangleHeight(
                    localPositions[0], localPositions[1], localPositions[2]);

                var sep = new ListViewItem("--- Triangle Analysis ---");
                for (int i = 0; i < 6; i++) sep.SubItems.Add("");
                sep.BackColor = Color.LightGray;
                lstResults.Items.Add(sep);

                AddTriRow("Side Star1-Star2", tri.SideAB, "mm", "");
                AddTriRow("Side Star2-Star3", tri.SideBC, "mm", "");
                AddTriRow("Side Star3-Star1", tri.SideCA, "mm", "");
                AddTriRow("Longest side", tri.LongestSide, "mm",
                    tri.LongestSideName, Color.LightYellow);

                var heightItem = new ListViewItem("Triangle Height");
                heightItem.SubItems.Add(string.Format("{0:F0}", tri.Height));
                heightItem.SubItems.Add("mm");
                heightItem.SubItems.Add("");
                heightItem.SubItems.Add("Min: 500 mm");
                heightItem.SubItems.Add(tri.IsValid ? "OK" : "FAIL");
                heightItem.SubItems.Add("");
                heightItem.BackColor = tri.IsValid
                    ? Color.FromArgb(0, 220, 0)
                    : Color.LightCoral;
                heightItem.ForeColor = tri.IsValid ? Color.White : Color.Black;
                lstResults.Items.Add(heightItem);

                try
                {
                    if (worldPositions[0] != null &&
                        worldPositions[1] != null &&
                        worldPositions[2] != null)
                    {
                        _triangleComponent = GeometryCalculations.CreateTriangleVisualization(
                            worldPositions[0], worldPositions[1], worldPositions[2],
                            tri.IsValid);
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
                var sep = new ListViewItem("--- Triangle Analysis ---");
                for (int i = 0; i < 6; i++) sep.SubItems.Add("");
                sep.BackColor = Color.LightGray;
                lstResults.Items.Add(sep);

                var skip = new ListViewItem("Triangle not calculated");
                skip.SubItems.Add(""); skip.SubItems.Add("");
                skip.SubItems.Add(""); skip.SubItems.Add("");
                skip.SubItems.Add("Not all stars OK");
                skip.SubItems.Add("");
                skip.BackColor = Color.LightYellow;
                lstResults.Items.Add(skip);
            }
            foreach(ColumnHeader col in lstResults.Columns)
            col.Width = -2;
        }

        private void AddTriRow(string label, double value, string unit,
            string note, Color? bg = null)
        {
            var item = new ListViewItem(label);
            item.SubItems.Add(string.Format("{0:F0}", value));
            item.SubItems.Add(unit);
            item.SubItems.Add("");
            item.SubItems.Add(note);
            item.SubItems.Add("");
            item.SubItems.Add("");
            if (bg.HasValue) item.BackColor = bg.Value;
            lstResults.Items.Add(item);
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
                TxVector localPt = _trackerFov.ToLocalCoordinates(ledWorldPos, trackerWorld);

                var angleResult = GeometryCalculations.CalculateStarTrackerAngle(
                    starLoc.AbsoluteLocation, trackerWorld);
                var zone = _trackerFov.GetPositionZone(localPt);

                bool ok = angleResult.IsValid &&
                    zone != tracker_920_0005.PositionZone.NOK;

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
}