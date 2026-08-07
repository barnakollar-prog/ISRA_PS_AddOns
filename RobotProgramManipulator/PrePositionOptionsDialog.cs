using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Tecnomatix.Engineering;

namespace ISRA.RobotProgramManipulator
{
    internal sealed class PrePositionOptionsDialog : Form
    {
        private readonly RadioButton _measurementRadioButton;
        private readonly RadioButton _serviceRadioButton;
        private readonly CheckBox _includeExistingCheckBox;
        private readonly NumericUpDown[] _axisInputs = new NumericUpDown[6];
        private readonly CheckBox _addCommentCheckBox = new CheckBox();
        private readonly TextBox _commentDeclarationBeforeTextBox = new TextBox();
        private readonly TextBox _commentDeclarationAfterTextBox = new TextBox();

        public PrePositionOptionsDialog(IList<ITxRoboticOrderedCompoundOperation> programs)
        {
            Text = "ISRA Robot Program Manipulator";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = true;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(720, 640);
            MinimumSize = new Size(680, 600);
            SizeGripStyle = SizeGripStyle.Show;
            Font = SystemFonts.MessageBoxFont;

            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(12),
                ColumnCount = 1,
                RowCount = 2
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
            Controls.Add(mainLayout);

            _measurementRadioButton = new RadioButton
            {
                Text = "Measurement",
                AutoSize = true,
                Anchor = AnchorStyles.None,
                Checked = true
            };
            _serviceRadioButton = new RadioButton
            {
                Text = "Service",
                AutoSize = true,
                Anchor = AnchorStyles.None
            };
            _measurementRadioButton.CheckedChanged += ProgramTypeChanged;
            _serviceRadioButton.CheckedChanged += ProgramTypeChanged;

            _includeExistingCheckBox = new CheckBox
            {
                Text = "Include existing PrePos_ points as source points",
                AutoSize = true,
                Anchor = AnchorStyles.Left
            };

            var tabs = new TabControl
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 8)
            };
            tabs.TabPages.Add(CreatePrePositionsTab(programs));
            tabs.TabPages.Add(CreateOlpCommandsTab());
            tabs.TabPages.Add(CreateInformationTab());
            mainLayout.Controls.Add(tabs, 0, 0);
            mainLayout.Controls.Add(CreateButtons(), 0, 1);

            ApplyProgramTypeDefaults();
        }

        private TabPage CreatePrePositionsTab(IList<ITxRoboticOrderedCompoundOperation> programs)
        {
            var tab = new TabPage("Pre-Positions") { Padding = new Padding(12) };
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 105));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 75));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 112));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            Control programList = CreateProgramList(programs);
            programList.Margin = new Padding(0, 0, 0, 10);
            layout.Controls.Add(programList, 0, 0);

            Control programType = CreateProgramTypeGroup();
            programType.Margin = new Padding(0, 0, 0, 10);
            layout.Controls.Add(programType, 0, 1);

            _includeExistingCheckBox.Margin = new Padding(8, 0, 0, 8);
            layout.Controls.Add(_includeExistingCheckBox, 0, 2);

            Control commentDeclarations = CreateCommentDeclarationsGroup();
            commentDeclarations.Margin = new Padding(0, 0, 0, 10);
            layout.Controls.Add(commentDeclarations, 0, 3);

            Control axisGroup = CreateAxisGroup();
            axisGroup.Margin = new Padding(0);
            layout.Controls.Add(axisGroup, 0, 4);

            tab.Controls.Add(layout);
            return tab;
        }

        private static TabPage CreateInformationTab()
        {
            var tab = new TabPage("Information") { Padding = new Padding(18) };
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 1,
                RowCount = 3
            };

            layout.Controls.Add(CreateInformationGroup(
                "Pre-Positions",
                "Select robot programs, choose Measurement or Service, adjust the axes and comment declarations, then create the taught pre-positions."), 0, 0);
            layout.Controls.Add(CreateInformationGroup(
                "Important",
                "Points containing HOME or MOVE are ignored. Existing PrePos_ points and duplicate handling are controlled by the selected options."), 0, 1);
            layout.Controls.Add(CreateInformationGroup(
                "OLP Commands",
                "A short explanation of the OLP command functions will be added here when that feature is implemented."), 0, 2);

            tab.Controls.Add(layout);
            return tab;
        }

        private static Control CreateInformationGroup(string title, string text)
        {
            var group = new GroupBox
            {
                Text = title,
                Dock = DockStyle.Top,
                AutoSize = true,
                Padding = new Padding(12),
                Margin = new Padding(0, 0, 0, 12)
            };
            group.Controls.Add(new Label
            {
                Text = text,
                Dock = DockStyle.Top,
                AutoSize = true,
                MaximumSize = new Size(520, 0),
                Margin = new Padding(0)
            });
            return group;
        }

        private Control CreateCommentDeclarationsGroup()
        {
            var group = new GroupBox { Text = "Generated OLP comment", Dock = DockStyle.Fill };
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 2,
                Padding = new Padding(12, 8, 12, 8)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 24));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 26));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 24));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 26));

            _addCommentCheckBox.Text = "Add comment";
            _addCommentCheckBox.AutoSize = true;
            _addCommentCheckBox.Anchor = AnchorStyles.Left;
            _addCommentCheckBox.Margin = new Padding(6, 0, 6, 4);
            _addCommentCheckBox.CheckedChanged += AddCommentCheckedChanged;
            layout.Controls.Add(_addCommentCheckBox, 0, 0);
            layout.SetColumnSpan(_addCommentCheckBox, 4);

            layout.Controls.Add(CreateFieldLabel("Declaration before"), 0, 1);
            ConfigureCommentDeclarationTextBox(_commentDeclarationBeforeTextBox);
            layout.Controls.Add(_commentDeclarationBeforeTextBox, 1, 1);
            layout.Controls.Add(CreateFieldLabel("Declaration after"), 2, 1);
            ConfigureCommentDeclarationTextBox(_commentDeclarationAfterTextBox);
            layout.Controls.Add(_commentDeclarationAfterTextBox, 3, 1);

            UpdateCommentDeclarationState();

            group.Controls.Add(layout);
            return group;
        }

        private static Label CreateFieldLabel(string text)
        {
            return new Label
            {
                Text = text,
                AutoSize = true,
                Anchor = AnchorStyles.Right,
                Margin = new Padding(6)
            };
        }

        private static void ConfigureCommentDeclarationTextBox(TextBox textBox)
        {
            textBox.Dock = DockStyle.Fill;
            textBox.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            textBox.Margin = new Padding(6);
        }

        private Control CreateProgramTypeGroup()
        {
            var group = new GroupBox { Text = "Robot program type", Dock = DockStyle.Fill };
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(12, 6, 12, 6)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            layout.Controls.Add(_measurementRadioButton, 0, 0);
            layout.Controls.Add(_serviceRadioButton, 1, 0);
            group.Controls.Add(layout);
            return group;
        }

        private static TabPage CreateOlpCommandsTab()
        {
            var tab = new TabPage("OLP Commands") { Padding = new Padding(24) };
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

            var messagePanel = new TableLayoutPanel
            {
                AutoSize = true,
                Anchor = AnchorStyles.None,
                ColumnCount = 1,
                RowCount = 2
            };
            messagePanel.Controls.Add(new Label
            {
                Text = "OLP command manipulation",
                AutoSize = true,
                Anchor = AnchorStyles.None,
                Font = new Font(SystemFonts.MessageBoxFont, FontStyle.Bold),
                Margin = new Padding(0, 0, 0, 10)
            }, 0, 0);
            messagePanel.Controls.Add(new Label
            {
                Text = "Controls for modifying OLP commands will be added here.",
                AutoSize = true,
                Anchor = AnchorStyles.None,
                ForeColor = SystemColors.GrayText
            }, 0, 1);

            layout.Controls.Add(messagePanel, 0, 1);
            tab.Controls.Add(layout);
            return tab;
        }

        public PrePositionOptions Options
        {
            get
            {
                var offsets = new double[_axisInputs.Length];
                for (int index = 0; index < _axisInputs.Length; index++)
                {
                    offsets[index] = decimal.ToDouble(_axisInputs[index].Value);
                }

                return new PrePositionOptions(
                    _serviceRadioButton.Checked ? RobotProgramType.Service : RobotProgramType.Measurement,
                    _includeExistingCheckBox.Checked,
                    offsets,
                    _addCommentCheckBox.Checked,
                    _commentDeclarationBeforeTextBox.Text,
                    _commentDeclarationAfterTextBox.Text);
            }
        }

        private void AddCommentCheckedChanged(object sender, EventArgs eventArgs)
        {
            UpdateCommentDeclarationState();
        }

        private void UpdateCommentDeclarationState()
        {
            _commentDeclarationBeforeTextBox.Enabled = _addCommentCheckBox.Checked;
            _commentDeclarationAfterTextBox.Enabled = _addCommentCheckBox.Checked;
        }

        private static Control CreateProgramList(IList<ITxRoboticOrderedCompoundOperation> programs)
        {
            var group = new GroupBox { Text = "Selected robot programs", Dock = DockStyle.Fill };
            var list = new ListBox
            {
                Dock = DockStyle.Fill,
                IntegralHeight = false,
                Margin = new Padding(8)
            };

            foreach (ITxRoboticOrderedCompoundOperation program in programs)
            {
                list.Items.Add(((ITxObject)program).Name);
            }

            group.Controls.Add(list);
            return group;
        }

        private Control CreateAxisGroup()
        {
            var group = new GroupBox { Text = "Robot axis offsets (degrees)", Dock = DockStyle.Fill };
            var grid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(16, 12, 16, 12),
                ColumnCount = 4,
                RowCount = 3
            };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 33.34F));

            for (int index = 0; index < _axisInputs.Length; index++)
            {
                var label = new Label
                {
                    Text = "Axis " + (index + 1),
                    AutoSize = true,
                    Anchor = AnchorStyles.Right,
                    Margin = new Padding(6)
                };
                var input = new NumericUpDown
                {
                    DecimalPlaces = 2,
                    Increment = 0.1M,
                    Minimum = -360M,
                    Maximum = 360M,
                    Width = 100,
                    Anchor = AnchorStyles.Left,
                    Margin = new Padding(6)
                };
                _axisInputs[index] = input;

                int row = index % 3;
                int column = (index / 3) * 2;
                grid.Controls.Add(label, column, row);
                grid.Controls.Add(input, column + 1, row);
            }

            group.Controls.Add(grid);
            return group;
        }

        private Control CreateButtons()
        {
            var panel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                Padding = new Padding(0, 8, 0, 0),
                Margin = new Padding(0)
            };
            var cancelButton = new Button
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Size = new Size(90, 30),
                Margin = new Padding(8, 0, 0, 0)
            };
            var createButton = new Button
            {
                Text = "Create pre-positions",
                DialogResult = DialogResult.OK,
                Size = new Size(150, 30),
                Margin = new Padding(8, 0, 0, 0)
            };

            AcceptButton = createButton;
            CancelButton = cancelButton;
            panel.Controls.Add(cancelButton);
            panel.Controls.Add(createButton);
            return panel;
        }

        private void ProgramTypeChanged(object sender, EventArgs eventArgs)
        {
            if (((RadioButton)sender).Checked)
            {
                ApplyProgramTypeDefaults();
            }
        }

        private void ApplyProgramTypeDefaults()
        {
            if (_axisInputs[0] == null)
            {
                return;
            }

            for (int index = 0; index < _axisInputs.Length; index++)
            {
                _axisInputs[index].Value = _serviceRadioButton.Checked ? -1M : 0M;
            }

            if (_measurementRadioButton.Checked)
            {
                _axisInputs[5].Value = -2M;
            }
        }
    }
}
