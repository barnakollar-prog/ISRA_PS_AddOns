namespace ISRA.Calculations.AccuSite.Domain.Results
{
    /// <summary>
    /// Result of visibility check for a single emitter from a single camera.
    /// </summary>
    public class EmitterVisibilityResult
    {
        /// <summary>Name of the emitter (e.g., "Emitter_1")</summary>
        public string EmitterName { get; set; }

        /// <summary>Name of the camera (e.g., "Camera_1")</summary>
        public string CameraName { get; set; }

        /// <summary>Angle between emitter direction and camera direction (degrees)</summary>
        public double AngleDeg { get; set; }

        /// <summary>True if angle is within acceptable range (emitter is visible)</summary>
        public bool IsVisible { get; set; }

        /// <summary>Human-readable label for display</summary>
        public string Label { get; set; }
    }
}
