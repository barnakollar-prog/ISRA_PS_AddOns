using Tecnomatix.Engineering;

namespace ISRA.Components.AccuSite.Trackers
{
    /// <summary>
    /// Interface for AccuSite tracker components.
    /// </summary>
    public interface ITracker
    {
        /// <summary>
        /// Returns all camera definitions for this tracker type.
        /// </summary>
        CameraData[] GetCameras();

        /// <summary>
        /// Returns the world position of a specific camera.
        /// </summary>
        TxVector GetCameraWorldPosition(TxTransformation trackerWorld, CameraData camera);

        /// <summary>
        /// Checks if a point (in tracker local coordinates) is within the Field of View.
        /// </summary>
        bool IsInFOV(TxVector localPoint);

        /// <summary>
        /// Checks if a point (in tracker local coordinates) is within the Field of View,
        /// after growing the FOV's X/Y/Z dimensions by <paramref name="fovScalePercent"/> percent
        /// (0-10, relaxing the already-reduced design FOV). Near/Far Z boundaries are
        /// moved away from the Mid plane by the same percentage.
        /// </summary>
        bool IsInFOV(TxVector localPoint, double fovScalePercent);

        /// <summary>
        /// Returns the position zone (Optimal, Warning, NOK) for a point in tracker local coordinates.
        /// </summary>
        PositionZone GetPositionZone(TxVector localPoint);

        /// <summary>
        /// Returns the FOV boundary zones (Near/Mid/Far) in tracker local coordinates,
        /// with X/Y/Z dimensions grown by <paramref name="fovScalePercent"/> percent (0-10).
        /// Used to build a visual wireframe of the (scaled) FOV volume.
        /// </summary>
        FovZone[] GetFovZones(double fovScalePercent);
    }

    /// <summary>
    /// Represents one Z-plane boundary of a tracker's Field of View (Near/Mid/Far),
    /// expressed as a rectangle half-width/half-height in local coordinates.
    /// </summary>
    public class FovZone
    {
        public string Name { get; set; }
        public double Z { get; set; }
        public double XMax { get; set; }
        public double YMax { get; set; }
    }

    /// <summary>
    /// Represents a camera on a tracker with position.
    /// </summary>
    public class CameraData
    {
        /// <summary>Camera name (e.g., "Camera_1")</summary>
        public string Name { get; set; }

        /// <summary>Camera position relative to tracker origin (mm)</summary>
        public TxVector Position { get; set; }
    }

    /// <summary>
    /// Position zone classification for star placement.
    /// </summary>
    public enum PositionZone
    {
        /// <summary>Optimal zone (Z > 0)</summary>
        Optimal,

        /// <summary>Warning zone (-803 < Z <= 0)</summary>
        Warning,

        /// <summary>Not OK zone (Z < -803)</summary>
        NOK
    }
}
