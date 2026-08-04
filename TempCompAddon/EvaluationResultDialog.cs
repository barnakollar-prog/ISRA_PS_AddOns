using System;
using System.Drawing;
using System.Windows.Forms;
using ISRA.Calculations.TempComp.Domain.Results;
using Tecnomatix.Engineering.Ui;

namespace TempCompAddon
{
    public class EvaluationResultDialog : TxForm
    {
        public EvaluationResultDialog(EvaluationResult result, string copilotOutput, string tokenInfo)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));

            Text = "TempComp Evaluation Report";
            Size = new Size(1080, 800);
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(900, 600);
            Font = new Font("Segoe UI", 9);
            BackColor = Color.FromArgb(245, 247, 250);
            SemiModal = false;
            ShouldAutoPosition = true;
            ShouldCloseOnDocumentUnloading = true;

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 4,
                ColumnCount = 1,
                Padding = new Padding(14)
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 108f));  // summary
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));    // grid
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));    // copilot
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
                Text = "Executive Summary",
                Dock = DockStyle.Top,
                Height = 26,
                Font = new Font("Segoe UI Semibold", 11f, FontStyle.Bold),
                ForeColor = Color.FromArgb(36, 36, 36),
                TextAlign = ContentAlignment.MiddleLeft
            };

            var isValid = result.IsValid;
            var lblStatus = new Label
            {
                Text = isValid ? "RESULT: ACCEPTED" : "RESULT: REVIEW REQUIRED",
                AutoSize = false,
                Width = 210,
                Height = 24,
                Left = pnlSummary.Width - 222,
                Top = 14,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = isValid
                    ? Color.FromArgb(215, 242, 220)
                    : Color.FromArgb(255, 238, 204),
                ForeColor = isValid
                    ? Color.FromArgb(20, 90, 40)
                    : Color.FromArgb(130, 78, 0),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold)
            };

            var lblSummary = new Label
            {
                Text = string.IsNullOrWhiteSpace(result.Summary)
                    ? "No summary was generated for this evaluation run."
                    : result.Summary,
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
                HeaderText = "Thermal Compensation Suggestion",
                FillWeight = 30
            });
            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "BodySuggestion",
                HeaderText = "Body Parameter Suggestion",
                FillWeight = 30
            });

            int criticalCount = 0;
            int warningCount = 0;

            if (result.Findings != null)
            {
                foreach (var finding in result.Findings)
                {
                    var isCritical = string.Equals(finding.Severity, "Critical", StringComparison.OrdinalIgnoreCase);
                    var severityText = isCritical ? "Critical" : "Warning";
                    if (isCritical)
                        criticalCount++;
                    else
                        warningCount++;

                    int rowIdx = grid.Rows.Add(
                        severityText,
                        finding.Axis,
                        finding.Issue,
                        finding.TcSuggestion ?? "",
                        finding.BodySuggestion ?? "—"
                    );

                    Color rowColor = isCritical
                        ? Color.FromArgb(255, 232, 232)
                        : Color.FromArgb(255, 247, 221);

                    grid.Rows[rowIdx].DefaultCellStyle.BackColor = rowColor;
                    grid.Rows[rowIdx].Cells[0].Style.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                }
            }

            layout.Controls.Add(grid, 0, 1);

            // ── Copilot Output ────────────────────────────────
            var pnlCopilot = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(0)
            };

            var pnlCopilotHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 34,
                Padding = new Padding(10, 0, 10, 0),
                BackColor = Color.FromArgb(245, 249, 255)
            };

            string headerText = "AI-Assisted Technical Analysis";
            if (!string.IsNullOrEmpty(tokenInfo))
                headerText += $"   ({tokenInfo})";

            var lblCopilotHeader = new Label
            {
                Text = headerText,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 84, 166),
                TextAlign = ContentAlignment.MiddleLeft
            };

            var pnlCopilotContent = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10, 8, 10, 10),
                BackColor = Color.White
            };

            var txtCopilot = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BackColor = Color.FromArgb(252, 253, 255),
                Font = new Font("Segoe UI", 9f),
                BorderStyle = BorderStyle.FixedSingle,
                ScrollBars = RichTextBoxScrollBars.Vertical,
                DetectUrls = true,
                WordWrap = true
            };

            PopulateAnalysisText(txtCopilot, copilotOutput);

            pnlCopilotHeader.Controls.Add(lblCopilotHeader);
            pnlCopilotContent.Controls.Add(txtCopilot);
            pnlCopilot.Controls.Add(pnlCopilotContent);
            pnlCopilot.Controls.Add(pnlCopilotHeader);
            layout.Controls.Add(pnlCopilot, 0, 2);

            // ── Footer ────────────────────────────────────────
            var footer = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0)
            };

            var lblCount = new Label
            {
                Text = string.Format(
                    "Findings: {0}   |   Critical: {1}   |   Warning: {2}",
                    result.Findings == null ? 0 : result.Findings.Count,
                    criticalCount,
                    warningCount),
                Dock = DockStyle.Left,
                Width = 420,
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
            layout.Controls.Add(footer, 0, 3);

            Controls.Add(layout);
        }

        private static void PopulateAnalysisText(RichTextBox outputBox, string content)
        {
            outputBox.Clear();

            if (string.IsNullOrWhiteSpace(content))
            {
                outputBox.SelectionColor = Color.FromArgb(110, 110, 110);
                outputBox.SelectionFont = new Font("Segoe UI", 9f, FontStyle.Italic);
                outputBox.AppendText("No AI analysis output is available for this evaluation.");
                outputBox.SelectionStart = 0;
                outputBox.SelectionLength = 0;
                return;
            }

            var lines = content.Replace("\r\n", "\n").Split('\n');
            foreach (var rawLine in lines)
            {
                var line = rawLine?.TrimEnd() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(line))
                {
                    outputBox.AppendText(Environment.NewLine);
                    continue;
                }

                if (IsAnalysisHeading(line))
                {
                    outputBox.SelectionColor = Color.FromArgb(0, 84, 166);
                    outputBox.SelectionFont = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold);
                    outputBox.AppendText(NormalizeHeading(line));
                    outputBox.AppendText(Environment.NewLine);
                    continue;
                }

                outputBox.SelectionColor = Color.FromArgb(40, 40, 40);
                outputBox.SelectionFont = new Font("Segoe UI", 9f, FontStyle.Regular);
                outputBox.AppendText(line);
                outputBox.AppendText(Environment.NewLine);
            }

            outputBox.SelectionStart = 0;
            outputBox.SelectionLength = 0;
        }

        private static bool IsAnalysisHeading(string line)
        {
            if (line.StartsWith("### ", StringComparison.Ordinal) ||
                line.StartsWith("## ", StringComparison.Ordinal) ||
                line.StartsWith("# ", StringComparison.Ordinal))
            {
                return true;
            }

            return line.EndsWith(":", StringComparison.Ordinal) && line.Length <= 80;
        }

        private static string NormalizeHeading(string line)
        {
            return line.TrimStart('#', ' ').Trim();
        }
    }
}