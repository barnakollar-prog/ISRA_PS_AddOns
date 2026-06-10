using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Tecnomatix.Engineering;
using Tecnomatix.Engineering.Ui;
using ISRA.Calculations.TempComp;

namespace TempCompAddon
{
    public class TempCompCommand : TxButtonCommand
    {
        public override string Category { get { return StringTable.CATEGORY; } }
        public override string Name { get { return StringTable.NAME; } }

        public override string Bitmap
        {
            get { return "temp_comp_add_on_16x16.bmp"; }
        }

        public override string LargeBitmap
        {
            get { return "temp_comp_add_on_32x32.png"; }
        }

        public override void Execute(object cmdParams)
        {
            var form = new TempCompForm();
            form.Show();
        }
    }

    public class TempCompForm : TxForm
    {
        // ── Controls ──────────────────────────────────────────────
        private TxObjEditBoxCtrl pickerRobot;
        private ListView lstBodyPaths;
        private ListView lstTempCompPaths;
        private Button btnPickBodyPaths;
        private Button btnClearBodyPaths;
        private Button btnPickTempCompPaths;
        private Button btnClearTempCompPaths;
        private NumericUpDown nudStepSize;
        private Button btnAnalyze;
        private RadioButton rbFanuc;
        private RadioButton rbKuka;
        private RadioButton rbAbb;

        // Tab results
        private ListView lstValidation;
        private ListView lstNearestTc;
        private ListView lstRawData;

        private readonly List<TxWeldOperation> _bodyPrograms
            = new List<TxWeldOperation>();
        private readonly List<TxWeldOperation> _tempCompPrograms
            = new List<TxWeldOperation>();

        private enum PickMode { None, Body, TempComp }
        private PickMode _pickMode = PickMode.None;

        public TempCompForm()
        {
            this.SemiModal = false;
            this.ShouldAutoPosition = true;
            this.ShouldCloseOnDocumentUnloading = true;
            this.FormClosing += OnFormClosing;
            BuildUI();
            TxApplication.ActiveSelection.ItemsSet += OnSelectionChanged;
        }

        private void BuildUI()
        {
            this.Text = "Temp Comp Validator";
            this.Width = 800;
            this.Height = 900;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimumSize = new Size(600, 700);

            int lx = 10;
            int y = 15;

            // ── Robot ─────────────────────────────────────────────
            var grpRobot = new GroupBox
            {
                Text = "Robot",
                Left = lx,
                Top = y,
                Width = 765,
                Height = 82,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            grpRobot.Controls.Add(new Label
            {
                Text = "Robot:",
                Left = 8,
                Top = 20,
                Width = 70,
                Height = 24,
                TextAlign = ContentAlignment.MiddleLeft
            });
            pickerRobot = new TxObjEditBoxCtrl
            {
                Left = 82,
                Top = 20,
                Width = 668,
                Height = 24,
                ValidatorType = TxValidatorType.Robot,
                PickLevel = TxPickLevel.Component,
                PickOnly = false,
                ListenToPick = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            pickerRobot.Picked += OnRobotPicked;
            grpRobot.Controls.Add(pickerRobot);

            rbFanuc = new RadioButton { Text = "Fanuc", Left = 82, Top = 50, Width = 80, Height = 20, Checked = true };
            rbKuka = new RadioButton { Text = "Kuka", Left = 170, Top = 50, Width = 80, Height = 20 };
            rbAbb = new RadioButton { Text = "ABB", Left = 255, Top = 50, Width = 80, Height = 20 };
            grpRobot.Controls.Add(rbFanuc);
            grpRobot.Controls.Add(rbKuka);
            grpRobot.Controls.Add(rbAbb);

            this.Controls.Add(grpRobot);
            y += 92;

            // ── Bodypart paths ────────────────────────────────────
            var grpBody = new GroupBox
            {
                Text = "Bodypart Measurement Paths",
                Left = lx,
                Top = y,
                Width = 765,
                Height = 130,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            lstBodyPaths = new ListView
            {
                Left = 8,
                Top = 18,
                Width = 660,
                Height = 95,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Font = new Font("Consolas", 8),
                Anchor = AnchorStyles.Top | AnchorStyles.Left |
                         AnchorStyles.Right | AnchorStyles.Bottom
            };
            lstBodyPaths.Columns.Add("Path Name", 640);
            grpBody.Controls.Add(lstBodyPaths);

            btnPickBodyPaths = new Button
            {
                Text = "Pick",
                Left = 676,
                Top = 18,
                Width = 80,
                Height = 28,
                BackColor = Color.FromArgb(0, 100, 180),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnPickBodyPaths.Click += (s, e) => StartPicking(PickMode.Body);
            grpBody.Controls.Add(btnPickBodyPaths);

            btnClearBodyPaths = new Button
            {
                Text = "Clear",
                Left = 676,
                Top = 50,
                Width = 80,
                Height = 28,
                BackColor = Color.FromArgb(100, 100, 100),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnClearBodyPaths.Click += (s, e) =>
            {
                _bodyPrograms.Clear();
                lstBodyPaths.Items.Clear();
            };
            grpBody.Controls.Add(btnClearBodyPaths);
            this.Controls.Add(grpBody);
            y += 140;

            // ── Temp Comp paths ───────────────────────────────────
            var grpTempComp = new GroupBox
            {
                Text = "Temp Comp Paths",
                Left = lx,
                Top = y,
                Width = 765,
                Height = 130,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            lstTempCompPaths = new ListView
            {
                Left = 8,
                Top = 18,
                Width = 660,
                Height = 95,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Font = new Font("Consolas", 8),
                Anchor = AnchorStyles.Top | AnchorStyles.Left |
                         AnchorStyles.Right | AnchorStyles.Bottom
            };
            lstTempCompPaths.Columns.Add("Path Name", 640);
            grpTempComp.Controls.Add(lstTempCompPaths);

            btnPickTempCompPaths = new Button
            {
                Text = "Pick",
                Left = 676,
                Top = 18,
                Width = 80,
                Height = 28,
                BackColor = Color.FromArgb(0, 100, 180),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnPickTempCompPaths.Click += (s, e) => StartPicking(PickMode.TempComp);
            grpTempComp.Controls.Add(btnPickTempCompPaths);

            btnClearTempCompPaths = new Button
            {
                Text = "Clear",
                Left = 676,
                Top = 50,
                Width = 80,
                Height = 28,
                BackColor = Color.FromArgb(100, 100, 100),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnClearTempCompPaths.Click += (s, e) =>
            {
                _tempCompPrograms.Clear();
                lstTempCompPaths.Items.Clear();
            };
            grpTempComp.Controls.Add(btnClearTempCompPaths);
            this.Controls.Add(grpTempComp);
            y += 140;

            // ── Settings ──────────────────────────────────────────
            var grpSettings = new GroupBox
            {
                Text = "Settings",
                Left = lx,
                Top = y,
                Width = 765,
                Height = 48,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            grpSettings.Controls.Add(new Label
            {
                Text = "Step size J2/J3 (deg):",
                Left = 8,
                Top = 14,
                Width = 150,
                Height = 24,
                TextAlign = ContentAlignment.MiddleLeft
            });
            nudStepSize = new NumericUpDown
            {
                Left = 162,
                Top = 14,
                Width = 70,
                Height = 24,
                Minimum = 5,
                Maximum = 90,
                Value = 30,
                DecimalPlaces = 0
            };
            grpSettings.Controls.Add(nudStepSize);
            this.Controls.Add(grpSettings);
            y += 58;

            // ── Analyze button ────────────────────────────────────
            btnAnalyze = new Button
            {
                Text = "Analyze",
                Left = lx,
                Top = y,
                Width = 765,
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

            // ── Tab Control ───────────────────────────────────────
            var tabControl = new TabControl
            {
                Left = lx,
                Top = y,
                Width = 765,
                Height = 380,
                Anchor = AnchorStyles.Top | AnchorStyles.Left |
                         AnchorStyles.Right | AnchorStyles.Bottom
            };

            // Tab 1: Validation
            var tabValidation = new TabPage { Text = "Validation" };
            lstValidation = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Font = new Font("Consolas", 9)
            };
            lstValidation.Columns.Add("Criterion", 180);
            lstValidation.Columns.Add("Bodypart", 120);
            lstValidation.Columns.Add("Temp Comp", 180);
            lstValidation.Columns.Add("Details", 180);
            lstValidation.Columns.Add("Status", 70);
            tabValidation.Controls.Add(lstValidation);

            // Tab 2: Nearest TC
            var tabNearest = new TabPage { Text = "Nearest TC Point" };
            lstNearestTc = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Font = new Font("Consolas", 8)
            };
            lstNearestTc.Columns.Add("Body Point", 120);
            lstNearestTc.Columns.Add("J2", 60);
            lstNearestTc.Columns.Add("J3", 60);
            lstNearestTc.Columns.Add("J4", 60);
            lstNearestTc.Columns.Add("J5", 60);
            lstNearestTc.Columns.Add("TC Point", 120);
            lstNearestTc.Columns.Add("TC J2", 60);
            lstNearestTc.Columns.Add("TC J3", 60);
            lstNearestTc.Columns.Add("TC J4", 60);
            lstNearestTc.Columns.Add("TC J5", 60);
            lstNearestTc.Columns.Add("Dist", 60);
            tabNearest.Controls.Add(lstNearestTc);

            // Tab 3: Raw Data
            var tabRaw = new TabPage { Text = "Raw Data" };
            lstRawData = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Font = new Font("Consolas", 8)
            };
            lstRawData.Columns.Add("Body Point", 110);
            lstRawData.Columns.Add("J1", 50);
            lstRawData.Columns.Add("J2", 50);
            lstRawData.Columns.Add("J3", 50);
            lstRawData.Columns.Add("J4", 50);
            lstRawData.Columns.Add("J5", 50);
            lstRawData.Columns.Add("J6", 50);
            lstRawData.Columns.Add("TC Point", 110);
            lstRawData.Columns.Add("TC J1", 50);
            lstRawData.Columns.Add("TC J2", 50);
            lstRawData.Columns.Add("TC J3", 50);
            lstRawData.Columns.Add("TC J4", 50);
            lstRawData.Columns.Add("TC J5", 50);
            lstRawData.Columns.Add("TC J6", 50);
            tabRaw.Controls.Add(lstRawData);

            tabControl.TabPages.Add(tabValidation);
            tabControl.TabPages.Add(tabNearest);
            tabControl.TabPages.Add(tabRaw);
            this.Controls.Add(tabControl);
        }

        // ── Pick from PS ──────────────────────────────────────────
        private void StartPicking(PickMode mode)
        {
            _pickMode = mode;

            btnPickBodyPaths.BackColor = mode == PickMode.Body
                ? Color.FromArgb(0, 160, 0) : Color.FromArgb(0, 100, 180);
            btnPickTempCompPaths.BackColor = mode == PickMode.TempComp
                ? Color.FromArgb(0, 160, 0) : Color.FromArgb(0, 100, 180);

            MessageBox.Show(
                string.Format("Select {0} paths in PS, then click OK.",
                    mode == PickMode.Body ? "Bodypart" : "Temp Comp"),
                "Pick Paths", MessageBoxButtons.OK, MessageBoxIcon.Information);

            AddFromSelection(TxApplication.ActiveSelection.GetItems());

            _pickMode = PickMode.None;
            btnPickBodyPaths.BackColor = Color.FromArgb(0, 100, 180);
            btnPickTempCompPaths.BackColor = Color.FromArgb(0, 100, 180);
        }

        private void OnSelectionChanged(object sender, TxSelection_ItemsSetEventArgs e)
        {
            if (_pickMode == PickMode.None) return;
            AddFromSelection(TxApplication.ActiveSelection.GetItems());
        }

        private void AddFromSelection(TxObjectList items)
        {
            foreach (ITxObject obj in items)
            {
                var prog = obj as TxWeldOperation;
                if (prog == null) continue;

                if (_pickMode == PickMode.Body)
                {
                    if (!_bodyPrograms.Contains(prog))
                    {
                        _bodyPrograms.Add(prog);
                        lstBodyPaths.Items.Add(new ListViewItem(prog.Name));
                    }
                }
                else if (_pickMode == PickMode.TempComp)
                {
                    if (!_tempCompPrograms.Contains(prog))
                    {
                        _tempCompPrograms.Add(prog);
                        lstTempCompPaths.Items.Add(new ListViewItem(prog.Name));
                    }
                }
            }
        }

        // ── Analyze ───────────────────────────────────────────────
        private void OnAnalyze(object sender, EventArgs e)
        {
            if (_bodyPrograms.Count == 0)
            {
                MessageBox.Show("Please select at least one Bodypart Path.",
                    "Missing Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (_tempCompPrograms.Count == 0)
            {
                MessageBox.Show("Please select at least one Temp Comp Path.",
                    "Missing Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var robot = pickerRobot.Object as TxRobot;
            if (robot == null)
            {
                MessageBox.Show("Please select a Robot.", "Missing Input",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Robot type
            TempCompCalculations.RobotType robotType = TempCompCalculations.RobotType.Fanuc;
            if (rbKuka.Checked) robotType = TempCompCalculations.RobotType.Kuka;
            else if (rbAbb.Checked) robotType = TempCompCalculations.RobotType.Abb;

            // Read poses
            var bodyPoses = new List<TempCompCalculations.RobotPose>();
            foreach (var prog in _bodyPrograms)
                bodyPoses.AddRange(TempCompCalculations.ReadPosesFromProgram(prog, robot));

            var tempCompPoses = new List<TempCompCalculations.RobotPose>();
            foreach (var prog in _tempCompPrograms)
                tempCompPoses.AddRange(TempCompCalculations.ReadPosesFromProgram(prog, robot));

            if (bodyPoses.Count == 0)
            {
                MessageBox.Show("No poses found in Bodypart Paths.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (tempCompPoses.Count == 0)
            {
                MessageBox.Show("No poses found in Temp Comp Paths.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            double stepSize = (double)nudStepSize.Value;

            // Run criteria
            var j23 = TempCompCalculations.CheckJ23AngleCoverage(bodyPoses, tempCompPoses, robotType);
            var c2 = TempCompCalculations.CheckJ2J3Spread(tempCompPoses);
            var c3 = TempCompCalculations.CheckJ5Symmetry(tempCompPoses);
            var j4 = TempCompCalculations.CheckAxisMaxCoverage(bodyPoses, tempCompPoses, p => p.J4);
            var j5 = TempCompCalculations.CheckAxisMaxCoverage(bodyPoses, tempCompPoses, p => p.J5);
            var j6 = TempCompCalculations.CheckAxisMaxCoverage(bodyPoses, tempCompPoses, p => p.J6);

            double tcJ23Range = TempCompCalculations.CalculateJ23Range(tempCompPoses, robotType);
            bool j23RangeOK = tcJ23Range >= 75.0;

            var bodySum = TempCompCalculations.CalculateSummary(bodyPoses);
            var tcSum = TempCompCalculations.CalculateSummary(tempCompPoses);




            // ── Tab 1: Validation ─────────────────────────────────
            lstValidation.Items.Clear();

            AddValidationRow("J2-J3 Angle Max.",
                string.Format("{0:F2} deg", j23.BodyMax),
                string.Format("{0} TC pts >= body max", j23.CountMax),
                "Min 2 TC pts >= body max",
                j23.MaxOK);

            AddValidationRow("J2-J3 Angle Min.",
                string.Format("{0:F2} deg", j23.BodyMin),
                string.Format("{0} TC pts <= body min", j23.CountMin),
                "Min 2 TC pts <= body min",
                j23.MinOK);

            AddValidationRow("J2-3 Range > 75°",
                string.Format("{0:F2} deg", TempCompCalculations.CalculateJ23Range(bodyPoses, robotType)),
                string.Format("{0:F2} deg", tcJ23Range),
                "TC range >= 75 deg",
                j23RangeOK);

            AddValidationRow("J5 Symmetry",
                "",
                string.Format("Neg: {0} / Pos: {1}", c3.NegCount, c3.PosCount),
                string.Format("Total: {0} pts", c3.Total),
                c3.IsValid);

            AddValidationRow("J4 Max",
                string.Format("{0:F2} deg", j4.BodyMax),
                string.Format("{0:F2} deg", j4.TcMax),
                "TC max >= body max",
                j4.IsValid);

            AddValidationRow("J5 Max",
                string.Format("{0:F2} deg", j5.BodyMax),
                string.Format("{0:F2} deg", j5.TcMax),
                "TC max >= body max",
                j5.IsValid);

            AddValidationRow("J6 Max",
                string.Format("{0:F2} deg", j6.BodyMax),
                string.Format("{0:F2} deg", j6.TcMax),
                "TC max >= body max",
                j6.IsValid);

            // Summary separator
            var sep = new ListViewItem("--- Summary ---");
            for (int i = 0; i < 4; i++) sep.SubItems.Add("");
            sep.BackColor = Color.LightGray;
            lstValidation.Items.Add(sep);

            AddSummaryRow("Body J2", bodySum.J2_Min, bodySum.J2_Max);
            AddSummaryRow("Body J3", bodySum.J3_Min, bodySum.J3_Max);
            AddSummaryRow("TC J2", tcSum.J2_Min, tcSum.J2_Max);
            AddSummaryRow("TC J3", tcSum.J3_Min, tcSum.J3_Max);
            AddSummaryRow("TC J4", tcSum.J4_Min, tcSum.J4_Max);
            AddSummaryRow("TC J5", tcSum.J5_Min, tcSum.J5_Max);
            AddSummaryRow("TC J6", tcSum.J6_Min, tcSum.J6_Max);

            foreach (ColumnHeader col in lstValidation.Columns)
                col.Width = -2;

            // ── Tab 2: Nearest TC ─────────────────────────────────
            lstNearestTc.Items.Clear();
            var nearest = TempCompCalculations.FindNearestTcPoints(bodyPoses, tempCompPoses);
            foreach (var r in nearest)
            {
                var item = new ListViewItem(r.BodyPose.Name);
                item.SubItems.Add(string.Format("{0:F2}", r.BodyPose.J2));
                item.SubItems.Add(string.Format("{0:F2}", r.BodyPose.J3));
                item.SubItems.Add(string.Format("{0:F2}", r.BodyPose.J4));
                item.SubItems.Add(string.Format("{0:F2}", r.BodyPose.J5));
                item.SubItems.Add(r.NearestTcPose != null ? r.NearestTcPose.Name : "N/A");
                item.SubItems.Add(r.NearestTcPose != null ? string.Format("{0:F2}", r.NearestTcPose.J2) : "-");
                item.SubItems.Add(r.NearestTcPose != null ? string.Format("{0:F2}", r.NearestTcPose.J3) : "-");
                item.SubItems.Add(r.NearestTcPose != null ? string.Format("{0:F2}", r.NearestTcPose.J4) : "-");
                item.SubItems.Add(r.NearestTcPose != null ? string.Format("{0:F2}", r.NearestTcPose.J5) : "-");
                item.SubItems.Add(string.Format("{0:F2}", r.Distance));
                item.BackColor = Color.White;
                lstNearestTc.Items.Add(item);
            }
            foreach (ColumnHeader col in lstNearestTc.Columns)
                col.Width = -2;

            // ── Tab 3: Raw Data ───────────────────────────────────
            lstRawData.Items.Clear();
            int maxRows = Math.Max(bodyPoses.Count, tempCompPoses.Count);
            for (int i = 0; i < maxRows; i++)
            {
                var body = i < bodyPoses.Count ? bodyPoses[i] : null;
                var tc = i < tempCompPoses.Count ? tempCompPoses[i] : null;

                var item = new ListViewItem(body != null ? body.Name : "");
                item.SubItems.Add(body != null ? string.Format("{0:F2}", body.J1) : "");
                item.SubItems.Add(body != null ? string.Format("{0:F2}", body.J2) : "");
                item.SubItems.Add(body != null ? string.Format("{0:F2}", body.J3) : "");
                item.SubItems.Add(body != null ? string.Format("{0:F2}", body.J4) : "");
                item.SubItems.Add(body != null ? string.Format("{0:F2}", body.J5) : "");
                item.SubItems.Add(body != null ? string.Format("{0:F2}", body.J6) : "");
                item.SubItems.Add(tc != null ? tc.Name : "");
                item.SubItems.Add(tc != null ? string.Format("{0:F2}", tc.J1) : "");
                item.SubItems.Add(tc != null ? string.Format("{0:F2}", tc.J2) : "");
                item.SubItems.Add(tc != null ? string.Format("{0:F2}", tc.J3) : "");
                item.SubItems.Add(tc != null ? string.Format("{0:F2}", tc.J4) : "");
                item.SubItems.Add(tc != null ? string.Format("{0:F2}", tc.J5) : "");
                item.SubItems.Add(tc != null ? string.Format("{0:F2}", tc.J6) : "");
                item.BackColor = i % 2 == 0 ? Color.White : Color.FromArgb(245, 245, 245);
                lstRawData.Items.Add(item);
            }
            foreach (ColumnHeader col in lstRawData.Columns)
                col.Width = -2;
        }

        // ── Helpers ───────────────────────────────────────────────
        private void AddValidationRow(string criterion,
            string j2val, string j3val, string details, bool ok)
        {
            var item = new ListViewItem(criterion);
            item.SubItems.Add(j2val);
            item.SubItems.Add(j3val);
            item.SubItems.Add(details);
            item.SubItems.Add(ok ? "OK" : "NOK");
            item.BackColor = ok ? Color.LightGreen : Color.LightCoral;
            lstValidation.Items.Add(item);
        }

        private void AddSummaryRow(string label, double min, double max)
        {
            var item = new ListViewItem(label);
            item.SubItems.Add(string.Format("Min: {0:F2} deg", min));
            item.SubItems.Add(string.Format("Max: {0:F2} deg", max));
            item.SubItems.Add("");
            item.SubItems.Add("");
            item.BackColor = Color.FromArgb(245, 245, 245);
            lstValidation.Items.Add(item);
        }
        private void OnRobotPicked(object sender, TxObjEditBoxCtrl_PickedEventArgs e)
        {
            var robot = e.Object as TxRobot;
            if (robot == null) return;

            string name = robot.Name.ToLower();

            if (name.Contains("kuka"))
                rbKuka.Checked = true;
            else if (name.Contains("fanuc"))
                rbFanuc.Checked = true;
            else if (name.Contains("abb"))
                rbAbb.Checked = true;
        }
        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                TxApplication.ActiveSelection.ItemsSet -= OnSelectionChanged;
            }
            catch { }
        }
    }
}