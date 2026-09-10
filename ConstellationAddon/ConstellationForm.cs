using ISRA.Calculations.AccuSite;
using ISRA.Components.AccuSite.SensorHolders;
using ISRA.Components.AccuSite.Trackers;
using ISRA.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Tecnomatix.Engineering;
using Tecnomatix.Engineering.Olp.OLP_Utilities;
using Tecnomatix.Engineering.OLP;
using Tecnomatix.Engineering.Ui;
using ISRA.Calculations.AccuSite;

namespace ConstellationAddon
{
    public class ConstellationForm : TxForm
    {
        // ── Controls ──────────────────────────────────────────────
        private TxObjEditBoxCtrl pickerRobot;
        private TxObjEditBoxCtrl[] pickerTrackers;
        private ComboBox cmbHolderType;
        private ListView lstPaths;
        private Button btnPickPaths;
        private Button btnClearPaths;
        private ComboBox cmbCollisionPair;
        private Button btnAnalyze;
        private ListView lstResults;
        private TabControl tabResults;
        private TreeView treeAngleDetails;
        private CheckBox chkShowOkLines;
        private CheckBox chkShowNokLines;
        private CheckBox chkShowFovLines;
        private string _currentPointName = null;

        // ── State ─────────────────────────────────────────────────
        private readonly List<TxWeldOperation> _paths
            = new List<TxWeldOperation>();
        private List<TxComponent> _visComponents
            = new List<TxComponent>();
        private bool _pickingPaths = false;
        private readonly Dictionary<string, ConstellationVisibilityResult> _pointVisibility
    = new Dictionary<string, ConstellationVisibilityResult>();
        private readonly Dictionary<string, string> _pointTrackerLabel
            = new Dictionary<string, string>();
        private List<TxComponent> _currentPointVis
            = new List<TxComponent>();


        // ── Configuration — change to scale up ───────────────────
        private const int TrackerCount = 4;  // → 8 when needed
        private const int TrackerCols = 2;  // → 4 when needed

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
            this.Width = 840;
            this.Height = 860;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimumSize = new Size(640, 660);

            int lx = 10;
            int y = 10;

            // ── Robot ─────────────────────────────────────────────
            var grpRobot = new GroupBox
            {
                Text = "Robot",
                Left = lx,
                Top = y,
                Width = 806,
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
                Width = 720,
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

            // ── Sensor Holder ─────────────────────────────────────
            var grpHolder = new GroupBox
            {
                Text = "Sensor Holder Type",
                Left = lx,
                Top = y,
                Width = 806,
                Height = 52,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            grpHolder.Controls.Add(new Label
            {
                Text = "Type:",
                Left = 8,
                Top = 18,
                Width = 50,
                Height = 24,
                TextAlign = ContentAlignment.MiddleLeft
            });
            cmbHolderType = new ComboBox
            {
                Left = 62,
                Top = 16,
                Width = 730,
                Height = 24,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            foreach (var typeId in SensorHolderCatalog.All.Keys)
                cmbHolderType.Items.Add(typeId);
            if (cmbHolderType.Items.Count > 0)
                cmbHolderType.SelectedIndex = 0;
            grpHolder.Controls.Add(cmbHolderType);
            this.Controls.Add(grpHolder);
            y += 62;

            // ── Trackers (2x2 grid, expandable to 4x2) ───────────
            int trackerRows = (int)Math.Ceiling((double)TrackerCount / TrackerCols);
            int cellW = 400;
            int cellH = 34;
            int grpTrackerH = 20 + trackerRows * cellH + 8;

            var grpTrackers = new GroupBox
            {
                Text = string.Format("Trackers (select up to {0}, Tracker 1 required)", TrackerCount),
                Left = lx,
                Top = y,
                Width = 806,
                Height = grpTrackerH,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            pickerTrackers = new TxObjEditBoxCtrl[TrackerCount];

            for (int i = 0; i < TrackerCount; i++)
            {
                int col = i % TrackerCols;
                int row = i / TrackerCols;
                int cx = 8 + col * cellW;
                int cy = 22 + row * cellH;

                grpTrackers.Controls.Add(new Label
                {
                    Text = string.Format("Tracker {0}:", i + 1),
                    Left = cx,
                    Top = cy + 2,
                    Width = 78,
                    Height = 20,
                    TextAlign = ContentAlignment.MiddleLeft
                });

                pickerTrackers[i] = new TxObjEditBoxCtrl
                {
                    Left = cx + 72,
                    Top = cy,
                    Width = 310,
                    Height = 24,
                    ValidatorType = TxValidatorType.Component,
                    PickLevel = TxPickLevel.Component,
                    PickOnly = false,
                    ListenToPick = true
                };
                grpTrackers.Controls.Add(pickerTrackers[i]);
            }

            this.Controls.Add(grpTrackers);
            y += grpTrackerH + 10;

            // ── Measurement Path ──────────────────────────────────
            var grpPaths = new GroupBox
            {
                Text = "Measurement Path",
                Left = lx,
                Top = y,
                Width = 806,
                Height = 100,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            lstPaths = new ListView
            {
                Left = 8,
                Top = 18,
                Width = 706,
                Height = 68,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Font = new Font("Consolas", 8),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            lstPaths.Columns.Add("Path Name", 690);
            grpPaths.Controls.Add(lstPaths);

            btnPickPaths = new Button
            {
                Text = "Pick",
                Left = 722,
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
                Left = 722,
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
            y += 110;

            // ── Collision Pair ────────────────────────────────────
            var grpCollision = new GroupBox
            {
                Text = "Collision Pair (from PS Collision Viewer)",
                Left = lx,
                Top = y,
                Width = 806,
                Height = 56,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            grpCollision.Controls.Add(new Label
            {
                Text = "Pair:",
                Left = 8,
                Top = 20,
                Width = 40,
                Height = 24,
                TextAlign = ContentAlignment.MiddleLeft
            });
            cmbCollisionPair = new ComboBox
            {
                Left = 52,
                Top = 20,
                Width = 620,
                Height = 24,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            cmbCollisionPair.Items.Add("(none — skip collision check)");
            cmbCollisionPair.SelectedIndex = 0;
            grpCollision.Controls.Add(cmbCollisionPair);

            var btnRefreshPairs = new Button
            {
                Text = "Refresh",
                Left = 680,
                Top = 18,
                Width = 74,
                Height = 26,
                BackColor = Color.FromArgb(80, 80, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnRefreshPairs.Click += OnRefreshPairsClick;
            grpCollision.Controls.Add(btnRefreshPairs);
            this.Controls.Add(grpCollision);
            y += 62;

            // ── Analyze button ────────────────────────────────────
            btnAnalyze = new Button
            {
                Text = "Analyze",
                Left = lx,
                Top = y,
                Width = 806,
                Height = 36,
                BackColor = Color.FromArgb(180, 0, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            btnAnalyze.Click += OnAnalyze;
            this.Controls.Add(btnAnalyze);
            y += 44;

            // ── Visualization filter ──────────────────────────────
            var pnlVisFilter = new Panel
            {
                Left = lx,
                Top = y,
                Width = 806,
                Height = 30,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            chkShowOkLines = new CheckBox
            {
                Text = "Show OK lines (green)",
                Left = 0,
                Top = 6,
                Width = 185,
                Height = 20,
                Checked = true
            };
            chkShowOkLines.CheckedChanged += OnVisFilterChanged;
            pnlVisFilter.Controls.Add(chkShowOkLines);

            chkShowNokLines = new CheckBox
            {
                Text = "Show NOK lines (red)",
                Left = 200,
                Top = 6,
                Width = 180,
                Height = 20,
                Checked = true
            };
            chkShowNokLines.CheckedChanged += OnVisFilterChanged;
            pnlVisFilter.Controls.Add(chkShowNokLines);

            chkShowFovLines = new CheckBox
            {
                Text = "Show FOV outside (gray)",
                Left = 400,
                Top = 6,
                Width = 200,
                Height = 20,
                Checked = false
            };
            chkShowFovLines.CheckedChanged += OnVisFilterChanged;
            pnlVisFilter.Controls.Add(chkShowFovLines);

            this.Controls.Add(pnlVisFilter);
            y += 32;

            // ── Results TabControl ────────────────────────────────
            var grpResults = new GroupBox
            {
                Text = "Results",
                Left = lx,
                Top = y,
                Width = 806,
                Height = this.Height - y - 50,
                Anchor = AnchorStyles.Top | AnchorStyles.Left |
                         AnchorStyles.Right | AnchorStyles.Bottom
            };

            tabResults = new TabControl
            {
                Dock = DockStyle.Fill
            };

            // Tab 1: Results
            var tabSummary = new TabPage { Text = "Results" };
            lstResults = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Font = new Font("Consolas", 8)
            };
            lstResults.Columns.Add("Location", 160);
            lstResults.Columns.Add("Status", 70);
            lstResults.Columns.Add("Tracker", 70);
            lstResults.Columns.Add("Plane", 55);
            lstResults.Columns.Add("Visible LEDs", 90);
            lstResults.Columns.Add("Details", 200);
            lstResults.Columns.Add("FOV", 50);
            lstResults.Columns.Add("Sight", 55);
            lstResults.MouseClick += OnResultsMouseClick;
            tabSummary.Controls.Add(lstResults);

            // Tab 2: Angle Details
            var tabAngles = new TabPage { Text = "Angle Details" };
            treeAngleDetails = new TreeView
            {
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 8),
                ShowLines = true,
                ShowPlusMinus = true,
                FullRowSelect = true
            };
            tabAngles.Controls.Add(treeAngleDetails);


            tabResults.TabPages.Add(tabSummary);
            tabResults.TabPages.Add(tabAngles);
            grpResults.Controls.Add(tabResults);
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
                    AddPathsFromSelection(compound.GetAllDescendants(
                        new TxTypeFilter(typeof(TxWeldOperation))));
            }
        }

        // ── Collision Pair refresh ────────────────────────────────

        private void OnRefreshPairsClick(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            try
            {
                cmbCollisionPair.Items.Clear();
                cmbCollisionPair.Items.Add("(none — skip collision check)");

                var pairs = CollisionPairReader.GetActivePairs();
                foreach (var pair in pairs)
                    cmbCollisionPair.Items.Add(pair);

                cmbCollisionPair.DisplayMember = "Name";
                cmbCollisionPair.SelectedIndex = 0;

                if (pairs.Count == 0)
                    MessageBox.Show(
                        "No active collision pairs found in PS Collision Viewer.",
                        "No Pairs", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        // ── Analyze ───────────────────────────────────────────────

        private void OnAnalyze(object sender, EventArgs e)
        {
            // Validate robot
            var robot = pickerRobot.Object as TxRobot;
            if (robot == null)
            {
                MessageBox.Show("Please select a Robot.", "Missing Input",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            // Validate sensor holder
            string selectedTypeId = cmbHolderType.SelectedItem as string;
            if (string.IsNullOrEmpty(selectedTypeId))
            {
                MessageBox.Show("Please select a Sensor Holder type.", "Missing Input",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Collect trackers (at least one required)
            var trackers = new List<ITxLocatableObject>();
            for (int i = 0; i < TrackerCount; i++)
            {
                var t = pickerTrackers[i].Object as ITxLocatableObject;
                if (t != null) trackers.Add(t);
            }

            if (trackers.Count == 0)
            {
                MessageBox.Show("Please select at least one Tracker.", "Missing Input",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_paths.Count == 0)
            {
                MessageBox.Show("Please select a path.", "Missing Input",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Get selected collision pair (null = skip)
            var selectedPair = cmbCollisionPair.SelectedItem as TxCollisionPair;

            // Cleanup previous results
            ConstellationVisibilityChecker.DeleteVisualizations(_visComponents);
            ConstellationVisibilityChecker.DeleteVisualizations(_currentPointVis);
            lstResults.Items.Clear();
            treeAngleDetails.Nodes.Clear();
            _pointVisibility.Clear();
            _pointTrackerLabel.Clear();

            ITracker trackerDef = new Tracker920_0005();
            var followMode = new TxOlpRobotFollowMode(robot);

            foreach (var path in _paths)
            {
                TxObjectList locations = path.GetAllDescendants(
                    new TxTypeFilter(typeof(ITxRoboticLocationOperation)));

                foreach (ITxObject obj in locations)
                {
                    var loc = obj as ITxRoboticLocationOperation;
                    if (loc == null) continue;

                    // 1. Jump
                    bool reached = followMode.JumpRobotToLocation(loc);
                    TxApplication.RefreshDisplay();

                    if (!reached)
                    {
                        AddResultRow(loc.Name, "SKIPPED", "", "", "",
                            "Robot could not reach location", "", "", Color.Gray);
                        continue;
                    }

                    // 2. Collision check via PS Collision Pair
                    if (selectedPair != null)
                    {
                        bool hasCollision =
                            CollisionPairReader.CheckCollisionWithPair(selectedPair);
                        if (hasCollision)
                        {
                            AddResultRow(loc.Name, "COLLISION", "", "", "",
                                "Collision detected", "", "", Color.OrangeRed);
                            continue;
                        }
                    }

                    // 3. Sensor holder — position from robot TCPF (holder self origin = flansch)
         
                    ISensorHolder holder = CreateHolderInstance(selectedTypeId);

                    if (holder == null || robot.TCPF == null)
                    {
                        AddResultRow(loc.Name, "SKIPPED", "", "", "",
                            "No recognized sensor holder mounted", "", "", Color.Gray);
                        continue;
                    }

                    // holderLoc = Toolframe — holder self origin coincides with robot flange
                    ITxLocatableObject holderLoc = robot.Toolframe;

                    // 4. Try each tracker — first OK wins
                    bool anyOk = false;

                    for (int t = 0; t < trackers.Count; t++)
                    {
                        TxTransformation trackerWorld = trackers[t].AbsoluteLocation;

                        // Üres temp lista — nem jelenítünk meg semmit analízis közben
                        var tempVis = new List<TxComponent>();

                        var visibility = ConstellationVisibilityChecker.Check(
                            holderLoc, holder, trackerWorld, trackerDef,
                            tempVis);

                        // Tárold az eredményt pontonként
                        _pointVisibility[loc.Name] = visibility;
                        _pointTrackerLabel[loc.Name] = string.Format("T{0}", t + 1);

                        // Fill Angle Details tab — egy főnode per pont, alatta emitterek (táblázatos formátum)
                        TreeNode pointNode = null;

                        // Keresd meg vagy hozd létre a pont node-ját
                        foreach (TreeNode n in treeAngleDetails.Nodes)
                        {
                            if (n.Text == loc.Name) { pointNode = n; break; }
                        }
                        if (pointNode == null)
                        {
                            pointNode = new TreeNode(loc.Name);
                            treeAngleDetails.Nodes.Add(pointNode);

                            var headerNode = new TreeNode(FormatAngleDetailRow(
                                "Group", "Emitter", "Cam1", "Cam2", "Cam3", "FOV", "Status"));
                            headerNode.ForeColor = Color.DimGray;
                            headerNode.NodeFont = new Font(treeAngleDetails.Font, FontStyle.Bold);
                            pointNode.Nodes.Add(headerNode);
                        }

                        var emitters = holder.GetEmitters();
                        for (int ei = 0; ei < emitters.Length; ei++)
                        {
                            if (visibility.AngleResults == null) continue;

                            double a1 = visibility.AngleResults[ei, 0].AngleDeg;
                            double a2 = visibility.AngleResults[ei, 1].AngleDeg;
                            double a3 = visibility.AngleResults[ei, 2].AngleDeg;

                            bool inFov = !double.IsNaN(a1);
                            bool allOk = inFov &&
                                         visibility.AngleResults[ei, 0].PassedAngle &&
                                         visibility.AngleResults[ei, 1].PassedAngle &&
                                         visibility.AngleResults[ei, 2].PassedAngle;

                            string fmt1 = double.IsNaN(a1) ? "-" : string.Format("{0:F1}", a1);
                            string fmt2 = double.IsNaN(a2) ? "-" : string.Format("{0:F1}", a2);
                            string fmt3 = double.IsNaN(a3) ? "-" : string.Format("{0:F1}", a3);

                            string nodeText = FormatAngleDetailRow(
                                emitters[ei].Group,
                                emitters[ei].Name,
                                fmt1, fmt2, fmt3,
                                inFov ? "YES" : "NO",
                                allOk ? "OK" : (inFov ? "NOK" : "FOV"));

                            var emitterNode = new TreeNode(nodeText);
                            emitterNode.ForeColor = allOk ? Color.DarkGreen :
                                                    !inFov ? Color.Gray :
                                                             Color.DarkRed;
                            pointNode.Nodes.Add(emitterNode);
                        }

                        // Evaluate criteria
                        var criteria = ConstellationCriteriaEngine.Evaluate(visibility);
                        string trackerLabel = string.Format("T{0}", t + 1);

                        if (criteria.IsOk)
                        {
                            string fov = visibility.IsInFov ? "YES" : "NO";
                            string sight = visibility.IsSightBlocked == null ? "N/A" :
                                           visibility.IsSightBlocked == true ? "NOK" : "OK";

                            AddResultRow(
                                loc.Name, "OK", trackerLabel,
                                criteria.SatisfiedPlane ?? "",
                                visibility.TotalVisibleCount.ToString(),
                                criteria.Label,
                                fov, sight,
                                Color.DarkGreen);
                            pointNode.ForeColor = Color.DarkGreen;
                            anyOk = true;
                            break;
                        }

                        if (t == trackers.Count - 1)
                        {
                            string fov = visibility.IsInFov ? "YES" : "NO";
                            string sight = visibility.IsSightBlocked == null ? "N/A" :
                                           visibility.IsSightBlocked == true ? "NOK" : "OK";

                            AddResultRow(
                                loc.Name, "NOK", trackerLabel, "",
                                visibility.TotalVisibleCount.ToString(),
                                "No tracker satisfies criteria",
                                fov, sight,
                                Color.DarkRed);
                            pointNode.ForeColor = Color.DarkRed;
                        }
                    }
                }
            }

            // Summary
            int ok = 0, nok = 0, coll = 0, skip = 0;
            foreach (ListViewItem item in lstResults.Items)
            {
                switch (item.SubItems[1].Text)
                {
                    case "OK": ok++; break;
                    case "NOK": nok++; break;
                    case "COLLISION": coll++; break;
                    case "SKIPPED": skip++; break;
                }
            }

            MessageBox.Show(
                string.Format("Done.\nOK: {0}  NOK: {1}  Collision: {2}  Skipped: {3}",
                    ok, nok, coll, skip),
                "Analysis Complete",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ── Jump to location on row click ─────────────────────────

        private void OnResultsMouseClick(object sender, MouseEventArgs e)
        {
            var hit = lstResults.HitTest(e.Location);
            var item = hit.Item;
            if (item == null) return;

            string locationName = item.SubItems[0].Text;
            if (string.IsNullOrEmpty(locationName)) return;

            var robot = pickerRobot.Object as TxRobot;
            if (robot == null) return;

            // 1. Töröld az előző pont vizualizációját
            ConstellationVisibilityChecker.DeleteVisualizations(_currentPointVis);

            // 2. Ugorj a pontba
            var followMode = new TxOlpRobotFollowMode(robot);
            foreach (var path in _paths)
            {
                TxObjectList locations = path.GetAllDescendants(
                    new TxTypeFilter(typeof(ITxRoboticLocationOperation)));

                foreach (ITxObject obj in locations)
                {
                    var loc = obj as ITxRoboticLocationOperation;
                    if (loc == null || loc.Name != locationName) continue;

                    followMode.JumpRobotToLocation(loc);

                    var sel = new TxObjectList();
                    sel.Add(loc);
                    TxApplication.ActiveSelection.SetItems(sel);
                    TxApplication.RefreshDisplay();
                    goto pointFound;
                }
            }
        pointFound:

            // Tárold a jelenlegi pont nevét és rajzold újra a szűrőkkel
            _currentPointName = locationName;
            RedrawCurrentPointVisualization();
        }

        // ── Helpers ───────────────────────────────────────────────

        private void AddResultRow(
            string location, string status, string tracker,
            string plane, string visLeds, string details,
            string fov, string sight, Color color)
        {
                    var item = new ListViewItem(new[]
                    {
                         location, status, tracker, plane, visLeds, details, fov, sight
                    });
                    item.ForeColor = color;
                    lstResults.Items.Add(item);
        }

        // Fixed-width columns so rows line up under the Consolas font used by treeAngleDetails.
        private static string FormatAngleDetailRow(
            string group, string emitter, string cam1, string cam2, string cam3,
            string fov, string status)
        {
            return string.Format(
                "{0,-8} {1,-14} {2,7} {3,7} {4,7}   {5,-5} {6,-6}",
                Truncate(group, 8), Truncate(emitter, 14), cam1, cam2, cam3, fov, status);
        }

        private static string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value) || value.Length <= maxLength) return value;
            return value.Substring(0, maxLength);
        }

        private static ISensorHolder CreateHolderInstance(string typeId)
        {
            if (string.IsNullOrEmpty(typeId)) return null;
            if (typeId == "perc_01-03944-10")
                return new SensorHolder_Perc_01_03944_10();
            return null;
        }
        private void OnVisFilterChanged(object sender, EventArgs e)
        {
            // Ha van kiválasztott pont, újrarajzoljuk a vonalakat
            if (_currentPointName == null) return;
            RedrawCurrentPointVisualization();
        }
        private void RedrawCurrentPointVisualization()
        {
            if (_currentPointName == null) return;
            if (!_pointVisibility.ContainsKey(_currentPointName)) return;

            // ← robot a pickerből
            var robot = pickerRobot.Object as TxRobot;
            if (robot == null) return;

            ConstellationVisibilityChecker.DeleteVisualizations(_currentPointVis);

            var visibility = _pointVisibility[_currentPointName];
            string selectedTypeId = cmbHolderType.SelectedItem as string;
            ISensorHolder holder = CreateHolderInstance(selectedTypeId);
            if (holder == null || robot.Toolframe == null) return;

            string trackerLabel = _pointTrackerLabel.ContainsKey(_currentPointName)
                ? _pointTrackerLabel[_currentPointName] : "T1";
            int trackerIdx = 0;
            if (trackerLabel.Length > 1)
                int.TryParse(trackerLabel.Substring(1), out trackerIdx);
            trackerIdx = Math.Max(1, trackerIdx) - 1;

            var trackerList = new List<ITxLocatableObject>();
            for (int i = 0; i < TrackerCount; i++)
            {
                var t = pickerTrackers[i].Object as ITxLocatableObject;
                if (t != null) trackerList.Add(t);
            }
            if (trackerIdx >= trackerList.Count) trackerIdx = 0;

            TxTransformation trackerWorld = trackerList[trackerIdx].AbsoluteLocation;
            ITracker trackerDef = new Tracker920_0005();
            ITxLocatableObject holderLoc = robot.Toolframe;

            foreach (var vis in visibility.VisibleEmitters)
                ConstellationVisibilityChecker.CreateLedSquare(
                    vis.WorldPos, vis.WorldZVec, _currentPointVis);

            ConstellationVisibilityChecker.CreateAngleVisualizationFiltered(
                holderLoc, holder, trackerWorld, trackerDef,
                visibility.AngleResults, _currentPointVis,
                chkShowOkLines.Checked,
                chkShowNokLines.Checked,
                chkShowFovLines.Checked);

            TxApplication.RefreshDisplay();
        }

        // ── Cleanup ───────────────────────────────────────────────

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                TxApplication.ActiveSelection.ItemsSet -= OnSelectionChanged;
                TxApplication.ActiveSelection.ItemsAdded -= OnSelectionAdded;
                ConstellationVisibilityChecker.DeleteVisualizations(_visComponents);
                ConstellationVisibilityChecker.DeleteVisualizations(_currentPointVis);
                _currentPointName = null;
            }
            catch { }
        }
    }
}