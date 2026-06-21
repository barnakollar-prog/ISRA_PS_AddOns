namespace ISRA.Calculations.TempComp.Domain.Results
{
    /// <summary>
    /// Result of the nearest TC point search for a single body point.
    /// </summary>
    public class NearestTcResult
    {
        /// <summary>The body measurement point pose</summary>
        public RobotPose BodyPose { get; set; }

        /// <summary>The nearest temp comp pose found</summary>
        public RobotPose NearestTcPose { get; set; }

        /// <summary>Euclidean distance between the poses</summary>
        public double Distance { get; set; }
    }
}
