using System;

namespace ISRA.Calculations.TempComp.Domain
{
    /// <summary>
    /// Represents a robot pose with 6 joint values (in degrees).
    /// </summary>
    public class RobotPose
    {
        /// <summary>
        /// Name/identifier of the pose (e.g., location operation name).
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Name of the parent path/program (WeldOperation) this pose belongs to.
        /// </summary>
        public string PathName { get; set; }

        /// <summary>
        /// Optional robot configuration signature captured from operation/pose metadata.
        /// </summary>
        public string ConfigurationSignature { get; set; }

        /// <summary>Joint 1 angle in degrees</summary>
        public double J1 { get; set; }

        /// <summary>Joint 2 angle in degrees</summary>
        public double J2 { get; set; }

        /// <summary>Joint 3 angle in degrees</summary>
        public double J3 { get; set; }

        /// <summary>Joint 4 angle in degrees</summary>
        public double J4 { get; set; }

        /// <summary>Joint 5 angle in degrees</summary>
        public double J5 { get; set; }

        /// <summary>Joint 6 angle in degrees</summary>
        public double J6 { get; set; }

        public RobotPose()
        {
            Name = string.Empty;
            PathName = string.Empty;
            ConfigurationSignature = string.Empty;
        }

        public RobotPose(string name, double j1, double j2, double j3, double j4, double j5, double j6)
        {
            Name = name;
            J1 = j1;
            J2 = j2;
            J3 = j3;
            J4 = j4;
            J5 = j5;
            J6 = j6;
        }

        /// <summary>
        /// Returns a string representation of the pose.
        /// </summary>
        public override string ToString()
        {
            return $"{Name}: J1={J1:F2}, J2={J2:F2}, J3={J3:F2}, J4={J4:F2}, J5={J5:F2}, J6={J6:F2}";
        }

        public string GetLocationKey()
        {
            return BuildLocationKey(PathName, Name);
        }

        public static string BuildLocationKey(string pathName, string locationName)
        {
            var path = pathName ?? string.Empty;
            var name = locationName ?? string.Empty;
            return string.Concat(path, "::", name);
        }

        public bool IsConfigurationEqual(RobotPose other)
        {
            if (other == null)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(ConfigurationSignature)
                || string.IsNullOrWhiteSpace(other.ConfigurationSignature))
            {
                return false;
            }

            return string.Equals(
                ConfigurationSignature,
                other.ConfigurationSignature,
                StringComparison.OrdinalIgnoreCase);
        }

        public bool HasJointDifference(RobotPose other, double toleranceDeg)
        {
            if (other == null)
            {
                return true;
            }

            return Math.Abs(J1 - other.J1) > toleranceDeg
                || Math.Abs(J2 - other.J2) > toleranceDeg
                || Math.Abs(J3 - other.J3) > toleranceDeg
                || Math.Abs(J4 - other.J4) > toleranceDeg
                || Math.Abs(J5 - other.J5) > toleranceDeg
                || Math.Abs(J6 - other.J6) > toleranceDeg;
        }

        public bool HasDifference(RobotPose other, double toleranceDeg, bool preferConfiguration)
        {
            if (other == null)
            {
                return true;
            }

            if (preferConfiguration)
            {
                bool hasConfigA = !string.IsNullOrWhiteSpace(ConfigurationSignature);
                bool hasConfigB = !string.IsNullOrWhiteSpace(other.ConfigurationSignature);

                if (hasConfigA && hasConfigB)
                {
                    return !IsConfigurationEqual(other);
                }
            }

            return HasJointDifference(other, toleranceDeg);
        }
    }
}
