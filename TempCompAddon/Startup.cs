using System;
using System.Windows.Forms;
using Tecnomatix.Engineering;
using Tecnomatix.Engineering.Ui;

namespace TempCompAddon
{
    public class TempCompCommand : TxButtonCommand
    {
        public override string Category { get { return "Tools"; } }
        public override string Name { get { return "Temp Comp Validator"; } }

        public override void Execute(object cmdParams)
        {
            var form = new TempCompForm();
            form.Show();
        }
    }

    public class TempCompForm : TxForm
    {
        public TempCompForm()
        {
            this.SemiModal = false;
            this.ShouldAutoPosition = true;
            this.ShouldCloseOnDocumentUnloading = true;
            BuildUI();
        }

        private void BuildUI()
        {
            this.Text = "Temp Comp Validator";
            this.Width = 600;
            this.Height = 400;

            var label = new System.Windows.Forms.Label
            {
                Text = "Temp Comp Validator - Work in Progress",
                Left = 20,
                Top = 20,
                Width = 400,
                Height = 30,
                Font = new System.Drawing.Font("Segoe UI", 12)
            };
            this.Controls.Add(label);
        }
    }
}