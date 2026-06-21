using System;
using System.Collections.Generic;
using ISRA.Calculations.TempComp.Domain;
using ISRA.Calculations.TempComp.Domain.Results;
using ISRA.Calculations.TempComp.RobotConfiguration;

namespace ISRA.Calculations.TempComp.Services
{
    /// <summary>
    /// Service for calculating weighted Euclidean distance between robot poses.
    /// </summary>
    public class DistanceCalculator
    {
        private readonly IRobotConfiguration _robotConfig;

        // Weights for Euclidean distance calculation
        private const double W_J23 = 2.0;   // J2-3 angle is dominant
        private const double W_J4 = 1.0;
        private const double W_J5 = 1.0;
        private const double W_J6 = 1.0;

        public DistanceCalculator(IRobotConfiguration robotConfig)
        {
            _robotConfig = robotConfig ?? throw new ArgumentNullException(nameof(robotConfig));
        }

        /// <summary>
        /// Calculates weighted Euclidean distance between two poses.
        /// Distance is calculated over (ΔJ2-3, ΔJ4, ΔJ5, ΔJ6).
        /// J4 and J6 differences are normalized to ±180° (shortest arc).
        /// </summary>
        public double CalculateDistance(RobotPose a, RobotPose b)
        {
            double d23 = _robotConfig.CalculateJ23Angle(a) - _robotConfig.CalculateJ23Angle(b);
            double d4 = _robotConfig.NormalizeAngle180(a.J4 - b.J4);
            double d5 = a.J5 - b.J5;
            double d6 = _robotConfig.NormalizeAngle180(a.J6 - b.J6);

            return Math.Sqrt(
                W_J23 * d23 * d23 +
                W_J4 * d4 * d4 +
                W_J5 * d5 * d5 +
                W_J6 * d6 * d6);
        }

        /// <summary>
        /// Finds the nearest temp comp pose for a single body pose.
        /// </summary>
        public NearestTcResult FindNearest(RobotPose bodyPose, List<RobotPose> tempCompPoses)
        {
            if (tempCompPoses == null || tempCompPoses.Count == 0)
            {
                return new NearestTcResult
                {
                    BodyPose = bodyPose,
                    NearestTcPose = null,
                    Distance = double.MaxValue
                };
            }

            RobotPose nearest = null;
            double minDistance = double.MaxValue;

            foreach (var tcPose in tempCompPoses)
            {
                double distance = CalculateDistance(bodyPose, tcPose);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = tcPose;
                }
            }

            return new NearestTcResult
            {
                BodyPose = bodyPose,
                NearestTcPose = nearest,
                Distance = minDistance
            };
        }

        /// <summary>
        /// Finds the nearest temp comp pose for each body pose.
        /// </summary>
        public List<NearestTcResult> FindNearestForAll(
            List<RobotPose> bodyPoses,
            List<RobotPose> tempCompPoses)
        {
            var results = new List<NearestTcResult>();

            if (bodyPoses == null)
                return results;

            foreach (var bodyPose in bodyPoses)
            {
                results.Add(FindNearest(bodyPose, tempCompPoses));
            }

            return results;
        }
    }
}
