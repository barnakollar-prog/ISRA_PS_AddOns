using ISRA.Calculations.TempComp.Domain;

namespace ISRA.Calculations.TempComp.RobotConfiguration
{
    /// <summary>
    /// Robot configuration for Kuka robots.
    /// J2-3 angle = (-1.0) * J3 + 180°
    /// </summary>
    public class KukaConfiguration : IRobotConfiguration
    {
        public string Name => "Kuka";

        public double CalculateJ23Angle(RobotPose pose)
        {
            return (-1.0) * pose.J3 + 180.0;
        }

        public double NormalizeAngle180(double angle)
        {
            double a = (angle + 180.0) % 360.0;
            if (a < 0) a += 360.0;
            return a - 180.0;
        }
    }
}
