using System.Collections.Generic;
using ISRA.Core.Domain;
using ISRA.Calculations.TempComp.Domain;
using ISRA.Calculations.TempComp.Domain.Results;
using ISRA.Calculations.TempComp.RobotConfiguration;

namespace TempCompAddon.Presentation
{
    /// <summary>
    /// Data transfer object containing all data required for TempComp Excel export.
    /// </summary>
    public class TempCompExportData
    {
        /// <summary>
        /// Complete validation analysis report with all validator results.
        /// </summary>
        public AnalysisReport ValidationReport { get; set; }

        /// <summary>
        /// List of nearest TC results (body pose -> nearest TC pose + distance).
        /// </summary>
        public List<NearestTcResult> NearestTcResults { get; set; }

        /// <summary>
        /// All body measurement poses.
        /// </summary>
        public List<RobotPose> BodyPoses { get; set; }

        /// <summary>
        /// All temp comp measurement poses.
        /// </summary>
        public List<RobotPose> TempCompPoses { get; set; }

        /// <summary>
        /// Robot configuration (for axis naming and formatting).
        /// </summary>
        public IRobotConfiguration RobotConfiguration { get; set; }

        /// <summary>
        /// Maximum angle threshold used in validation.
        /// </summary>
        public double MaxAngleThreshold { get; set; }

        /// <summary>
        /// Statistics for body poses.
        /// </summary>
        public PoseStatistics BodyStatistics { get; set; }

        /// <summary>
        /// Statistics for temp comp poses.
        /// </summary>
        public PoseStatistics TempCompStatistics { get; set; }

        public TempCompExportData()
        {
            NearestTcResults = new List<NearestTcResult>();
            BodyPoses = new List<RobotPose>();
            TempCompPoses = new List<RobotPose>();
        }
    }
}
