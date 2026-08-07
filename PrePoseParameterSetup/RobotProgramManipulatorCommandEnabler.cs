using Tecnomatix.Engineering;

namespace ISRA.RobotProgramManipulator
{
    public sealed class RobotProgramManipulatorCommandEnabler : TxCommandSelectionEnabler
    {
        protected override void OnSelectionCleared()
        {
            _enable = false;
        }

        protected override void OnSelectionItemsAdded(TxObjectList objects)
        {
            UpdateEnabledState();
        }

        protected override void OnSelectionItemsRemoved(TxObjectList objects)
        {
            UpdateEnabledState();
        }

        protected override void OnSelectionItemsSet(TxObjectList objects)
        {
            UpdateEnabledState();
        }

        private void UpdateEnabledState()
        {
            try
            {
                _enable = RobotProgramSelection.GetSelectedPrograms().Count > 0;
            }
            catch
            {
                _enable = false;
            }
        }
    }
}
