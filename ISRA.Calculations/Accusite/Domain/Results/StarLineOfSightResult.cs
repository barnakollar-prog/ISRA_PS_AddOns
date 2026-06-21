namespace ISRA.Calculations.AccuSite.Domain.Results
{
    /// <summary>
    /// Complete line-of-sight check result for a star (all cameras).
    /// </summary>
    public class StarLineOfSightResult
    {
        /// <summary>
        /// Array of line-of-sight results for each camera.
        /// </summary>
        public LineOfSightResult[] CameraResults { get; set; }

        /// <summary>
        /// True if all cameras have clear line-of-sight to the star.
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Overall status label.
        /// </summary>
        public string Label { get; set; }
    }
}
