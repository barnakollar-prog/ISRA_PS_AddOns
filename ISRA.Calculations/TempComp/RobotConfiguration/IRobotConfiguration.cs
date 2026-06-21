using ISRA.Calculations.TempComp.Domain;

namespace ISRA.Calculations.TempComp.RobotConfiguration
{
    /// <summary>
    /// Interface for robot-specific configuration and calculations.
    /// Different robot brands (Fanuc, Kuka, ABB) calculate J2-3 angle differently.
    /// </summary>
    public interface IRobotConfiguration
    {
        /// <summary>
        /// Robot brand name (e.g., "Fanuc", "Kuka", "ABB").
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Calculates the combined J2-3 angle for this robot type.
        /// </summary>
        /// <param name="pose">Robot pose with joint values</param>
        /// <returns>J2-3 angle in degrees</returns>
        double CalculateJ23Angle(RobotPose pose);

        /// <summary>
        /// Normalizes an angle to ±180° range (shortest arc).
        /// </summary>
        double NormalizeAngle180(double angle);
    }
}
