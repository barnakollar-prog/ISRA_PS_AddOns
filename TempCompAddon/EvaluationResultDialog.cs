using System;
using System.Drawing;
using System.Windows.Forms;
using ISRA.Calculations.TempComp.Domain.Results;

namespace TempCompAddon
{
    public class EvaluationResultDialog : Form
    {
        public EvaluationResultDialog(EvaluationResult result)
        {
            Text = "AI Evaluation — TempComp Analysis";
            Size = new Size(1000, 650);
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(800, 500);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1,
                Padding = new Padding(8)
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60f));   // summary
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));   // grid
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40f));   // close button

            // ── Summary ───────────────────────────────────────
            var pnlSummary = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = result.IsValid
                    ? Color.FromArgb(198, 239, 206)
                    : Color.FromArgb(255, 235, 156)
            };
            var lblSummary = new Label
            {
                Text = result.Summary,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                Padding = new Padding(8, 0, 8, 0)
            };
            pnlSummary.Controls.Add(lblSummary);
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
                Font = new Font("Segoe UI", 8),
                BorderStyle = BorderStyle.None,
                GridColor = Color.LightGray,
                BackgroundColor = Color.White,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Severity",
                HeaderText = "!",
                Width = 60,
                FillWeight = 5,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None
            });
            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Axis",
                HeaderText = "Axis",
                Width = 60,
                FillWeight = 5,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None
            });
            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Issue",
                HeaderText = "Issue",
                FillWeight = 25
            });
            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "TcSuggestion",
                HeaderText = "TC Suggestion",
                FillWeight = 35
            });
            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "BodySuggestion",
                HeaderText = "Body Suggestion",
                FillWeight = 35
            });

            foreach (var finding in result.Findings)
            {
                int rowIdx = grid.Rows.Add(
                    finding.Severity == "Critical" ? "⚠ Critical" : "ℹ Warning",
                    finding.Axis,
                    finding.Issue,
                    finding.TcSuggestion ?? "",
                    finding.BodySuggestion ?? "—"
                );

                Color rowColor = finding.Severity == "Critical"
                    ? Color.FromArgb(255, 199, 206)
                    : Color.FromArgb(255, 235, 156);

                grid.Rows[rowIdx].DefaultCellStyle.BackColor = rowColor;
            }

            grid.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

            layout.Controls.Add(grid, 0, 1);

            // ── Close button ──────────────────────────────────
            var btnClose = new Button
            {
                Text = "Close",
                Dock = DockStyle.Right,
                Width = 100,
                Height = 30,
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9)
            };
            btnClose.Click += (s, e) => Close();
            layout.Controls.Add(btnClose, 0, 2);

            Controls.Add(layout);
        }
    }
}