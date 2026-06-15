using System;
using System.Collections;
using System.Collections.Generic;
using Tecnomatix.Engineering;

namespace ISRA.Calculations.TempComp
{
    public static class TempCompCalculations
    {
        // ── Data structures ───────────────────────────────────────

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
        // ── Robot type ────────────────────────────────────────

        public enum RobotType { Fanuc, Kuka, Abb }

        /// <summary>
        /// Calculates J2-3 angle based on robot type.
        /// </summary>
        public static double CalculateJ23Angle(RobotPose pose, RobotType robotType)
        {
            switch (robotType)
            {
                case RobotType.Fanuc:
                    return pose.J2 + pose.J3 + 90.0;
                case RobotType.Kuka:
                    return (-1.0) * pose.J3 + 180.0;
                case RobotType.Abb:
                    return (-1.0) * pose.J3 + 90.0;
                default:
                    return pose.J2 + pose.J3 + 90.0;
            }
        }

        /// <summary>
        /// Normalizes an angle to ±180° (shortest arc).
        /// ΔA = ((Δ+180) mod 360) − 180
        /// Example: −300° → ((−300+180) mod 360) − 180 = 240 − 180 = +60°
        /// </summary>
        public static double NormalizeAngle180(double angle)
        {
            double a = (angle + 180.0) % 360.0;
            if (a < 0) a += 360.0;   // C# modulo lehet negatív
            return a - 180.0;
        }

        /// <summary>
        /// Calculates J2-3 range (max - min) for a list of poses.
        /// </summary>
        public static double CalculateJ23Range(List<RobotPose> poses, RobotType robotType)
        {
            if (poses.Count == 0) return 0.0;

            double min = double.MaxValue;
            double max = double.MinValue;

            foreach (var p in poses)
            {
                double angle = CalculateJ23Angle(p, robotType);
                if (angle < min) min = angle;
                if (angle > max) max = angle;
            }

            return max - min;
        }

        /// <summary>
        /// Result of nearest TC point search for one body point.
        /// </summary>
        public class NearestTcResult
        {
            public RobotPose BodyPose { get; set; }
            public RobotPose NearestTcPose { get; set; }
            public double Distance { get; set; }
        }

        /// <summary>
        /// Summary statistics for a list of poses.
        /// </summary>
        public class PoseSummary
        {
            public double J1_Min { get; set; }
            public double J1_Max { get; set; }
            public double J2_Min { get; set; }
            public double J2_Max { get; set; }
            public double J3_Min { get; set; }
            public double J3_Max { get; set; }
            public double J4_Min { get; set; }
            public double J4_Max { get; set; }
            public double J5_Min { get; set; }
            public double J5_Max { get; set; }
            public double J6_Min { get; set; }
            public double J6_Max { get; set; }
            public int J5_NegCount { get; set; }
            public int J5_PosCount { get; set; }
        }

        // ── Weights for Euclidean distance ────────────────────────
        private const double W_J23 = 2.0;   // J2-3 domináns
        private const double W_J4 = 1.0;
        private const double W_J5 = 1.0;
        private const double W_J6 = 1.0;

        // ── PS API helper ─────────────────────────────────────────

        public static List<RobotPose> ReadPosesFromProgram(
          TxWeldOperation prog,
    TxRobot robot,
    string[] namePrefixes = null)  // null = nincs szűrés
        {
            var result = new List<RobotPose>();
            var locations = prog.GetAllDescendants(
                new TxTypeFilter(typeof(ITxRoboticLocationOperation)));

            foreach (ITxObject obj in locations)
            {
                var loc = obj as ITxRoboticLocationOperation;
                if (loc == null) continue;

                // Szűrés – ha van prefix megadva
                if (namePrefixes != null)
                {
                    if (!MeasurementPointFilter.IsMeasurementPoint(loc, namePrefixes))
                        continue;
                }

                try
                {
                    var poseData = robot.GetPoseAtLocation(loc);
                    if (poseData?.JointValues == null) continue;

                    var joints = poseData.JointValues;
                    if (joints.Count < 6) continue;

                    result.Add(new RobotPose
                    {
                        Name = loc.Name,
                        J1 = (double)joints[0] * (180.0 / Math.PI),
                        J2 = (double)joints[1] * (180.0 / Math.PI),
                        J3 = (double)joints[2] * (180.0 / Math.PI),
                        J4 = (double)joints[3] * (180.0 / Math.PI),
                        J5 = (double)joints[4] * (180.0 / Math.PI),
                        J6 = (double)joints[5] * (180.0 / Math.PI),
                    });
                }
                catch { }
            }
            return result;
        }

        // ── Summary statistics ────────────────────────────────────

        public static PoseSummary CalculateSummary(List<RobotPose> poses)
        {
            var s = new PoseSummary
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

            foreach (var p in poses)
            {
                if (p.J1 < s.J1_Min) s.J1_Min = p.J1;
                if (p.J1 > s.J1_Max) s.J1_Max = p.J1;
                if (p.J2 < s.J2_Min) s.J2_Min = p.J2;
                if (p.J2 > s.J2_Max) s.J2_Max = p.J2;
                if (p.J3 < s.J3_Min) s.J3_Min = p.J3;
                if (p.J3 > s.J3_Max) s.J3_Max = p.J3;
                if (p.J4 < s.J4_Min) s.J4_Min = p.J4;
                if (p.J4 > s.J4_Max) s.J4_Max = p.J4;
                if (p.J5 < s.J5_Min) s.J5_Min = p.J5;
                if (p.J5 > s.J5_Max) s.J5_Max = p.J5;
                if (p.J6 < s.J6_Min) s.J6_Min = p.J6;
                if (p.J6 > s.J6_Max) s.J6_Max = p.J6;
                if (p.J5 < 0) s.J5_NegCount++;
                else if (p.J5 > 0) s.J5_PosCount++;
            }

            return s;
        }

        // ── Criterion 1: J2-J3 Coverage ──────────────────────────

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
        // ── Criterion: J2-3 Angle Max/Min Coverage ────────────────

        public class J23AngleCoverageResult
        {
            public double BodyMax { get; set; }
            public double BodyMin { get; set; }
            public int CountMax { get; set; }
            public int CountMin { get; set; }
            public bool MaxOK { get; set; }
            public bool MinOK { get; set; }
        }

        public static J23AngleCoverageResult CheckJ23AngleCoverage(
            List<RobotPose> bodyPoses,
            List<RobotPose> tempCompPoses,
            RobotType robotType)
        {
            double bodyMax = double.MinValue;
            double bodyMin = double.MaxValue;

            foreach (var p in bodyPoses)
            {
                double angle = CalculateJ23Angle(p, robotType);
                if (angle > bodyMax) bodyMax = angle;
                if (angle < bodyMin) bodyMin = angle;
            }

            int countMax = 0, countMin = 0;
            foreach (var p in tempCompPoses)
            {
                double angle = CalculateJ23Angle(p, robotType);
                if (angle >= bodyMax) countMax++;
                if (angle <= bodyMin) countMin++;
            }

            return new J23AngleCoverageResult
            {
                BodyMax = bodyMax,
                BodyMin = bodyMin,
                CountMax = countMax,
                CountMin = countMin,
                MaxOK = countMax >= 2,
                MinOK = countMin >= 2
            };
        }

        // ── Criterion: J4/J5/J6 Max/Min individual ───────────────

        public class AxisCoverageResult
        {
            public double BodyMax { get; set; }
            public double TcMax { get; set; }
            public bool IsValid { get; set; }
        }

        public static AxisCoverageResult CheckAxisMaxCoverage(
            List<RobotPose> bodyPoses,
            List<RobotPose> tempCompPoses,
            Func<RobotPose, double> selector)
        {
            double bodyMax = double.MinValue;
            foreach (var p in bodyPoses)
            {
                double v = Math.Abs(selector(p));
                if (v > bodyMax) bodyMax = v;
            }

            double tcMax = double.MinValue;
            bool covered = false;
            foreach (var p in tempCompPoses)
            {
                double v = Math.Abs(selector(p));
                if (v > tcMax) tcMax = v;
                if (v >= bodyMax) covered = true;
            }

            return new AxisCoverageResult
            {
                BodyMax = bodyMax,
                TcMax = tcMax,
                IsValid = covered
            };
        }

        // ── Criterion 2: J2-J3 Spread ────────────────────────────

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
                    j2gaps.Add(string.Format("{0:F0} to {1:F0}", s, end));
            }

            for (double s = minJ3; s < maxJ3; s += stepSizeDeg)
            {
                double end = s + stepSizeDeg;
                bool covered = false;
                foreach (var p in tempCompPoses)
                    if (p.J3 >= s && p.J3 < end) { covered = true; break; }
                if (!covered)
                    j3gaps.Add(string.Format("{0:F0} to {1:F0}", s, end));
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

        // ── Criterion 5: J4/J5/J6 Max Coverage ───────────────────

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

        // ── Nearest TC Point (Euclidean distance) ─────────────────

        /// <summary>
        /// Normalizes J4/J6 difference for 360° rotational axes.
        /// </summary>
        private static double NormalizeAngleDiff(double diff)
        {
            diff = Math.Abs(diff) % 360.0;
            if (diff > 180.0) diff = 360.0 - diff;
            return diff;
        }

        /// <summary>
        /// Distance between two poses: Euclidean over (ΔJ2-3, ΔJ4, ΔJ5, ΔJ6).
        /// J4/J6 differences normalized to ±180° (shortest arc).
        /// </summary>
        public static double CalculateDistance(RobotPose a, RobotPose b, RobotType robotType)
        {
            double d23 = CalculateJ23Angle(a, robotType) - CalculateJ23Angle(b, robotType);
            double d4 = NormalizeAngle180(a.J4 - b.J4);
            double d5 = a.J5 - b.J5;
            double d6 = NormalizeAngle180(a.J6 - b.J6);

            return Math.Sqrt(
                W_J23 * d23 * d23 +
                W_J4 * d4 * d4 +
                W_J5 * d5 * d5 +
                W_J6 * d6 * d6);
        }

        public static List<NearestTcResult> FindNearestTcPoints(
            List<RobotPose> bodyPoses, List<RobotPose> tempCompPoses, RobotType robotType)
        {
            var results = new List<NearestTcResult>();

            foreach (var body in bodyPoses)
            {
                RobotPose nearest = null;
                double minDist = double.MaxValue;

                foreach (var tc in tempCompPoses)
                {
                    double dist = CalculateDistance(body, tc, robotType);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        nearest = tc;
                    }
                }

                results.Add(new NearestTcResult
                {
                    BodyPose = body,
                    NearestTcPose = nearest,
                    Distance = minDist
                });
            }

            return results;
        }
    }
}