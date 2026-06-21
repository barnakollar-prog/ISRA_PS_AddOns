namespace ISRA.Calculations.AccuSite.Domain.Results
{
    /// <summary>
    /// Complete visibility analysis result for a star (all emitters vs all cameras).
    /// </summary>
    public class StarEmitterVisibilityResult
    {
        /// <summary>
        /// 2D array of visibility results [emitterIndex, cameraIndex].
        /// </summary>
        public EmitterVisibilityResult[,] Results { get; set; }

        /// <summary>
        /// Boolean array indicating which emitters are visible from ALL cameras.
        /// </summary>
        public bool[] EmitterVisibleFromAllCameras { get; set; }

        /// <summary>
        /// Count of emitters visible from all cameras.
        /// </summary>
        public int VisibleEmitterCount { get; set; }

        /// <summary>
        /// True if at least 3 emitters are visible from all cameras.
        /// </summary>
        public bool IsValid { get; set; }
    }
}
