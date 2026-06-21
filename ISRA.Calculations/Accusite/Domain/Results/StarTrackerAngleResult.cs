namespace ISRA.Calculations.AccuSite.Domain.Results
{
    /// <summary>
    /// Result of star-tracker Z vector angle validation.
    /// Checks if star orientation relative to tracker is within acceptable ranges.
    /// </summary>
    public class StarTrackerAngleResult
    {
        /// <summary>Angle in XZ plane (Y axis rotation) in degrees</summary>
        public double AngleXZ { get; set; }

        /// <summary>Angle in YZ plane (X axis rotation) in degrees</summary>
        public double AngleYZ { get; set; }

        /// <summary>True if XZ angle is within tolerance (typically ±25°)</summary>
        public bool IsValidXZ { get; set; }

        /// <summary>True if YZ angle is within tolerance (typically ±40°)</summary>
        public bool IsValidYZ { get; set; }

        /// <summary>True if both angles are valid</summary>
        public bool IsValid { get; set; }

        /// <summary>Label for XZ angle status</summary>
        public string LabelXZ { get; set; }

        /// <summary>Label for YZ angle status</summary>
        public string LabelYZ { get; set; }

        /// <summary>Overall label</summary>
        public string Label { get; set; }
    }
}
