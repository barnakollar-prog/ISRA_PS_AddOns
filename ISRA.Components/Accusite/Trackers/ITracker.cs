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
        /// Returns the position zone (Optimal, Warning, NOK) for a point in tracker local coordinates.
        /// </summary>
        PositionZone GetPositionZone(TxVector localPoint);
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
