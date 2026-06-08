using System;
using System.Collections;
using System.Collections.Generic;
using Tecnomatix.Engineering;

namespace ISRA.Calculations.TempComp
{
    /// <summary>
    /// Calculations and validation for Temperature Compensation paths.
    /// </summary>
    public static class TempCompCalculations
    {
        // ── Data structures ───────────────────────────────────────

        /// <summary>
        /// Represents a robot pose with J1-J6 axis values in degrees.
        /// </summary>
        public class RobotPose
        {
            public string Name { get; set; }
            public double J1 { get; set; }
            public double J2 { get; set; }
            public double J3 { get; set; }
            public double J4 { get; set; }
            public double J5 { get; set; }
            public double J6 { get; set; }
        }

        // ── PS API helpers ────────────────────────────────────────

        /// <summary>
        /// Reads all robot poses from a robotic program.
        /// </summary>
        public static List<RobotPose> ReadPosesFromProgram(
            TxRoboticProgram program, TxRobot robot)
        {
            var poses = new List<RobotPose>();

            var elements = program.GetDirectChildElements(
                new TxTypeFilter(typeof(ITxRoboticLocationOperation)));

            if (elements == null) return poses;

            foreach (ITxObject obj in elements)
            {
                var loc = obj as ITxRoboticLocationOperation;
                if (loc == null) continue;

                try
                {
                    TxPoseData pose = robot.GetPoseAtLocation(loc);
                    if (pose == null) continue;

                    ArrayList joints = pose.JointValues;
                    if (joints == null || joints.Count < 6) continue;

                    poses.Add(new RobotPose
                    {
                        Name = loc.Name,
                        J1 = Convert.ToDouble(joints[0]),
                        J2 = Convert.ToDouble(joints[1]),
                        J3 = Convert.ToDouble(joints[2]),
                        J4 = Convert.ToDouble(joints[3]),
                        J5 = Convert.ToDouble(joints[4]),
                        J6 = Convert.ToDouble(joints[5])
                    });
                }
                catch { }
            }

            return poses;
        }

        // ── Criterion 1: J2-J3 Max Coverage ──────────────────────

        public class CoverageResult
        {
            public double MaxJ2_Body { get; set; }
            public double MaxJ3_Body { get; set; }
            public int CountJ2 { get; set; }
            public int CountJ3 { get; set; }
            public bool J2_OK { get; set; }
            public bool J3_OK { get; set; }
            public bool IsValid { get; set; }
        }

        public static CoverageResult CheckJ2J3Coverage(
            List<RobotPose> bodyPoses, List<RobotPose> tempCompPoses)
        {
            double maxJ2 = double.MinValue;
            double maxJ3 = double.MinValue;

            foreach (var p in bodyPoses)
            {
                if (p.J2 > maxJ2) maxJ2 = p.J2;
                if (p.J3 > maxJ3) maxJ3 = p.J3;
            }

            int countJ2 = 0, countJ3 = 0;
            foreach (var p in tempCompPoses)
            {
                if (p.J2 >= maxJ2) countJ2++;
                if (p.J3 >= maxJ3) countJ3++;
            }

            bool j2ok = countJ2 >= 2;
            bool j3ok = countJ3 >= 2;

            return new CoverageResult
            {
                MaxJ2_Body = maxJ2,
                MaxJ3_Body = maxJ3,
                CountJ2 = countJ2,
                CountJ3 = countJ3,
                J2_OK = j2ok,
                J3_OK = j3ok,
                IsValid = j2ok && j3ok
            };
        }

        // ── Criterion 2: J2-J3 Angular Spread ────────────────────

        public class SpreadResult
        {
            public double SpreadJ2 { get; set; }
            public double SpreadJ3 { get; set; }
            public bool J2_OK { get; set; }
            public bool J3_OK { get; set; }
            public bool IsValid { get; set; }
        }

        public static SpreadResult CheckJ2J3Spread(List<RobotPose> tempCompPoses)
        {
            double minJ2 = double.MaxValue, maxJ2 = double.MinValue;
            double minJ3 = double.MaxValue, maxJ3 = double.MinValue;

            foreach (var p in tempCompPoses)
            {
                if (p.J2 < minJ2) minJ2 = p.J2;
                if (p.J2 > maxJ2) maxJ2 = p.J2;
                if (p.J3 < minJ3) minJ3 = p.J3;
                if (p.J3 > maxJ3) maxJ3 = p.J3;
            }

            double spreadJ2 = maxJ2 - minJ2;
            double spreadJ3 = maxJ3 - minJ3;

            bool j2ok = spreadJ2 >= 75.0;
            bool j3ok = spreadJ3 >= 75.0;

            return new SpreadResult
            {
                SpreadJ2 = spreadJ2,
                SpreadJ3 = spreadJ3,
                J2_OK = j2ok,
                J3_OK = j3ok,
                IsValid = j2ok && j3ok
            };
        }

        // ── Criterion 3: J5 Symmetry ──────────────────────────────

        public class SymmetryResult
        {
            public int NegCount { get; set; }
            public int PosCount { get; set; }
            public int Total { get; set; }
            public bool IsValid { get; set; }
        }

        public static SymmetryResult CheckJ5Symmetry(List<RobotPose> tempCompPoses)
        {
            int neg = 0, pos = 0;
            foreach (var p in tempCompPoses)
            {
                if (p.J5 < 0) neg++;
                else if (p.J5 > 0) pos++;
            }

            int total = tempCompPoses.Count;
            int half = total / 2;

            return new SymmetryResult
            {
                NegCount = neg,
                PosCount = pos,
                Total = total,
                IsValid = neg >= half && pos >= half
            };
        }

        // ── Criterion 4: J2-J3 Step Coverage ─────────────────────

        public class StepCoverageResult
        {
            public List<string> J2_Gaps { get; set; }
            public List<string> J3_Gaps { get; set; }
            public bool J2_OK { get; set; }
            public bool J3_OK { get; set; }
            public bool IsValid { get; set; }
        }

        public static StepCoverageResult CheckJ2J3StepCoverage(
            List<RobotPose> bodyPoses,
            List<RobotPose> tempCompPoses,
            double stepSizeDeg = 30.0)
        {
            double minJ2 = double.MaxValue, maxJ2 = double.MinValue;
            double minJ3 = double.MaxValue, maxJ3 = double.MinValue;

            foreach (var p in bodyPoses)
            {
                if (p.J2 < minJ2) minJ2 = p.J2;
                if (p.J2 > maxJ2) maxJ2 = p.J2;
                if (p.J3 < minJ3) minJ3 = p.J3;
                if (p.J3 > maxJ3) maxJ3 = p.J3;
            }

            var j2gaps = new List<string>();
            var j3gaps = new List<string>();

            for (double s = minJ2; s < maxJ2; s += stepSizeDeg)
            {
                double end = s + stepSizeDeg;
                bool covered = false;
                foreach (var p in tempCompPoses)
                    if (p.J2 >= s && p.J2 < end) { covered = true; break; }
                if (!covered)
                    j2gaps.Add(string.Format("{0:F0} to {1:F0} deg", s, end));
            }

            for (double s = minJ3; s < maxJ3; s += stepSizeDeg)
            {
                double end = s + stepSizeDeg;
                bool covered = false;
                foreach (var p in tempCompPoses)
                    if (p.J3 >= s && p.J3 < end) { covered = true; break; }
                if (!covered)
                    j3gaps.Add(string.Format("{0:F0} to {1:F0} deg", s, end));
            }

            return new StepCoverageResult
            {
                J2_Gaps = j2gaps,
                J3_Gaps = j3gaps,
                J2_OK = j2gaps.Count == 0,
                J3_OK = j3gaps.Count == 0,
                IsValid = j2gaps.Count == 0 && j3gaps.Count == 0
            };
        }

        // ── Criterion 5: J4, J5, J6 Max Coverage ─────────────────

        public class MaxCoverageResult
        {
            public double MaxJ4_Body { get; set; }
            public double MaxJ5Pos_Body { get; set; }
            public double MaxJ5Neg_Body { get; set; }
            public double MaxJ6_Body { get; set; }
            public bool J4_OK { get; set; }
            public bool J5_OK { get; set; }
            public bool J6_OK { get; set; }
            public bool IsValid { get; set; }
        }

        public static MaxCoverageResult CheckJ456MaxCoverage(
            List<RobotPose> bodyPoses, List<RobotPose> tempCompPoses)
        {
            double maxJ4 = double.MinValue;
            double maxJ5pos = double.MinValue;
            double maxJ5neg = double.MaxValue;
            double maxJ6 = double.MinValue;

            foreach (var p in bodyPoses)
            {
                if (Math.Abs(p.J4) > maxJ4) maxJ4 = Math.Abs(p.J4);
                if (p.J5 > 0 && p.J5 > maxJ5pos) maxJ5pos = p.J5;
                if (p.J5 < 0 && p.J5 < maxJ5neg) maxJ5neg = p.J5;
                if (Math.Abs(p.J6) > maxJ6) maxJ6 = Math.Abs(p.J6);
            }

            bool j4ok = false, j5posok = false, j5negok = false, j6ok = false;

            foreach (var p in tempCompPoses)
            {
                if (Math.Abs(p.J4) >= maxJ4) j4ok = true;
                if (p.J5 >= maxJ5pos) j5posok = true;
                if (p.J5 <= maxJ5neg) j5negok = true;
                if (Math.Abs(p.J6) >= maxJ6) j6ok = true;
            }

            return new MaxCoverageResult
            {
                MaxJ4_Body = maxJ4,
                MaxJ5Pos_Body = maxJ5pos,
                MaxJ5Neg_Body = maxJ5neg,
                MaxJ6_Body = maxJ6,
                J4_OK = j4ok,
                J5_OK = j5posok && j5negok,
                J6_OK = j6ok,
                IsValid = j4ok && j5posok && j5negok && j6ok
            };
        }
    }
}