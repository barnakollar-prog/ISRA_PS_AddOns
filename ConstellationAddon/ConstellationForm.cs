using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Tecnomatix.Engineering;
using Tecnomatix.Engineering.Ui;
using ISRA.Calculations.AccuSite;
using ISRA.Components.AccuSite.SensorHolders;
using ISRA.Components.AccuSite.Trackers;
using ISRA.Core.Utilities;

namespace ConstellationAddon
{
    public class ConstellationForm : TxForm
    {
        // ── Controls ──────────────────────────────────────────────
        private TxObjEditBoxCtrl pickerRobot;
        private TxObjEditBoxCtrl pickerTracker;
        private ListView lstPaths;
        private Button btnPickPaths;
        private Button btnClearPaths;
        private Button btnSetCollision;
        private Label lblCollisionCount;
        private TextBox txtJsonPath;
        private Button btnBrowseJson;
        private Button btnImportExcel;
        private Button btnRun;
        private ListView lstResults;

        // ── State ─────────────────────────────────────────────────
        private readonly List<TxWeldOperation> _paths
            = new List<TxWeldOperation>();
        private List<TxComponent> _collisionObjects
            = new List<TxComponent>();
        private List<TxComponent> _visComponents
            = new List<TxComponent>();
        private bool _pickingPaths = false;

        public ConstellationForm()
        {
            this.SemiModal = false;
            this.ShouldAutoPosition = true;
            this.ShouldCloseOnDocumentUnloading = true;
            this.FormClosing += OnFormClosing;
            BuildUI();
            TxApplication.ActiveSelection.ItemsSet += OnSelectionChanged;
            TxApplication.ActiveSelection.ItemsAdded += OnSelectionAdded;
        }

        private void BuildUI()
        {
            this.Text = "Constellation Validator";
            this.Width = 820;
            this.Height = 780;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimumSize = new Size(600, 600);

            int lx = 10;
            int y = 10;

            // ── Robot ─────────────────────────────────────────────
            var grpRobot = new GroupBox
            {
                Text = "Robot",
                Left = lx,
                Top = y,
                Width = 785,
                Height = 52,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            grpRobot.Controls.Add(new Label
            {
                Text = "Robot:",
                Left = 8,
                Top = 18,
                Width = 60,
                Height = 24,
                TextAlign = ContentAlignment.MiddleLeft
            });
            pickerRobot = new TxObjEditBoxCtrl
            {
                Left = 72,
                Top = 18,
                Width = 700,
                Height = 24,
                ValidatorType = TxValidatorType.Robot,
                PickLevel = TxPickLevel.Component,
                PickOnly = false,
                ListenToPick = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            grpRobot.Controls.Add(pickerRobot);
            this.Controls.Add(grpRobot);
            y += 62;

            // ── Tracker ───────────────────────────────────────────
            var grpTracker = new GroupBox
            {
                Text = "Tracker",
                Left = lx,
                Top = y,
                Width = 785,
                Height = 52,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            grpTracker.Controls.Add(new Label
            {
                Text = "Tracker:",
                Left = 8,
                Top = 18,
                Width = 60,
                Height = 24,
                TextAlign = ContentAlignment.MiddleLeft
            });
            pickerTracker = new TxObjEditBoxCtrl
            {
                Left = 72,
                Top = 18,
                Width = 700,
                Height = 24,
                ValidatorType = TxValidatorType.Component,
                PickLevel = TxPickLevel.Component,
                PickOnly = false,
                ListenToPick = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            grpTracker.Controls.Add(pickerTracker);
            this.Controls.Add(grpTracker);
            y += 62;

            // ── Paths ─────────────────────────────────────────────
            var grpPaths = new GroupBox
            {
                Text = "Measurement Paths",
                Left = lx,
                Top = y,
                Width = 785,
                Height = 110,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            lstPaths = new ListView
            {
                Left = 8,
                Top = 18,
                Width = 688,
                Height = 78,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Font = new Font("Consolas", 8),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            lstPaths.Columns.Add("Path Name", 670);
            grpPaths.Controls.Add(lstPaths);

            btnPickPaths = new Button
            {
                Text = "Pick",
                Left = 704,
                Top = 18,
                Width = 74,
                Height = 28,
                BackColor = Color.FromArgb(0, 100, 180),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnPickPaths.Click += OnPickPathsClick;
            grpPaths.Controls.Add(btnPickPaths);

            btnClearPaths = new Button
            {
                Text = "Clear",
                Left = 704,
                Top = 52,
                Width = 74,
                Height = 28,
                BackColor = Color.FromArgb(100, 100, 100),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnClearPaths.Click += (s, e) => { _paths.Clear(); lstPaths.Items.Clear(); };
            grpPaths.Controls.Add(btnClearPaths);
            this.Controls.Add(grpPaths);
            y += 120;

            // ── Collision Objects ─────────────────────────────────
            var grpCollision = new GroupBox
            {
                Text = "Collision Objects (select in PS, then click Set)",
                Left = lx,
                Top = y,
                Width = 785,
                Height = 52,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            btnSetCollision = new Button
            {
                Text = "Set from Selection",
                Left = 8,
                Top = 16,
                Width = 150,
                Height = 26,
                BackColor = Color.FromArgb(0, 100, 180),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSetCollision.Click += OnSetCollisionClick;
            grpCollision.Controls.Add(btnSetCollision);

            lblCollisionCount = new Label
            {
                Text = "No objects selected",
                Left = 168,
                Top = 18,
                Width = 400,
                Height = 22,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.Gray
            };
            grpCollision.Controls.Add(lblCollisionCount);
            this.Controls.Add(grpCollision);
            y += 62;

            // ── MP Feature JSON ───────────────────────────────────
            var grpJson = new GroupBox
            {
                Text = "MP Feature JSON (from Excel import)",
                Left = lx,
                Top = y,
                Width = 785,
                Height = 80,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            txtJsonPath = new TextBox
            {
                Left = 8,
                Top = 18,
                Width = 580,
                Height = 24,
                ReadOnly = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            grpJson.Controls.Add(txtJsonPath);

            btnBrowseJson = new Button
            {
                Text = "Browse...",
                Left = 596,
                Top = 16,
                Width = 80,
                Height = 26,
                BackColor = Color.FromArgb(80, 80, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnBrowseJson.Click += OnBrowseJsonClick;
            grpJson.Controls.Add(btnBrowseJson);

            btnImportExcel = new Button
            {
                Text = "Import from Excel...",
                Left = 684,
                Top = 16,
                Width = 92,
                Height = 26,
                BackColor = Color.FromArgb(0, 120, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnImportExcel.Click += OnImportExcelClick;
            grpJson.Controls.Add(btnImportExcel);

            grpJson.Controls.Add(new Label
            {
                Text = "JSON provides MP feature types and allowed TCP rotation ranges (Aiming Guidelines).",
                Left = 8,
                Top = 50,
                Width = 760,
                Height = 20,
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 7.5f, FontStyle.Italic)
            });
            this.Controls.Add(grpJson);
            y += 90;

            // ── Run button ────────────────────────────────────────
            btnRun = new Button
            {
                Text = "Run Constellation Check",
                Left = lx,
                Top = y,
                Width = 785,
                Height = 36,
                BackColor = Color.FromArgb(180, 0, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            btnRun.Click += OnRun;
            this.Controls.Add(btnRun);
            y += 44;

            // ── Results ───────────────────────────────────────────
            var grpResults = new GroupBox
            {
                Text = "Results",
                Left = lx,
                Top = y,
                Width = 785,
                Height = this.Height - y - 50,
                Anchor = AnchorStyles.Top | AnchorStyles.Left |
                         AnchorStyles.Right | AnchorStyles.Bottom
            };
            lstResults = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Font = new Font("Consolas", 8)
            };
            lstResults.Columns.Add("Location", 160);
            lstResults.Columns.Add("Status", 80);
            lstResults.Columns.Add("Plane", 60);
            lstResults.Columns.Add("Visible LEDs", 90);
            lstResults.Columns.Add("Details", 350);
            grpResults.Controls.Add(lstResults);
            this.Controls.Add(grpResults);
        }

        // ── Path picking ──────────────────────────────────────────

        private void OnPickPathsClick(object sender, EventArgs e)
        {
            if (_pickingPaths) StopPickingPaths();
            else StartPickingPaths();
        }

        private void StartPickingPaths()
        {
            _pickingPaths = true;
            btnPickPaths.Text = "OK";
            btnPickPaths.BackColor = Color.FromArgb(0, 160, 0);
            AddPathsFromSelection(TxApplication.ActiveSelection.GetItems());
        }

        private void StopPickingPaths()
        {
            _pickingPaths = false;
            btnPickPaths.Text = "Pick";
            btnPickPaths.BackColor = Color.FromArgb(0, 100, 180);
        }

        private void OnSelectionChanged(object sender, TxSelection_ItemsSetEventArgs e)
        {
            if (!_pickingPaths) return;
            AddPathsFromSelection(TxApplication.ActiveSelection.GetItems());
        }

        private void OnSelectionAdded(object sender, TxSelection_ItemsAddedEventArgs e)
        {
            if (!_pickingPaths) return;
            AddPathsFromSelection(TxApplication.ActiveSelection.GetItems());
        }

        private void AddPathsFromSelection(TxObjectList items)
        {
            foreach (ITxObject obj in items)
            {
                var prog = obj as TxWeldOperation;
                if (prog != null && !_paths.Contains(prog))
                {
                    _paths.Add(prog);
                    lstPaths.Items.Add(new ListViewItem(prog.Name));
                    continue;
                }
                var compound = obj as ITxCompoundOperation;
                if (compound != null)
                {
                    var children = compound.GetAllDescendants(
                        new TxTypeFilter(typeof(TxWeldOperation)));
                    AddPathsFromSelection(children);
                }
            }
        }

        // ── Collision objects ─────────────────────────────────────

        private void OnSetCollisionClick(object sender, EventArgs e)
        {
            _collisionObjects = RobotCollisionCheck.GetCollisionObjectsFromSelection();
            lblCollisionCount.Text = string.Format(
                "{0} object(s) set for collision check", _collisionObjects.Count);
            lblCollisionCount.ForeColor = _collisionObjects.Count > 0
                ? Color.DarkGreen : Color.Gray;
        }

        // ── JSON / Excel ──────────────────────────────────────────

        private void OnBrowseJsonClick(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Filter = "JSON files|*.json|All files|*.*";
                dlg.Title = "Select MP Feature JSON file";
                if (dlg.ShowDialog() == DialogResult.OK)
                    txtJsonPath.Text = dlg.FileName;
            }
        }

        private void OnImportExcelClick(object sender, EventArgs e)
        {
            string excelPath;
            using (var dlg = new OpenFileDialog())
            {
                dlg.Filter = "Excel files|*.xls;*.xlsx|All files|*.*";
                dlg.Title = "Select MP Excel file";
                if (dlg.ShowDialog() != DialogResult.OK) return;
                excelPath = dlg.FileName;
            }

            string jsonPath;
            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter = "JSON files|*.json";
                dlg.Title = "Save MP Feature JSON as";
                dlg.FileName = "mp_features.json";
                if (dlg.ShowDialog() != DialogResult.OK) return;
                jsonPath = dlg.FileName;
            }

            var result = MpFeatureExcelImporter.Import(excelPath, jsonPath);

            if (result.Success)
            {
                txtJsonPath.Text = jsonPath;
                string msg = string.Format(
                    "Import successful.\n{0} levels imported, {1} rows skipped.",
                    result.LevelCount, result.SkippedCount);
                if (result.Warnings.Count > 0)
                    msg += "\n\nWarnings:\n" + string.Join("\n", result.Warnings);
                MessageBox.Show(msg, "Import Complete",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Import failed: " + result.ErrorMessage,
                    "Import Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Run ───────────────────────────────────────────────────

        private void OnRun(object sender, EventArgs e)
        {
            // Validate inputs
            var robot = pickerRobot.Object as TxRobot;
            if (robot == null)
            {
                MessageBox.Show("Please select a Robot.", "Missing Input",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var trackerComp = pickerTracker.Object as ITxLocatableObject;
            if (trackerComp == null)
            {
                MessageBox.Show("Please select a Tracker.", "Missing Input",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_paths.Count == 0)
            {
                MessageBox.Show("Please select at least one path.", "Missing Input",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Clean up previous visualization
            ConstellationVisibilityChecker.DeleteVisualizations(_visComponents);
            lstResults.Items.Clear();

            // Tracker world transform
            TxTransformation trackerWorld = trackerComp.AbsoluteLocation;
            ITracker tracker = new Tracker920_0005();

            // MP feature lookup (optional — JSON may not be loaded)
            MpFeatureLookup featureLookup = null;
            if (!string.IsNullOrEmpty(txtJsonPath.Text) && File.Exists(txtJsonPath.Text))
            {
                try { featureLookup = MpFeatureLookup.LoadFromJson(txtJsonPath.Text); }
                catch { /* proceed without feature lookup */ }
            }

            // Run path checker
            var checker = new ConstellationPathChecker(
                robot, tracker, trackerWorld, _collisionObjects);

            foreach (var path in _paths)
            {
                ConstellationPathResult pathResult;
                try
                {
                    pathResult = checker.CheckPath(path, _visComponents);
                }
                catch (Exception ex)
                {
                    lstResults.Items.Add(new ListViewItem(new[]
                    {
                        path.Name, "ERROR", "", "", ex.Message
                    })
                    { ForeColor = Color.Red });
                    continue;
                }

                foreach (var pt in pathResult.PointResults)
                {
                    string status = pt.HasCollision ? "COLLISION" :
                                     !pt.RobotReached ? "SKIPPED" :
                                     pt.Criteria != null && pt.Criteria.IsOk ? "OK" : "NOK";

                    string plane = pt.Criteria != null && pt.Criteria.SatisfiedPlane != null
                                     ? pt.Criteria.SatisfiedPlane : "";

                    string visible = pt.Visibility != null
                                     ? pt.Visibility.TotalVisibleCount.ToString()
                                     : "";

                    string details = pt.Label ?? "";

                    var item = new ListViewItem(new[]
                    {
                        pt.LocationName, status, plane, visible, details
                    });

                    item.ForeColor = status == "OK" ? Color.DarkGreen :
                                     status == "NOK" ? Color.DarkRed :
                                     status == "COLLISION" ? Color.OrangeRed :
                                                            Color.Gray;

                    lstResults.Items.Add(item);
                }
            }

            // Summary
            int ok = 0; int nok = 0; int skip = 0; int coll = 0;
            foreach (ListViewItem item in lstResults.Items)
            {
                switch (item.SubItems[1].Text)
                {
                    case "OK": ok++; break;
                    case "NOK": nok++; break;
                    case "SKIPPED": skip++; break;
                    case "COLLISION": coll++; break;
                }
            }

            MessageBox.Show(
                string.Format("Done.\nOK: {0}  NOK: {1}  Collision: {2}  Skipped: {3}",
                    ok, nok, coll, skip),
                "Constellation Check Complete",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ── Cleanup ───────────────────────────────────────────────

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                TxApplication.ActiveSelection.ItemsSet -= OnSelectionChanged;
                TxApplication.ActiveSelection.ItemsAdded -= OnSelectionAdded;
                ConstellationVisibilityChecker.DeleteVisualizations(_visComponents);
            }
            catch { }
        }
    }
}