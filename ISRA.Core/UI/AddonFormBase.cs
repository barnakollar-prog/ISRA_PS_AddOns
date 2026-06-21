using System;
using System.Drawing;
using System.Windows.Forms;
using Tecnomatix.Engineering.Ui;

namespace ISRA.Core.UI
{
    /// <summary>
    /// Base class for all ISRA addon forms providing common functionality.
    /// </summary>
    public abstract class AddonFormBase : TxForm
    {
        /// <summary>
        /// Main analyze button (common to all addons).
        /// </summary>
        protected Button BtnAnalyze { get; set; }

        /// <summary>
        /// Help/About button (common to all addons).
        /// </summary>
        protected Button BtnHelp { get; set; }

        protected AddonFormBase()
        {
            InitializeBase();
        }

        private void InitializeBase()
        {
            this.SemiModal = false;
            this.ShouldAutoPosition = true;
            this.ShouldCloseOnDocumentUnloading = true;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.FormClosing += OnFormClosing;
        }

        /// <summary>
        /// Builds the UI. Override this in derived classes.
        /// </summary>
        protected abstract void BuildUI();

        /// <summary>
        /// Handles the analyze button click. Override this in derived classes.
        /// </summary>
        protected abstract void OnAnalyzeClick(object sender, EventArgs e);

        /// <summary>
        /// Handles the help button click. Override this in derived classes.
        /// </summary>
        protected abstract void OnHelpClick(object sender, EventArgs e);

        /// <summary>
        /// Creates a standard analyze button with consistent styling.
        /// </summary>
        protected Button CreateAnalyzeButton(int left, int top, int width, int height)
        {
            var btn = new Button
            {
                Text = "Analyze",
                Left = left,
                Top = top,
                Width = width,
                Height = height,
                BackColor = Color.FromArgb(180, 0, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            btn.Click += OnAnalyzeClick;
            return btn;
        }

        /// <summary>
        /// Creates a standard help button with consistent styling.
        /// </summary>
        protected Button CreateHelpButton(int left, int top, int width, int height, AnchorStyles anchor)
        {
            var btn = new Button
            {
                Text = "Help / About",
                Left = left,
                Top = top,
                Width = width,
                Height = height,
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Anchor = anchor
            };
            btn.Click += OnHelpClick;
            return btn;
        }

        /// <summary>
        /// Creates a standard GroupBox with consistent styling.
        /// </summary>
        protected GroupBox CreateGroupBox(string text, int left, int top, int width, int height, AnchorStyles anchor)
        {
            return new GroupBox
            {
                Text = text,
                Left = left,
                Top = top,
                Width = width,
                Height = height,
                Anchor = anchor
            };
        }

        /// <summary>
        /// Called when form is closing. Override to add cleanup logic.
        /// </summary>
        protected virtual void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            // Base implementation - derived classes can override
        }
    }
}
