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
using LedVisibilityAddon.Presentation;

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

    public class VisibilityForm : TxForm, ILedVisibilityView
    {
        // ── MVP ───────────────────────────────────────────────────
        private readonly LedVisibilityPresenter _presenter;
        private readonly LedVisibilityReportFormatter _formatter;

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
            _presenter = new LedVisibilityPresenter(this);
            _formatter = new LedVisibilityReportFormatter();

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

            _formatter.ToggleCollapsibleRow(selected, lstResults);
        }

        // ── Analyze ───────────────────────────────────────────────
        private void OnAnalyze(object sender, EventArgs e)
        {
            _presenter.Analyze();
        }

        // ── ILedVisibilityView Implementation ─────────────────────
        public ITxObject SelectedTracker => pickerTracker.Object;

        public List<ITxObject> SelectedStars
        {
            get
            {
                var stars = new List<ITxObject>();
                var starPickers = new[] { pickerStar1, pickerStar2, pickerStar3 };
                foreach (var picker in starPickers)
                {
                    if (picker.Object != null)
                        stars.Add(picker.Object);
                }
                return stars;
            }
        }

        public double MaxAngle => (double)nudMaxAngle.Value;

        public List<TxComponent> CylinderComponents => _cylinderComponents;

        public void ClearResults()
        {
            lstResults.Items.Clear();
        }

        public void DeleteVisualizations()
        {
            GeometryCalculations.DeleteTriangleVisualization(ref _triangleComponent);
            DeleteEmitterVisualization();
            CollisionCheck.DeleteCylinderVisualizations(_cylinderComponents);
        }

        public void DisplayStarResults(List<StarAnalysisResult> results)
        {
            _formatter.FormatStarResults(results, lstResults);
        }

        public void DisplayTriangleResult(TriangleAnalysisResult result)
        {
            _formatter.FormatTriangleResult(result, lstResults);

            // Create PS visualization
            try
            {
                if (result.WorldPositions != null && result.WorldPositions.Count == 3)
                {
                    _triangleComponent = GeometryCalculations.CreateTriangleVisualization(
                        result.WorldPositions[0],
                        result.WorldPositions[1],
                        result.WorldPositions[2],
                        result.IsValid);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Triangle visualization error: " + ex.Message,
                    "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            AutoResizeColumns();
        }

        public void DisplayTriangleSkipped(string reason)
        {
            _formatter.FormatTriangleSkipped(reason, lstResults);
            AutoResizeColumns();
        }

        public void ColorizeStarInScene(ITxObject starObj, PositionZone zone, bool emitterValid)
        {
            var displayable = starObj as ITxDisplayableObject;
            if (displayable == null) return;

            try
            {
                TxColor col;
                if (zone == PositionZone.NOK || !emitterValid)
                    col = ColorNOK;
                else if (zone == PositionZone.Warning)
                    col = ColorWarn;
                else
                    col = ColorOK;

                displayable.Color = col;
                if (!_coloredObjects.Contains(displayable))
                    _coloredObjects.Add(displayable);
            }
            catch { }
        }

        public void RefreshDisplay()
        {
            try
            {
                TxApplication.RefreshDisplay();
            }
            catch { }
        }

        public void ShowError(string message, string title)
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public bool HasResults => lstResults.Items.Count > 0;

        public void ShowExportDialog(ExportData data)
        {
            ExcelExporter.Export(data.Rows, data.TrackerName, data.Triangle);
        }

        private void AutoResizeColumns()
        {
            foreach (ColumnHeader col in lstResults.Columns)
                col.Width = -2;
        }

        // ── Helpers ───────────────────────────────────────────────

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
            _presenter.Export();
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
}