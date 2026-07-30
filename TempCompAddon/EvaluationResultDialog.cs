using System;
using System.Drawing;
using System.Windows.Forms;
using ISRA.Calculations.TempComp.Domain.Results;
using Tecnomatix.Engineering.Ui;

namespace TempCompAddon
{
    public class EvaluationResultDialog : TxForm
    {
        public EvaluationResultDialog(EvaluationResult result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));

            Text = "AI Evaluation — TempComp Analysis";
            Size = new Size(1080, 700);
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(900, 560);
            Font = new Font("Segoe UI", 9);
            BackColor = Color.FromArgb(245, 247, 250);
            SemiModal = false;
            ShouldAutoPosition = true;
            ShouldCloseOnDocumentUnloading = true;

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1,
                Padding = new Padding(14)
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 108f));  // summary
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));   // grid
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48f));   // footer

            // ── Summary ───────────────────────────────────────
            var pnlSummary = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(12)
            };

            var lblTitle = new Label
            {
                Text = "Evaluation Summary",
                Dock = DockStyle.Top,
                Height = 26,
                Font = new Font("Segoe UI Semibold", 11f, FontStyle.Bold),
                ForeColor = Color.FromArgb(36, 36, 36),
                TextAlign = ContentAlignment.MiddleLeft
            };

            var lblStatus = new Label
            {
                Text = result.IsValid ? "STATUS: OK" : "STATUS: ACTION NEEDED",
                AutoSize = false,
                Width = 180,
                Height = 24,
                Left = pnlSummary.Width - 192,
                Top = 14,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = result.IsValid
                    ? Color.FromArgb(215, 242, 220)
                    : Color.FromArgb(255, 238, 204),
                ForeColor = result.IsValid
                    ? Color.FromArgb(20, 90, 40)
                    : Color.FromArgb(130, 78, 0),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold)
            };

            var lblSummary = new Label
            {
                Text = result.Summary,
                Dock = DockStyle.Bottom,
                Height = 58,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                ForeColor = Color.FromArgb(52, 52, 52),
                Padding = new Padding(0, 8, 0, 0)
            };

            pnlSummary.Controls.Add(lblSummary);
            pnlSummary.Controls.Add(lblStatus);
            pnlSummary.Controls.Add(lblTitle);
            layout.Controls.Add(pnlSummary, 0, 0);

            // ── DataGridView ──────────────────────────────────
            var grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Font = new Font("Segoe UI", 9),
                BorderStyle = BorderStyle.FixedSingle,
                GridColor = Color.FromArgb(230, 232, 236),
                BackgroundColor = Color.White,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                EnableHeadersVisualStyles = false,
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells,
                DefaultCellStyle =
                {
                    WrapMode = DataGridViewTriState.True,
                    Padding = new Padding(6, 5, 6, 5),
                    SelectionBackColor = Color.FromArgb(232, 241, 252),
                    SelectionForeColor = Color.Black
                },
                ColumnHeadersDefaultCellStyle =
                {
                    BackColor = Color.FromArgb(44, 62, 80),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI Semibold", 9f, FontStyle.Bold),
                    Alignment = DataGridViewContentAlignment.MiddleLeft,
                    Padding = new Padding(6, 5, 6, 5)
                },
                ColumnHeadersHeight = 36,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                AlternatingRowsDefaultCellStyle =
                {
                    BackColor = Color.FromArgb(248, 250, 252)
                }
            };

            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Severity",
                HeaderText = "Severity",
                Width = 110,
                FillWeight = 8,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None
            });
            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Axis",
                HeaderText = "Axis",
                Width = 70,
                FillWeight = 5,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None
            });
            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Issue",
                HeaderText = "Issue",
                FillWeight = 27
            });
            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "TcSuggestion",
                HeaderText = "TC Suggestion",
                FillWeight = 30
            });
            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "BodySuggestion",
                HeaderText = "Body Suggestion",
                FillWeight = 30
            });

            foreach (var finding in result.Findings)
            {
                int rowIdx = grid.Rows.Add(
                    finding.Severity == "Critical" ? "Critical" : "Warning",
                    finding.Axis,
                    finding.Issue,
                    finding.TcSuggestion ?? "",
                    finding.BodySuggestion ?? "—"
                );

                Color rowColor = finding.Severity == "Critical"
                    ? Color.FromArgb(255, 232, 232)
                    : Color.FromArgb(255, 247, 221);

                grid.Rows[rowIdx].DefaultCellStyle.BackColor = rowColor;
                grid.Rows[rowIdx].Cells[0].Style.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            }

            layout.Controls.Add(grid, 0, 1);

            // ── Footer ────────────────────────────────────────
            var footer = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0)
            };

            var lblCount = new Label
            {
                Text = string.Format("Findings: {0}", result.Findings == null ? 0 : result.Findings.Count),
                Dock = DockStyle.Left,
                Width = 180,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(96, 96, 96)
            };

            var btnClose = new Button
            {
                Text = "Close",
                Dock = DockStyle.Right,
                Width = 110,
                Height = 32,
                Margin = new Padding(0),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => Close();

            footer.Controls.Add(btnClose);
            footer.Controls.Add(lblCount);
            layout.Controls.Add(footer, 0, 2);

            Controls.Add(layout);
        }
    }
}