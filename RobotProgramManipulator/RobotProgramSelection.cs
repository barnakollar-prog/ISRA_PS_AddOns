using System.Collections.Generic;
using Tecnomatix.Engineering;

namespace ISRA.RobotProgramManipulator
{
    internal static class RobotProgramSelection
    {
        public static IList<ITxRoboticOrderedCompoundOperation> GetSelectedPrograms()
        {
            var programs = new List<ITxRoboticOrderedCompoundOperation>();
            TxObjectList selectedObjects = TxApplication.ActiveSelection.GetItems();

            foreach (ITxObject selectedObject in selectedObjects)
            {
                var program = selectedObject as ITxRoboticOrderedCompoundOperation;
                if (program != null && program.Robot != null && !Contains(programs, program))
                {
                    programs.Add(program);
                }
            }

            return programs;
        }

        private static bool Contains(
            IEnumerable<ITxRoboticOrderedCompoundOperation> programs,
            ITxRoboticOrderedCompoundOperation candidate)
        {
            foreach (ITxRoboticOrderedCompoundOperation program in programs)
            {
                if (ReferenceEquals(program, candidate) || ((ITxObject)program).Id == ((ITxObject)candidate).Id)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
