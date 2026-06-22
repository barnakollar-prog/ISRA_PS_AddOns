using System;
using System.Collections.Generic;
using Tecnomatix.Engineering;
using ISRA.Calculations.TempComp.Domain;

namespace ISRA.Calculations.TempComp.Services
{
    /// <summary>
    /// Service for reading robot poses from Process Studio programs.
    /// </summary>
    public class PoseReader
    {
        private readonly TxRobot _robot;

        public PoseReader(TxRobot robot)
        {
            _robot = robot ?? throw new ArgumentNullException(nameof(robot));
        }

        /// <summary>
        /// Reads poses from a single weld operation program.
        /// </summary>
        /// <param name="program">Weld operation to read from</param>
        /// <param name="nameFilters">Optional name prefixes to filter measurement points</param>
        /// <returns>List of robot poses</returns>
        public List<RobotPose> ReadPosesFromProgram(TxWeldOperation program, string[] nameFilters = null)
        {
            var result = new List<RobotPose>();

            if (program == null)
                return result;

            string pathName = program.Name; // Capture the path name

            var locations = program.GetAllDescendants(
                new TxTypeFilter(typeof(ITxRoboticLocationOperation)));

            foreach (ITxObject obj in locations)
            {
                var loc = obj as ITxRoboticLocationOperation;
                if (loc == null) continue;

                // Filter by name prefix if specified
                if (nameFilters != null && nameFilters.Length > 0)
                {
                    if (!MeasurementPointFilter.IsMeasurementPoint(loc, nameFilters))
                        continue;
                }

                try
                {
                    var poseData = _robot.GetPoseAtLocation(loc);
                    if (poseData?.JointValues == null) continue;

                    var joints = poseData.JointValues;
                    if (joints.Count < 6) continue;

                    result.Add(new RobotPose
                    {
                        Name = loc.Name,
                        PathName = pathName, // Set the parent path name
                        J1 = (double)joints[0] * (180.0 / Math.PI),
                        J2 = (double)joints[1] * (180.0 / Math.PI),
                        J3 = (double)joints[2] * (180.0 / Math.PI),
                        J4 = (double)joints[3] * (180.0 / Math.PI),
                        J5 = (double)joints[4] * (180.0 / Math.PI),
                        J6 = (double)joints[5] * (180.0 / Math.PI),
                    });
                }
                catch
                {
                    // Silently skip invalid locations
                }
            }

            return result;
        }

        /// <summary>
        /// Reads poses from multiple weld operation programs.
        /// </summary>
        /// <param name="programs">Collection of weld operations</param>
        /// <param name="nameFilters">Optional name prefixes to filter measurement points</param>
        /// <returns>Aggregated list of robot poses from all programs</returns>
        public List<RobotPose> ReadPosesFromPrograms(
            IEnumerable<TxWeldOperation> programs,
            string[] nameFilters = null)
        {
            var result = new List<RobotPose>();

            if (programs == null)
                return result;

            foreach (var program in programs)
            {
                result.AddRange(ReadPosesFromProgram(program, nameFilters));
            }

            return result;
        }
    }
}
