using System.Collections.Generic;
using ISRA.Calculations.TempComp.RobotConfiguration;

namespace ISRA.Calculations.TempComp.Domain
{
    /// <summary>
    /// Input data for TempComp validation analysis.
    /// </summary>
    public class TempCompValidationInput
    {
        /// <summary>Robot poses from bodypart measurement paths</summary>
        public List<RobotPose> BodyPoses { get; set; }

        /// <summary>Robot poses from temp comp measurement paths</summary>
        public List<RobotPose> TempCompPoses { get; set; }

        /// <summary>Robot configuration (Fanuc, Kuka, ABB)</summary>
        public IRobotConfiguration RobotConfiguration { get; set; }

        /// <summary>Maximum angle threshold for nearest TC point distance (optional, default 35 degrees)</summary>
        public double MaxAngleThreshold { get; set; }

        public TempCompValidationInput()
        {
            BodyPoses = new List<RobotPose>();
            TempCompPoses = new List<RobotPose>();
            MaxAngleThreshold = 35.0;
        }
    }
}
