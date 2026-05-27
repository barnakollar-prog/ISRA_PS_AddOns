using System;
using Tecnomatix.Engineering;

namespace ISRA.Components.AccuSite.Trackers
{
    /// <summary>
    /// Defines the FOV geometry and camera positions of tracker 920-0005.
    /// All coordinates are local to the tracker self origin.
    /// MiddleField = Z origin (Z=0)
    /// </summary>
    public class tracker_920_0005
    {
        // ── FOV field definitions (local coordinates in mm) ───────

        private const double NearZ = -803;
        private const double NearXMax = 278;
        private const double NearYMax = 265;

        private const double MidZ = 0;
        private const double MidXMax = 850;
        private const double MidYMax = 440;

        private const double FarZ = 1200;
        private const double FarXMax = 1670;
        private const double FarYMax = 870;

        // ── Position zone constants ───────────────────────────────
        public const double ZoneOptimalMin = 0;
        public const double ZoneWarnMin = -803;

        // ── Position zone enum ────────────────────────────────────
        public enum PositionZone
        {
            Optimal,  // Z > 0
            Warning,  // -803 < Z <= 0
            NOK       // Z < -803
        }

        // ── Camera definitions ────────────────────────────────────
        // Position relative to tracker self origin (mm)

        public class CameraData
        {
            public string Name { get; set; }
            public TxVector Position { get; set; }
        }

        /// <summary>
        /// Returns the 3 camera definitions in tracker local coordinate system.
        /// </summary>
        public static CameraData[] GetCameras()
        {
            return new CameraData[]
            {
                new CameraData
                {
                    Name     = "Camera_1",
                    Position = new TxVector(524.06, 0.00, -1776.25)
                },
                new CameraData
                {
                    Name     = "Camera_2",
                    Position = new TxVector(0.00, 0.00, -1776.50)
                },
                new CameraData
                {
                    Name     = "Camera_3",
                    Position = new TxVector(-525.50, 0.00, -1776.25)
                }
            };
        }

        /// <summary>
        /// Returns the world position of a specific camera.
        /// </summary>
        public static TxVector GetCameraWorldPosition(
            TxTransformation trackerWorld, CameraData camera)
        {
            return trackerWorld.Transform(camera.Position);
        }

        // ── Position zone methods ─────────────────────────────────

        /// <summary>
        /// Returns the position zone of a star based on its
        /// local Z coordinate relative to tracker origin.
        /// </summary>
        public PositionZone GetPositionZone(TxVector localPoint)
        {
            if (localPoint.Z > ZoneOptimalMin)
                return PositionZone.Optimal;
            else if (localPoint.Z > ZoneWarnMin)
                return PositionZone.Warning;
            else
                return PositionZone.NOK;
        }

        /// <summary>
        /// Returns a display string for the position zone.
        /// </summary>
        public string GetPositionZoneLabel(TxVector localPoint)
        {
            switch (GetPositionZone(localPoint))
            {
                case PositionZone.Optimal: return "OK Optimal";
                case PositionZone.Warning: return "WARN Near Field";
                default: return "NOK";
            }
        }

        /// <summary>
        /// Returns the name of the FOV zone at a given local point.
        /// </summary>
        public string GetZoneName(TxVector localPoint)
        {
            if (localPoint.Z < NearZ) return "Before Near Field";
            if (localPoint.Z > FarZ) return "Beyond Far Field";
            if (localPoint.Z <= MidZ) return "Near-Mid Zone";
            return "Mid-Far Zone";
        }

        /// <summary>
        /// Transforms a world-space point into tracker local coordinate system.
        /// </summary>
        public TxVector ToLocalCoordinates(TxVector worldPoint,
            TxTransformation trackerWorldTransform)
        {
            return trackerWorldTransform.Inverse.Transform(worldPoint);
        }

        /// <summary>
        /// Extracts the Z axis direction vector from a TxTransformation.
        /// </summary>
        public static TxVector GetZVector(TxTransformation tx)
        {
            return new TxVector(tx[0, 2], tx[1, 2], tx[2, 2]);
        }
    }
}