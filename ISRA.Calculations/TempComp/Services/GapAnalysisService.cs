using System;
using System.Collections.Generic;
using ISRA.Calculations.TempComp.Domain;
using ISRA.Calculations.TempComp.Domain.Results;
using ISRA.Calculations.TempComp.RobotConfiguration;

namespace ISRA.Calculations.TempComp.Services
{
    public class GapAnalysisService
    {
        public GapAnalysisResult Analyze(
            List<RobotPose> tcPoses,
            IRobotConfiguration config,
            double j2Threshold = 25,
            double j3Threshold = 25,
            double j4Threshold = 25,
            double j5Threshold = 25,
            double j6Threshold = 25)
        {
            var result = new GapAnalysisResult();

            result.J2.Threshold = j2Threshold;
            result.J3.Threshold = j3Threshold;
            result.J4.Threshold = j4Threshold;
            result.J5.Threshold = j5Threshold;
            result.J6.Threshold = j6Threshold;
            result.J23.Threshold = 0;

            // Collect (value, poseName, pathName) per axis
            var j2Data = new List<(double, string, string)>();
            var j3Data = new List<(double, string, string)>();
            var j4Data = new List<(double, string, string)>();
            var j5Data = new List<(double, string, string)>();
            var j6Data = new List<(double, string, string)>();
            var j23Data = new List<(double, string, string)>();

            foreach (var pose in tcPoses)
            {
                string name = pose.Name ?? "";
                string path = pose.PathName ?? "";

                j2Data.Add((pose.J2, name, path));
                j3Data.Add((pose.J3, name, path));
                j4Data.Add((pose.J4, name, path));
                j5Data.Add((pose.J5, name, path));
                j6Data.Add((pose.J6, name, path));
                j23Data.Add((config.CalculateJ23Angle(pose), name, path));
            }

            CalculateAxis(result.J2, j2Data);
            CalculateAxis(result.J3, j3Data);
            CalculateAxis(result.J4, j4Data);
            CalculateAxis(result.J5, j5Data);
            CalculateAxis(result.J6, j6Data);
            CalculateAxis(result.J23, j23Data);

            return result;
        }

        private void CalculateAxis(
            AxisGapResult axisResult,
            List<(double Value, string PoseName, string PathName)> data)
        {
            if (data == null || data.Count == 0)
                return;

            // Sort by value ascending
            data.Sort((a, b) => a.Value.CompareTo(b.Value));
            axisResult.SortedValues.AddRange(data);

            double maxGap = 0;
            for (int i = 0; i < data.Count - 1; i++)
            {
                double gap = data[i + 1].Value - data[i].Value;
                if (gap > maxGap)
                    maxGap = gap;

                if (axisResult.Threshold > 0 && gap > axisResult.Threshold)
                    axisResult.GapIndices.Add(i);
            }

            axisResult.MaxGap = maxGap;
        }
    }
}