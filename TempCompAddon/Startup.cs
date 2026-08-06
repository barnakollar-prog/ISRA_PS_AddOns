using ISRA.Calculations.TempComp;
using ISRA.Calculations.TempComp.Domain;
using ISRA.Calculations.TempComp.Domain.Results;
using ISRA.Calculations.TempComp.RobotConfiguration;
using ISRA.Core.Domain;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Tecnomatix.Engineering;
using Tecnomatix.Engineering.Ui;
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
        private DataGridView dgvGapAnalysis;
        private NumericUpDown nudJ2Gap;
        private NumericUpDown nudJ3Gap;
        private NumericUpDown nudJ4Gap;
        private NumericUpDown nudJ5Gap;
        private NumericUpDown nudJ6Gap;

        // Filter controls
        private RadioButton rbFilterNone;
        private RadioButton rbFilterAuto;
        private RadioButton rbFilterCustom;
        private TextBox txtBodyPrefixes;
        private TextBox txtTcPrefixes;
        private TextBox txtOlpKeywords;
        // 

        private NumericUpDown nudStepSize;
        private Button btnAnalyze;
        private Button btnExport;
        private RadioButton rbFanuc;
        private RadioButton rbKuka;
        private RadioButton rbAbb;

        // Tab results
        private ListView lstValidation;
        private ListView lstNearestTc;
        private ListView lstRawBody;
        private ListView lstRawTc;
        private int _rawBodySortColumn = -1;
        private SortOrder _rawBodySortOrder = SortOrder.Ascending;
        private int _rawTcSortColumn = -1;
        private SortOrder _rawTcSortOrder = SortOrder.Ascending;

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
            TxApplication.ActiveSelection.ItemsAdded += OnSelectionAdded;
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

            // ── Filter GroupBox ───────────────────────────────────
            var grpFilter = new GroupBox
            {
                Text = "Measurement Point Filter",
                Left = lx,
                Top = y,
                Width = 765,
                Height = 110,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            rbFilterNone = new RadioButton { Text = "No Filter", Left = 8, Top = 20, Width = 90, Height = 20 };
            rbFilterAuto = new RadioButton { Text = "Auto", Left = 105, Top = 20, Width = 70, Height = 20, Checked = true };
            rbFilterCustom = new RadioButton { Text = "Custom", Left = 180, Top = 20, Width = 80, Height = 20 };
            grpFilter.Controls.Add(rbFilterNone);
            grpFilter.Controls.Add(rbFilterAuto);
            grpFilter.Controls.Add(rbFilterCustom);

            grpFilter.Controls.Add(new Label { Text = "Body prefixes:", Left = 8, Top = 50, Width = 90, Height = 20, TextAlign = ContentAlignment.MiddleLeft });
            txtBodyPrefixes = new TextBox { Left = 102, Top = 48, Width = 200, Height = 22, Text = "mp", Enabled = false };
            grpFilter.Controls.Add(txtBodyPrefixes);

            grpFilter.Controls.Add(new Label { Text = "TC prefixes:", Left = 310, Top = 50, Width = 80, Height = 20, TextAlign = ContentAlignment.MiddleLeft });
            txtTcPrefixes = new TextBox { Left = 394, Top = 48, Width = 200, Height = 22, Text = "art, temp", Enabled = false };
            grpFilter.Controls.Add(txtTcPrefixes);

            grpFilter.Controls.Add(new Label { Text = "OLP keywords:", Left = 8, Top = 78, Width = 90, Height = 20, TextAlign = ContentAlignment.MiddleLeft });
            txtOlpKeywords = new TextBox
            {
                Left = 102,
                Top = 76,
                Width = 650,
                Height = 22,
                Text = "meas, cmeas, inline, VW_USER, TECH10, PRC_IMT",
                Enabled = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            grpFilter.Controls.Add(txtOlpKeywords);

            // Enable/disable textboxes based on radio selection
            rbFilterCustom.CheckedChanged += (s, e) =>
            {
                txtBodyPrefixes.Enabled = rbFilterCustom.Checked;
                txtTcPrefixes.Enabled = rbFilterCustom.Checked;
                txtOlpKeywords.Enabled = rbFilterCustom.Checked;
            };

            this.Controls.Add(grpFilter);
            y += 118;

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
            // Tab 2: Nearest TC
            var tabNearest = new TabPage { Text = "Nearest TC Point (Experimental)" };
            lstNearestTc = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Font = new Font("Consolas", 8)
            };
            lstNearestTc.Columns.Add("Body Point", 120);
            lstNearestTc.Columns.Add("Body Path", 120);   // ← ÚJ
            lstNearestTc.Columns.Add("J2-3", 60);
            lstNearestTc.Columns.Add("J4", 60);
            lstNearestTc.Columns.Add("J5", 60);
            lstNearestTc.Columns.Add("J6", 60);
            lstNearestTc.Columns.Add("TC Point", 120);
            lstNearestTc.Columns.Add("TC Path", 120);     // ← ÚJ
            lstNearestTc.Columns.Add("TC J2-3", 60);
            lstNearestTc.Columns.Add("TC J4", 60);
            lstNearestTc.Columns.Add("TC J5", 60);
            lstNearestTc.Columns.Add("TC J6", 60);
            lstNearestTc.Columns.Add("Max Diff", 60);
            var pnlNearestInfo = new Panel
            {
                Dock = DockStyle.Top,
                Height = 30,
                BackColor = Color.FromArgb(255, 235, 156) // sárga
            };
            pnlNearestInfo.Controls.Add(new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 8, FontStyle.Italic),
                Padding = new Padding(6, 0, 0, 0)
            });
            tabNearest.Controls.Add(pnlNearestInfo);
            tabNearest.Controls.Add(lstNearestTc);

            // Tab 3: Raw Data
            var tabRaw = new TabPage { Text = "Raw Data" };
            var rawSplit = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 380
            };

            var grpRawBody = new GroupBox
            {
                Text = "Bodypart",
                Dock = DockStyle.Fill
            };

            lstRawBody = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Font = new Font("Consolas", 8)
            };
            lstRawBody.ColumnClick += OnRawBodyColumnClick;
            lstRawBody.MouseClick += OnRawBodyMouseClick;
            lstRawBody.Columns.Add("Body Point", 110);
            lstRawBody.Columns.Add("Body Path", 120);
            lstRawBody.Columns.Add("J1", 50);
            lstRawBody.Columns.Add("J2", 50);
            lstRawBody.Columns.Add("J3", 50);
            lstRawBody.Columns.Add("J4", 50);
            lstRawBody.Columns.Add("J5", 50);
            lstRawBody.Columns.Add("J6", 50);
            lstRawBody.Columns.Add("J2-3", 60);
            grpRawBody.Controls.Add(lstRawBody);

            var grpRawTc = new GroupBox
            {
                Text = "Temp Comp",
                Dock = DockStyle.Fill
            };

            lstRawTc = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Font = new Font("Consolas", 8)
            };
            lstRawTc.ColumnClick += OnRawTcColumnClick;
            lstRawTc.MouseClick += OnRawTcMouseClick;
            lstRawTc.Columns.Add("TC Point", 110);
            lstRawTc.Columns.Add("TC Path", 120);
            lstRawTc.Columns.Add("TC J1", 50);
            lstRawTc.Columns.Add("TC J2", 50);
            lstRawTc.Columns.Add("TC J3", 50);
            lstRawTc.Columns.Add("TC J4", 50);
            lstRawTc.Columns.Add("TC J5", 50);
            lstRawTc.Columns.Add("TC J6", 50);
            lstRawTc.Columns.Add("TC J2-3", 60);
            grpRawTc.Controls.Add(lstRawTc);

            rawSplit.Panel1.Controls.Add(grpRawBody);
            rawSplit.Panel2.Controls.Add(grpRawTc);
            tabRaw.Controls.Add(rawSplit);
            // Tab 4: Gap Analysis
            var tabGap = new TabPage { Text = "Gap Analysis" };

            // Settings panel (top)
            var pnlGapSettings = new Panel
            {
                Dock = DockStyle.Top,
                Height = 36,
                Padding = new Padding(4)
            };

            pnlGapSettings.Controls.Add(new Label { Text = "J2 max gap (°):", Left = 8, Top = 8, Width = 100, Height = 20, TextAlign = ContentAlignment.MiddleLeft });
            nudJ2Gap = new NumericUpDown { Left = 110, Top = 6, Width = 60, Height = 24, Minimum = 1, Maximum = 180, Value = 25, DecimalPlaces = 0 };
            pnlGapSettings.Controls.Add(nudJ2Gap);

            pnlGapSettings.Controls.Add(new Label { Text = "J3 max gap (°):", Left = 182, Top = 8, Width = 100, Height = 20, TextAlign = ContentAlignment.MiddleLeft });
            nudJ3Gap = new NumericUpDown { Left = 284, Top = 6, Width = 60, Height = 24, Minimum = 1, Maximum = 180, Value = 25, DecimalPlaces = 0 };
            pnlGapSettings.Controls.Add(nudJ3Gap);

            pnlGapSettings.Controls.Add(new Label { Text = "J4 max gap (°):", Left = 356, Top = 8, Width = 100, Height = 20, TextAlign = ContentAlignment.MiddleLeft });
            nudJ4Gap = new NumericUpDown { Left = 458, Top = 6, Width = 60, Height = 24, Minimum = 1, Maximum = 180, Value = 25, DecimalPlaces = 0 };
            pnlGapSettings.Controls.Add(nudJ4Gap);

            pnlGapSettings.Controls.Add(new Label { Text = "J5 max gap (°):", Left = 530, Top = 8, Width = 100, Height = 20, TextAlign = ContentAlignment.MiddleLeft });
            nudJ5Gap = new NumericUpDown { Left = 632, Top = 6, Width = 60, Height = 24, Minimum = 1, Maximum = 180, Value = 25, DecimalPlaces = 0 };
            pnlGapSettings.Controls.Add(nudJ5Gap);

            pnlGapSettings.Controls.Add(new Label { Text = "J6 max gap (°):", Left = 704, Top = 8, Width = 100, Height = 20, TextAlign = ContentAlignment.MiddleLeft });
            nudJ6Gap = new NumericUpDown { Left = 806, Top = 6, Width = 60, Height = 24, Minimum = 1, Maximum = 180, Value = 25, DecimalPlaces = 0 };
            pnlGapSettings.Controls.Add(nudJ6Gap);

            // DataGridView
            dgvGapAnalysis = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Font = new Font("Consolas", 8),
                BorderStyle = BorderStyle.None,
                GridColor = Color.LightGray,
                BackgroundColor = Color.White
            };
            dgvGapAnalysis.CellClick += OnGapAnalysisCellClick;

            tabGap.Controls.Add(dgvGapAnalysis);
            tabGap.Controls.Add(pnlGapSettings);

            tabControl.TabPages.Add(tabValidation);
            tabControl.TabPages.Add(tabNearest);
            tabControl.TabPages.Add(tabRaw);
            tabControl.TabPages.Add(tabGap);        // ← ÚJ
            this.Controls.Add(tabControl);


            // ── Bottom button panel (docked) ──────────────────────
            var pnlButtons = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 108,
                Padding = new Padding(lx, 4, lx, 4)
            };

            var buttonsLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            buttonsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32f));
            buttonsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32f));
            buttonsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28f));

            btnExport = new Button
            {
                Text = "Export to Excel",
                Dock = DockStyle.Fill,
                Height = 32,
                Margin = new Padding(0),
                BackColor = Color.FromArgb(0, 120, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };
            btnExport.Click += OnExport;

            var btnEvaluate = new Button
            {
                Text = "AI Evaluation",
                Dock = DockStyle.Fill,
                Height = 32,
                Margin = new Padding(0, 4, 0, 0),
                BackColor = Color.FromArgb(0, 84, 166),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };
            btnEvaluate.Click += OnEvaluate;

            btnHelp = new Button
            {
                Text = "Help / About",
                Dock = DockStyle.Fill,
                Height = 28,
                Margin = new Padding(0, 4, 0, 0),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                TextAlign = ContentAlignment.MiddleCenter
            };
            btnHelp.Click += (s, e) => HelpAbout.ShowAbout();

            buttonsLayout.Controls.Add(btnExport, 0, 0);
            buttonsLayout.Controls.Add(btnEvaluate, 0, 1);
            buttonsLayout.Controls.Add(btnHelp, 0, 2);
            pnlButtons.Controls.Add(buttonsLayout);
            this.Controls.Add(pnlButtons);

            // ── Adjust TabControl height to end above the button panel ──
            tabControl.Height = this.ClientSize.Height - tabControl.Top - pnlButtons.Height - 8;
        }
        private void OnEvaluate(object sender, EventArgs e)
        {
            _presenter.Evaluate();
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
        private void OnRawBodyMouseClick(object sender, MouseEventArgs e)
        {
            var lv = sender as ListView;
            if (lv == null) return;

            var hit = lv.HitTest(e.Location);
            var item = hit.Item;
            if (item == null || hit.SubItem == null) return;

            int columnIndex = item.SubItems.IndexOf(hit.SubItem);
            if (columnIndex != 0) return;
            if (item.SubItems.Count < 2) return;

            string poseName = item.SubItems[0].Text;        // Body Point (0. oszlop)
            string pathName = item.SubItems[1].Text;        // Body Path (1. oszlop)
            if (string.IsNullOrWhiteSpace(poseName) || string.IsNullOrWhiteSpace(pathName)) return;


            _presenter.JumpToLocation(pathName, poseName, isBodyPoint: true);
        }

        private void OnRawTcMouseClick(object sender, MouseEventArgs e)
        {
            var lv = sender as ListView;
            if (lv == null) return;

            var hit = lv.HitTest(e.Location);
            var item = hit.Item;
            if (item == null || hit.SubItem == null) return;

            int columnIndex = item.SubItems.IndexOf(hit.SubItem);
            if (columnIndex != 0) return;
            if (item.SubItems.Count < 2) return;

            string poseName = item.SubItems[0].Text;        // TC Point (0. oszlop)
            string pathName = item.SubItems[1].Text;        // TC Path (1. oszlop)
            if (string.IsNullOrWhiteSpace(poseName) || string.IsNullOrWhiteSpace(pathName)) return;


            _presenter.JumpToLocation(pathName, poseName, isBodyPoint: false);
        }
        // ── ÚJ: shift-tel bővített kijelölés kezelése (2408) ──
        private void OnSelectionAdded(object sender, TxSelection_ItemsAddedEventArgs e)
        {
            if (_pickMode == PickMode.None) return;
            AddFromSelection(TxApplication.ActiveSelection.GetItems());
        }
        private int AddFromSelection(TxObjectList items)
        {
            int found = 0;
            foreach (ITxObject obj in items)
            {
                // 1. Először WeldOperation — ezt adjuk hozzá közvetlenül
                var prog = obj as TxWeldOperation;
                if (prog != null)
                {
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
                    continue;
                }

                // 2. Compound vagy Generic Robotic Operation → kibontjuk
                var compound = obj as ITxCompoundOperation;
                if (compound != null)
                {
                    var children = compound.GetAllDescendants(
                        new TxTypeFilter(typeof(TxWeldOperation)));
                    found += AddFromSelection(children);
                    continue;
                }
            }
            return found;
        }

        // ── Analyze ───────────────────────────────────────────────
        private void OnAnalyze(object sender, EventArgs e)
        {
            _presenter.Analyze();
        }

        // ── Export ────────────────────────────────────────────────
        private void OnExport(object sender, EventArgs e)
        {
            _presenter.Export();
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
        public FilterMode FilterMode
        {
            get
            {
                if (rbFilterNone.Checked) return FilterMode.NoFilter;
                if (rbFilterCustom.Checked) return FilterMode.Custom;
                return FilterMode.Auto;
            }
        }

        public string[] CustomBodyPrefixes =>
            txtBodyPrefixes.Text
                .Split(',')
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrEmpty(s))
                .ToArray();

        public string[] CustomTcPrefixes =>
            txtTcPrefixes.Text
                .Split(',')
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrEmpty(s))
                .ToArray();

        public string[] CustomOlpKeywords =>
            txtOlpKeywords.Text
                .Split(',')
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrEmpty(s))
                .ToArray();
        public double J2GapThreshold => (double)nudJ2Gap.Value;
        public double J3GapThreshold => (double)nudJ3Gap.Value;
        public double J4GapThreshold => (double)nudJ4Gap.Value;
        public double J5GapThreshold => (double)nudJ5Gap.Value;
        public double J6GapThreshold => (double)nudJ6Gap.Value;

        public void DisplayGapAnalysis(GapAnalysisResult result)
        {
            _formatter.FormatGapAnalysis(result, dgvGapAnalysis);
        }
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
            _formatter.FormatRawData(bodyPoses, tcPoses, lstRawBody, lstRawTc, config, threshold);
        }

        public void ShowError(string message, string title)
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public bool HasResults => lstValidation.Items.Count > 0 ||
                                   lstNearestTc.Items.Count > 0 ||
                                   lstRawBody.Items.Count > 0 ||
                                   lstRawTc.Items.Count > 0;

        public void ShowExportDialog(TempCompExportData data)
        {
            TempCompAddon.Services.TempCompExcelExporter.Export(data);
        }

        public void ShowEvaluationResult(EvaluationResult result, string copilotOutput, string tokenInfo)
        {
            var dialog = new EvaluationResultDialog(result, copilotOutput, tokenInfo);
            dialog.ShowDialog();
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
        //
        private void OnRawBodyColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (_rawBodySortColumn == e.Column)
            {
                _rawBodySortOrder = _rawBodySortOrder == SortOrder.Ascending
                    ? SortOrder.Descending
                    : SortOrder.Ascending;
            }
            else
            {
                _rawBodySortColumn = e.Column;
                _rawBodySortOrder = SortOrder.Ascending;
            }

            lstRawBody.ListViewItemSorter = new ListViewItemComparer(e.Column, _rawBodySortOrder);
            lstRawBody.Sort();
        }

        private void OnRawTcColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (_rawTcSortColumn == e.Column)
            {
                _rawTcSortOrder = _rawTcSortOrder == SortOrder.Ascending
                    ? SortOrder.Descending
                    : SortOrder.Ascending;
            }
            else
            {
                _rawTcSortColumn = e.Column;
                _rawTcSortOrder = SortOrder.Ascending;
            }

            lstRawTc.ListViewItemSorter = new ListViewItemComparer(e.Column, _rawTcSortOrder);
            lstRawTc.Sort();
        }//
        private void OnGapAnalysisCellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var grid = sender as DataGridView;
            if (grid == null) return;

            // Ellenőrizzük hogy a kattintott oszlop "Point" oszlop-e
            if (!grid.Columns[e.ColumnIndex].HeaderText.Contains("Point")) return;

            var cell = grid.Rows[e.RowIndex].Cells[e.ColumnIndex];
            string cellValue = cell.Value as string;
            if (string.IsNullOrEmpty(cellValue)) return;

            // "UP107 - ART1_7" szétválasztása
            var parts = cellValue.Split(new[] { " - " }, StringSplitOptions.None);
            if (parts.Length != 2) return;

            string pathName = parts[0].Trim();
            string poseName = parts[1].Trim();

            _presenter.JumpToLocation(pathName, poseName);
        }
        private class ListViewItemComparer : System.Collections.IComparer
        {
            private readonly int _col;
            private readonly SortOrder _order;

            public ListViewItemComparer(int col, SortOrder order)
            {
                _col = col;
                _order = order;
            }

            public int Compare(object x, object y)
            {
                var lx = (ListViewItem)x;
                var ly = (ListViewItem)y;

                string sx = _col < lx.SubItems.Count ? lx.SubItems[_col].Text : "";
                string sy = _col < ly.SubItems.Count ? ly.SubItems[_col].Text : "";

                // Numeric comparison if both parseable
                if (double.TryParse(sx.Replace(",", "."),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out double dx) &&
                    double.TryParse(sy.Replace(",", "."),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out double dy))
                {
                    int result = dx.CompareTo(dy);
                    return _order == SortOrder.Ascending ? result : -result;
                }

                // String comparison fallback
                int strResult = string.Compare(sx, sy, StringComparison.OrdinalIgnoreCase);
                return _order == SortOrder.Ascending ? strResult : -strResult;
            }
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
                TxApplication.ActiveSelection.ItemsAdded -= OnSelectionAdded;
            }
            catch { }
        }
    }
}