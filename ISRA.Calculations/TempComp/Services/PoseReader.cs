using System;
using System.Collections.Generic;
using Tecnomatix.Engineering;
using ISRA.Calculations.TempComp.Domain;

namespace ISRA.Calculations.TempComp.Services
{
    public class PoseReader
    {
        private readonly TxRobot _robot;

        public PoseReader(TxRobot robot)
        {
            _robot = robot ?? throw new ArgumentNullException(nameof(robot));
        }

        public List<RobotPose> ReadPosesFromProgram(
            TxWeldOperation program,
            FilterMode filterMode,
            string[] namePrefixes,
            string[] olpKeywords = null)
        {
            var result = new List<RobotPose>();

            if (program == null)
                return result;

            string pathName = program.Name;

            var locations = program.GetAllDescendants(
                new TxTypeFilter(typeof(ITxRoboticLocationOperation)));

            foreach (ITxObject obj in locations)
            {
                var loc = obj as ITxRoboticLocationOperation;
                if (loc == null) continue;

                // Apply filter based on mode
                if (filterMode == FilterMode.Auto)
                {
                    if (!MeasurementPointFilter.IsMeasurementPoint(loc, namePrefixes))
                        continue;
                }
                else if (filterMode == FilterMode.Custom)
                {
                    if (!MeasurementPointFilter.IsMeasurementPoint(loc, namePrefixes, olpKeywords))
                        continue;
                }
                // NoFilter: minden location bekerül

                try
                {
                    var poseData = _robot.GetPoseAtLocation(loc);
                    if (poseData?.JointValues == null) continue;

                    var joints = poseData.JointValues;
                    if (joints.Count < 6) continue;

                    result.Add(new RobotPose
                    {
                        Name = loc.Name,
                        PathName = pathName,
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

        public List<RobotPose> ReadPosesFromPrograms(
            IEnumerable<TxWeldOperation> programs,
            FilterMode filterMode,
            string[] namePrefixes,
            string[] olpKeywords = null)
        {
            var result = new List<RobotPose>();

            if (programs == null)
                return result;

            foreach (var program in programs)
            {
                result.AddRange(ReadPosesFromProgram(program, filterMode, namePrefixes, olpKeywords));
            }

            return result;
        }
    }
}