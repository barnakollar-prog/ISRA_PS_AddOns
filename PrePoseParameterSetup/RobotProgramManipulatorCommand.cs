using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Tecnomatix.Engineering;

namespace ISRA.RobotProgramManipulator
{
    public sealed class RobotProgramManipulatorCommand : TxButtonCommand
    {
        private readonly RobotProgramManipulatorCommandEnabler _enabler =
            new RobotProgramManipulatorCommandEnabler();

        public override string Category
        {
            get { return "Robotics"; }
        }

        public override string Name
        {
            get { return "ISRA Robot Program Manipulator"; }
        }

        public override string Bitmap
        {
            get { return string.Empty; }
        }

        public override ITxCommandEnabler CommandEnabler
        {
            get { return _enabler; }
        }

        public override bool Connect()
        {
            return true;
        }

        public override void Execute(object cmdParams)
        {
            try
            {
                IList<ITxRoboticOrderedCompoundOperation> programs =
                    RobotProgramSelection.GetSelectedPrograms();

                if (programs.Count == 0)
                {
                    MessageBox.Show(
                        "Select one or more robot programs in the operation tree.",
                        Name,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                using (var dialog = new PrePositionOptionsDialog(programs))
                {
                    if (dialog.ShowDialog() != DialogResult.OK)
                    {
                        return;
                    }

                    var processor = new PrePositionProcessor();
                    PrePositionProcessingResult result = processor.Process(programs, dialog.Options);
                    MessageBox.Show(
                        result.CreateSummary(),
                        Name,
                        MessageBoxButtons.OK,
                        result.ErrorCount > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(
                    "The pre-position operation could not be completed." + Environment.NewLine + exception.Message,
                    Name,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
