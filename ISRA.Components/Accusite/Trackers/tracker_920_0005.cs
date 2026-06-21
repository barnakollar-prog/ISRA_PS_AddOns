using System;
using Tecnomatix.Engineering;

namespace ISRA.Components.AccuSite.Trackers
{
    /// <summary>
    /// Defines the FOV geometry and camera positions of tracker 920-0005.
    /// All coordinates are local to the tracker self origin.
    /// MiddleField = Z origin (Z=0)
    /// </summary>
    public class Tracker920_0005 : ITracker
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

        // ── ITracker implementation ───────────────────────────────

        /// <summary>
        /// Returns the 3 camera definitions in tracker local coordinate system.
        /// </summary>
        public CameraData[] GetCameras()
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
        public TxVector GetCameraWorldPosition(
            TxTransformation trackerWorld, CameraData camera)
        {
            return trackerWorld.Transform(camera.Position);
        }

        /// <summary>
        /// Checks if a point (in tracker local coordinates) is within the Field of View.
        /// Uses linear interpolation between Near, Mid, and Far field boundaries.
        /// </summary>
        public bool IsInFOV(TxVector localPoint)
        {
            double z = localPoint.Z;
            double x = Math.Abs(localPoint.X);
            double y = Math.Abs(localPoint.Y);

            // Outside Z range
            if (z < NearZ || z > FarZ) return false;

            // Interpolate X and Y limits based on Z position
            double xMax, yMax;

            if (z <= MidZ)
            {
                // Between Near and Mid
                double t = (z - NearZ) / (MidZ - NearZ);
                xMax = NearXMax + t * (MidXMax - NearXMax);
                yMax = NearYMax + t * (MidYMax - NearYMax);
            }
            else
            {
                // Between Mid and Far
                double t = (z - MidZ) / (FarZ - MidZ);
                xMax = MidXMax + t * (FarXMax - MidXMax);
                yMax = MidYMax + t * (FarYMax - MidYMax);
            }

            return x <= xMax && y <= yMax;
        }

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

    /// <summary>
    /// Backward compatibility: static wrapper for tracker_920_0005.
    /// Legacy code can continue using tracker_920_0005.GetCameras() etc.
    /// </summary>
    public static class tracker_920_0005
    {
        private static readonly Tracker920_0005 _instance = new Tracker920_0005();

        public static CameraData[] GetCameras() => _instance.GetCameras();

        public static TxVector GetCameraWorldPosition(TxTransformation trackerWorld, CameraData camera)
            => _instance.GetCameraWorldPosition(trackerWorld, camera);

        // Re-export PositionZone enum for backward compatibility
        public enum PositionZone
        {
            Optimal = Trackers.PositionZone.Optimal,
            Warning = Trackers.PositionZone.Warning,
            NOK = Trackers.PositionZone.NOK
        }
    }
}