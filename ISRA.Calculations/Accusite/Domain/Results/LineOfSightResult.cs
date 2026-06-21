namespace ISRA.Calculations.AccuSite.Domain.Results
{
    /// <summary>
    /// Result of line-of-sight check between a single camera and a star.
    /// </summary>
    public class LineOfSightResult
    {
        /// <summary>Name of the camera</summary>
        public string CameraName { get; set; }

        /// <summary>Name of the star</summary>
        public string StarName { get; set; }

        /// <summary>True if line-of-sight is blocked by geometry</summary>
        public bool IsBlocked { get; set; }

        /// <summary>Human-readable label</summary>
        public string Label { get; set; }
    }
}
