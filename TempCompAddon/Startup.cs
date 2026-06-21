using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Tecnomatix.Engineering;
using Tecnomatix.Engineering.Ui;
using ISRA.Calculations.TempComp;
using ISRA.Calculations.TempComp.Domain;
using ISRA.Calculations.TempComp.Domain.Results;
using ISRA.Calculations.TempComp.RobotConfiguration;
using ISRA.Core.Domain;
using TempCompAddon.Presentation;

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

    public class TempCompForm : TxForm, ITempCompView
    {
        // ── MVP ───────────────────────────────────────────────────
        private readonly TempCompPresenter _presenter;
        private readonly TempCompReportFormatter _formatter;

        // ── Controls ──────────────────────────────────────────────
        private TxObjEditBoxCtrl pickerRobot;
        private ListView lstBodyPaths;
        private ListView lstTempCompPaths;
        private Button btnPickBodyPaths;
        private Button btnClearBodyPaths;
        private Button btnPickTempCompPaths;
        private Button btnClearTempCompPaths;
        private Button btnRemoveBodyPaths;
        private Button btnRemoveTempCompPaths;
        private Button btnHelp;
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
            _presenter = new TempCompPresenter(this);
            _formatter = new TempCompReportFormatter();

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
            btnPickBodyPaths.Click += OnPickBodyClick;
            grpBody.Controls.Add(btnPickBodyPaths);

            btnRemoveBodyPaths = new Button
            {
                Text = "Remove",
                Left = 676,
                Top = 50,
                Width = 80,
                Height = 28,
                BackColor = Color.FromArgb(180, 120, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnRemoveBodyPaths.Click += (s, e) =>
            {
                for (int i = lstBodyPaths.SelectedIndices.Count - 1; i >= 0; i--)
                {
                    int idx = lstBodyPaths.SelectedIndices[i];
                    _bodyPrograms.RemoveAt(idx);
                    lstBodyPaths.Items.RemoveAt(idx);
                }
            };
            grpBody.Controls.Add(btnRemoveBodyPaths);

            btnClearBodyPaths = new Button
            {
                Text = "Clear",
                Left = 676,
                Top = 82,
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
            btnPickTempCompPaths.Click += OnPickTempCompClick;
            grpTempComp.Controls.Add(btnPickTempCompPaths);

            btnRemoveTempCompPaths = new Button
            {
                Text = "Remove",
                Left = 676,
                Top = 50,
                Width = 80,
                Height = 28,
                BackColor = Color.FromArgb(180, 120, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnRemoveTempCompPaths.Click += (s, e) =>
            {
                for (int i = lstTempCompPaths.SelectedIndices.Count - 1; i >= 0; i--)
                {
                    int idx = lstTempCompPaths.SelectedIndices[i];
                    _tempCompPrograms.RemoveAt(idx);
                    lstTempCompPaths.Items.RemoveAt(idx);
                }
            };
            grpTempComp.Controls.Add(btnRemoveTempCompPaths);

            btnClearTempCompPaths = new Button
            {
                Text = "Clear",
                Left = 676,
                Top = 82,
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
                Text = "Nearest TC distance (deg):",
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
                Maximum = 180,
                Value = 35,
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
                Height = 340,
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
            lstNearestTc.Columns.Add("J2-3", 60);
            lstNearestTc.Columns.Add("J4", 60);
            lstNearestTc.Columns.Add("J5", 60);
            lstNearestTc.Columns.Add("J6", 60);
            lstNearestTc.Columns.Add("TC Point", 120);
            lstNearestTc.Columns.Add("TC J2-3", 60);
            lstNearestTc.Columns.Add("TC J4", 60);
            lstNearestTc.Columns.Add("TC J5", 60);
            lstNearestTc.Columns.Add("TC J6", 60);
            lstNearestTc.Columns.Add("Max Diff", 60);
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
            lstRawData.Columns.Add("J2-3", 60);
            lstRawData.Columns.Add("TC Point", 110);
            lstRawData.Columns.Add("TC J1", 50);
            lstRawData.Columns.Add("TC J2", 50);
            lstRawData.Columns.Add("TC J3", 50);
            lstRawData.Columns.Add("TC J4", 50);
            lstRawData.Columns.Add("TC J5", 50);
            lstRawData.Columns.Add("TC J6", 50);
            lstRawData.Columns.Add("TC J2-3", 60);
            tabRaw.Controls.Add(lstRawData);

            tabControl.TabPages.Add(tabValidation);
            tabControl.TabPages.Add(tabNearest);
            tabControl.TabPages.Add(tabRaw);
            this.Controls.Add(tabControl);

            // ── Help button ───────────────────────────────────────
            btnHelp = new Button
            {
                Text = "Help / About",
                Left = lx,
                Top = this.ClientSize.Height - 38,
                Width = 765,
                Height = 28,
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            btnHelp.Click += (s, e) => HelpAbout.ShowAbout();
            this.Controls.Add(btnHelp);
        }

        // ── Pick from PS ──────────────────────────────────────────
        private void OnPickBodyClick(object sender, EventArgs e)
        {
            if (_pickMode == PickMode.Body) FinishPicking();
            else StartPicking(PickMode.Body);
        }

        private void OnPickTempCompClick(object sender, EventArgs e)
        {
            if (_pickMode == PickMode.TempComp) FinishPicking();
            else StartPicking(PickMode.TempComp);
        }

        private void StartPicking(PickMode mode)
        {
            _pickMode = mode;
            UpdatePickButtons();

            // Ami már ki van jelölve PS-ben, azt rögtön felvesszük
            AddFromSelection(TxApplication.ActiveSelection.GetItems());
        }

        private void FinishPicking()
        {
            var mode = _pickMode;
            _pickMode = PickMode.None;
            UpdatePickButtons();
            try { TxApplication.ActiveSelection.Clear(); } catch { }

            int count = mode == PickMode.Body
                ? _bodyPrograms.Count
                : _tempCompPrograms.Count;

            if (count == 0)
            {
                MessageBox.Show(
                    "No valid paths (Weld Operations) were selected in PS.",
                    "Nothing Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void UpdatePickButtons()
        {
            btnPickBodyPaths.Text = _pickMode == PickMode.Body ? "OK" : "Pick";
            btnPickBodyPaths.BackColor = _pickMode == PickMode.Body
                ? Color.FromArgb(0, 160, 0) : Color.FromArgb(0, 100, 180);

            btnPickTempCompPaths.Text = _pickMode == PickMode.TempComp ? "OK" : "Pick";
            btnPickTempCompPaths.BackColor = _pickMode == PickMode.TempComp
                ? Color.FromArgb(0, 160, 0) : Color.FromArgb(0, 100, 180);
        }


        private void OnSelectionChanged(object sender, TxSelection_ItemsSetEventArgs e)
        {
            if (_pickMode == PickMode.None) return;
            AddFromSelection(TxApplication.ActiveSelection.GetItems());
        }

        private int AddFromSelection(TxObjectList items)
        {
            int found = 0;
            foreach (ITxObject obj in items)
            {
                var prog = obj as TxWeldOperation;
                if (prog == null) continue;
                found++;

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
            return found;
        
        }

        // ── Analyze ───────────────────────────────────────────────
        private void OnAnalyze(object sender, EventArgs e)
        {
            _presenter.Analyze();
        }

        // ── ITempCompView Implementation ──────────────────────────
        public List<TxWeldOperation> BodyPrograms => _bodyPrograms;
        public List<TxWeldOperation> TempCompPrograms => _tempCompPrograms;
        public TxRobot SelectedRobot => pickerRobot.Object as TxRobot;
        public string SelectedRobotType
        {
            get
            {
                if (rbKuka.Checked) return "Kuka";
                if (rbAbb.Checked) return "ABB";
                return "Fanuc";
            }
        }
        public double MaxAngleThreshold => (double)nudStepSize.Value;

        public void DisplayValidationResults(AnalysisReport report)
        {
            _formatter.FormatValidationResults(report, lstValidation);
        }

        public void DisplayNearestTcResults(List<NearestTcResult> results, double threshold, IRobotConfiguration config)
        {
            _formatter.FormatNearestTcResults(results, lstNearestTc, threshold, config);
        }

        public void DisplayRawData(List<RobotPose> bodyPoses, List<RobotPose> tcPoses, IRobotConfiguration config, double threshold)
        {
            _formatter.FormatRawData(bodyPoses, tcPoses, lstRawData, config, threshold);
        }

        public void ShowError(string message, string title)
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        // ── Helpers ───────────────────────────────────────────────


        private void OnRobotPicked(object sender, TxObjEditBoxCtrl_PickedEventArgs e)
        {
            var robot = e.Object as TxRobot;
            if (robot == null) return;

            TempCompCalculations.RobotType? detected = null;

            // 1. Próba: 3D fájl útvonal (cojt path)
            try
            {
                var libStorage = robot.StorageObject as TxLibraryStorage;
                if (libStorage != null)
                    detected = DetectRobotTypeFromPath(libStorage.FullPath);
            }
            catch { }

            // 2. Fallback: robot név
            if (detected == null)
                detected = DetectRobotTypeFromPath(robot.Name);

            if (detected == TempCompCalculations.RobotType.Kuka)
                rbKuka.Checked = true;
            else if (detected == TempCompCalculations.RobotType.Fanuc)
                rbFanuc.Checked = true;
            else if (detected == TempCompCalculations.RobotType.Abb)
                rbAbb.Checked = true;
        }

        private TempCompCalculations.RobotType? DetectRobotTypeFromPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;

            string p = path.ToLower();

            // Könyvtárak vizsgálata az útvonalban
            if (p.Contains("kuka")) return TempCompCalculations.RobotType.Kuka;
            if (p.Contains("fanuc")) return TempCompCalculations.RobotType.Fanuc;
            if (p.Contains("abb")) return TempCompCalculations.RobotType.Abb;

            // Hint: KUKA robotok cojt fájlneve gyakran "KR"-rel kezdődik
            try
            {
                string fileName = System.IO.Path.GetFileNameWithoutExtension(path);
                if (fileName.StartsWith("KR", StringComparison.OrdinalIgnoreCase))
                    return TempCompCalculations.RobotType.Kuka;
            }
            catch { }

            return null;
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