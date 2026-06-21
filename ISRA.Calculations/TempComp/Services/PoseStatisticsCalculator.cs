using System;
using System.Collections.Generic;
using ISRA.Calculations.TempComp.Domain;
using ISRA.Calculations.TempComp.Domain.Results;

namespace ISRA.Calculations.TempComp.Services
{
    /// <summary>
    /// Service for calculating statistical summaries of robot poses.
    /// </summary>
    public class PoseStatisticsCalculator
    {
        /// <summary>
        /// Calculates min/max statistics for all joint values in a collection of poses.
        /// </summary>
        public PoseStatistics Calculate(List<RobotPose> poses)
        {
            if (poses == null || poses.Count == 0)
                return new PoseStatistics();

            var stats = new PoseStatistics
            {
                J1_Min = double.MaxValue,
                J1_Max = double.MinValue,
                J2_Min = double.MaxValue,
                J2_Max = double.MinValue,
                J3_Min = double.MaxValue,
                J3_Max = double.MinValue,
                J4_Min = double.MaxValue,
                J4_Max = double.MinValue,
                J5_Min = double.MaxValue,
                J5_Max = double.MinValue,
                J6_Min = double.MaxValue,
                J6_Max = double.MinValue,
            };

            foreach (var pose in poses)
            {
                // J1
                if (pose.J1 < stats.J1_Min) stats.J1_Min = pose.J1;
                if (pose.J1 > stats.J1_Max) stats.J1_Max = pose.J1;

                // J2
                if (pose.J2 < stats.J2_Min) stats.J2_Min = pose.J2;
                if (pose.J2 > stats.J2_Max) stats.J2_Max = pose.J2;

                // J3
                if (pose.J3 < stats.J3_Min) stats.J3_Min = pose.J3;
                if (pose.J3 > stats.J3_Max) stats.J3_Max = pose.J3;

                // J4
                if (pose.J4 < stats.J4_Min) stats.J4_Min = pose.J4;
                if (pose.J4 > stats.J4_Max) stats.J4_Max = pose.J4;

                // J5
                if (pose.J5 < stats.J5_Min) stats.J5_Min = pose.J5;
                if (pose.J5 > stats.J5_Max) stats.J5_Max = pose.J5;
                if (pose.J5 < 0) stats.J5_NegCount++;
                else if (pose.J5 > 0) stats.J5_PosCount++;

                // J6
                if (pose.J6 < stats.J6_Min) stats.J6_Min = pose.J6;
                if (pose.J6 > stats.J6_Max) stats.J6_Max = pose.J6;
            }

            return stats;
        }
    }
}
