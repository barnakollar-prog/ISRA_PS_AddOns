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

        public override void Execute(object cmdParams)
        {
            var form = new TempCompForm();
            form.Show();
        }
    }

    public class TempCompForm : TxForm
    {
        // ── Controls ──────────────────────────────────────────────

        private ListView lstBodyPaths;
        private ListView lstTempCompPaths;
        private Button btnPickBodyPaths;
        private Button btnClearBodyPaths;
        private Button btnPickTempCompPaths;
        private Button btnClearTempCompPaths;
        private NumericUpDown nudStepSize;
        private Button btnAnalyze;
        private ListView lstResults;

        private readonly List<TxWeldOperation> _bodyPrograms = new List<TxWeldOperation>();
        private readonly List<TxWeldOperation> _tempCompPrograms = new List<TxWeldOperation>();

        // Track which list is currently being picked
        private enum PickMode { None, Body, TempComp }
        private PickMode _pickMode = PickMode.None;

        public TempCompForm()
        {
            this.SemiModal = false;
            this.ShouldAutoPosition = true;
            this.ShouldCloseOnDocumentUnloading = true;
            this.FormClosing += OnFormClosing;
            BuildUI();

            // Subscribe to PS selection changes
            TxApplication.ActiveSelection.ItemsSet += OnSelectionChanged;
        }

        private void BuildUI()
        {
            this.Text = "Temp Comp Validator";
            this.Width = 680;
            this.Height = 820;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimumSize = new Size(500, 650);

            int lx = 10;
            int y = 15;

            
            // ── Bodypart paths ────────────────────────────────────
            var grpBody = new GroupBox
            {
                Text = "Bodypart Measurement Paths",
                Left = lx,
                Top = y,
                Width = 645,
                Height = 155,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            lstBodyPaths = new ListView
            {
                Left = 8,
                Top = 18,
                Width = 540,
                Height = 120,  // magasabb
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Font = new Font("Consolas", 8),
                Anchor = AnchorStyles.Top | AnchorStyles.Left |
             AnchorStyles.Right | AnchorStyles.Bottom
            };
            lstBodyPaths.Columns.Add("Path Name", 520);
            grpBody.Controls.Add(lstBodyPaths);

            btnPickBodyPaths = new Button
            {
                Text = "Pick",
                Left = 556,
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
                Left = 556,
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
            y += 130;

            // ── Temp Comp paths ───────────────────────────────────
            var grpTempComp = new GroupBox
            {
                Text = "Temp Comp Paths",
                Left = lx,
                Top = y,
                Width = 645,
                Height = 155,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            lstTempCompPaths = new ListView
            {
                Left = 8,
                Top = 18,
                Width = 540,
                Height = 120,  // magasabb
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Font = new Font("Consolas", 8),
                Anchor = AnchorStyles.Top | AnchorStyles.Left |
             AnchorStyles.Right | AnchorStyles.Bottom
            };
            lstTempCompPaths.Columns.Add("Path Name", 520);
            grpTempComp.Controls.Add(lstTempCompPaths);

            btnPickTempCompPaths = new Button
            {
                Text = "Pick",
                Left = 556,
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
                Left = 556,
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
            y += 130;

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
            lstResults.Columns.Add("Criterion", 220);
            lstResults.Columns.Add("J2", 90);
            lstResults.Columns.Add("J3", 90);
            lstResults.Columns.Add("Details", 200);
            lstResults.Columns.Add("Status", 80);

            this.Controls.Add(grpBody);
            this.Controls.Add(grpTempComp);
            this.Controls.Add(grpSettings);
            this.Controls.Add(btnAnalyze);
            this.Controls.Add(lblRes);
            this.Controls.Add(lstResults);
        }

        // ── Pick from PS ──────────────────────────────────────────
        private void StartPicking(PickMode mode)
        {
            _pickMode = mode;

            // Visual feedback
            btnPickBodyPaths.BackColor = mode == PickMode.Body
                ? Color.FromArgb(0, 160, 0)
                : Color.FromArgb(0, 100, 180);
            btnPickTempCompPaths.BackColor = mode == PickMode.TempComp
                ? Color.FromArgb(0, 160, 0)
                : Color.FromArgb(0, 100, 180);

            MessageBox.Show(
                string.Format("Select {0} paths in PS, then click OK.",
                    mode == PickMode.Body ? "Bodypart" : "Temp Comp"),
                "Pick Paths",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            // Read current PS selection
            var selection = TxApplication.ActiveSelection;
            AddFromSelection(selection.GetItems());

            // Reset pick mode
            _pickMode = PickMode.None;
            btnPickBodyPaths.BackColor = Color.FromArgb(0, 100, 180);
            btnPickTempCompPaths.BackColor = Color.FromArgb(0, 100, 180);
        }

        private void OnSelectionChanged(object sender, TxSelection_ItemsSetEventArgs e)
        {
            if (_pickMode == PickMode.None) return;
            var selection = TxApplication.ActiveSelection;
            AddFromSelection(selection.GetItems());
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
            lstResults.Items.Clear();

            

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

            // Read poses from Path
            var bodyPoses = new List<TempCompCalculations.RobotPose>();
            foreach (var prog in _bodyPrograms)
                bodyPoses.AddRange(TempCompCalculations.ReadPosesFromProgram(prog));

            var tempCompPoses = new List<TempCompCalculations.RobotPose>();
            foreach (var prog in _tempCompPrograms)
                tempCompPoses.AddRange(TempCompCalculations.ReadPosesFromProgram(prog));

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
            var c1 = TempCompCalculations.CheckJ2J3Coverage(bodyPoses, tempCompPoses);
            var c2 = TempCompCalculations.CheckJ2J3Spread(tempCompPoses);
            var c3 = TempCompCalculations.CheckJ5Symmetry(tempCompPoses);
            var c4 = TempCompCalculations.CheckJ2J3StepCoverage(bodyPoses, tempCompPoses, stepSize);
            var c5 = TempCompCalculations.CheckJ456MaxCoverage(bodyPoses, tempCompPoses);

            // Display results
            AddResultRow("1. J2/J3 Max Coverage",
                string.Format("Body max J2: {0:F1} ({1} TC pts)", c1.MaxJ2_Body, c1.CountJ2),
                string.Format("Body max J3: {0:F1} ({1} TC pts)", c1.MaxJ3_Body, c1.CountJ3),
                "Min 2 TC pts >= body max", c1.IsValid);

            AddResultRow("2. J2/J3 Angular Spread",
                string.Format("Spread: {0:F1} deg", c2.SpreadJ2),
                string.Format("Spread: {0:F1} deg", c2.SpreadJ3),
                "Min spread 75 deg", c2.IsValid);

            AddResultRow("3. J5 Symmetry",
                string.Format("Neg: {0} / Pos: {1}", c3.NegCount, c3.PosCount),
                "", string.Format("Total: {0} pts", c3.Total), c3.IsValid);

            AddResultRow("4. J2/J3 Step Coverage",
                c4.J2_OK ? "OK" : string.Format("{0} gaps", c4.J2_Gaps.Count),
                c4.J3_OK ? "OK" : string.Format("{0} gaps", c4.J3_Gaps.Count),
                string.Format("Step: {0} deg", stepSize), c4.IsValid);

            AddResultRow("5. J4/J5/J6 Max Coverage",
                string.Format("J4:{0} J5:{1} J6:{2}",
                    c5.J4_OK ? "OK" : "NOK",
                    c5.J5_OK ? "OK" : "NOK",
                    c5.J6_OK ? "OK" : "NOK"),
                "",
                string.Format("Max J4:{0:F1} J6:{1:F1}", c5.MaxJ4_Body, c5.MaxJ6_Body),
                c5.IsValid);

            foreach (ColumnHeader col in lstResults.Columns)
                col.Width = -2;
        }

        private void AddResultRow(string criterion,
            string j2val, string j3val, string details, bool ok)
        {
            var item = new ListViewItem(criterion);
            item.SubItems.Add(j2val);
            item.SubItems.Add(j3val);
            item.SubItems.Add(details);
            item.SubItems.Add(ok ? "OK" : "NOK");
            item.BackColor = ok ? Color.LightGreen : Color.LightCoral;
            lstResults.Items.Add(item);
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