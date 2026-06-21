using ISRA.Calculations.TempComp.Domain;

namespace ISRA.Calculations.TempComp.RobotConfiguration
{
    /// <summary>
    /// Robot configuration for ABB robots.
    /// J2-3 angle = (-1.0) * J3 + 90°
    /// </summary>
    public class AbbConfiguration : IRobotConfiguration
    {
        public string Name => "ABB";

        public double CalculateJ23Angle(RobotPose pose)
        {
            return (-1.0) * pose.J3 + 90.0;
        }

        public double NormalizeAngle180(double angle)
        {
            double a = (angle + 180.0) % 360.0;
            if (a < 0) a += 360.0;
            return a - 180.0;
        }
    }
}
