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
            return IsInFOV(localPoint, 0.0);
        }

        /// <summary>
        /// Checks if a point (in tracker local coordinates) is within the Field of View,
        /// after growing the FOV's X/Y/Z dimensions by <paramref name="fovScalePercent"/> percent
        /// (0-10, relaxing the already-reduced design FOV). The Near/Far Z boundaries
        /// are moved away from the Mid plane by the same percentage.
        /// Uses linear interpolation between Near, Mid, and Far field boundaries.
        /// </summary>
        public bool IsInFOV(TxVector localPoint, double fovScalePercent)
        {
            double z = localPoint.Z;
            double x = Math.Abs(localPoint.X);
            double y = Math.Abs(localPoint.Y);

            double factor = GetScaleFactor(fovScalePercent);
            double nearZ = MidZ + (NearZ - MidZ) * factor;
            double farZ = MidZ + (FarZ - MidZ) * factor;

            // Outside Z range
            if (z < nearZ || z > farZ) return false;

            double xMax, yMax;
            GetFovLimitsAtZ(z, fovScalePercent, out xMax, out yMax);

            return x <= xMax && y <= yMax;
        }

        /// <summary>
        /// Returns the FOV boundary zones (Near/Mid/Far) in tracker local coordinates,
        /// with X/Y/Z dimensions grown by <paramref name="fovScalePercent"/> percent (0-10).
        /// Used to build a visual wireframe of the (scaled) FOV volume.
        /// </summary>
        public FovZone[] GetFovZones(double fovScalePercent)
        {
            double factor = GetScaleFactor(fovScalePercent);
            double nearZ = MidZ + (NearZ - MidZ) * factor;
            double farZ = MidZ + (FarZ - MidZ) * factor;

            return new[]
            {
                new FovZone { Name = "Near", Z = nearZ, XMax = NearXMax * factor, YMax = NearYMax * factor },
                new FovZone { Name = "Mid",  Z = MidZ,  XMax = MidXMax  * factor, YMax = MidYMax  * factor },
                new FovZone { Name = "Far",  Z = farZ,  XMax = FarXMax  * factor, YMax = FarYMax  * factor }
            };
        }

        /// <summary>
        /// Interpolates the X/Y FOV limits at a given local Z, after applying the scale factor
        /// (which grows the FOV).
        /// </summary>
        private static void GetFovLimitsAtZ(double z, double fovScalePercent, out double xMax, out double yMax)
        {
            double factor = GetScaleFactor(fovScalePercent);

            double nearZ = MidZ + (NearZ - MidZ) * factor;
            double farZ = MidZ + (FarZ - MidZ) * factor;

            double nearXMax = NearXMax * factor;
            double nearYMax = NearYMax * factor;
            double midXMax = MidXMax * factor;
            double midYMax = MidYMax * factor;
            double farXMax = FarXMax * factor;
            double farYMax = FarYMax * factor;

            if (z <= MidZ)
            {
                // Between Near and Mid
                double t = (z - nearZ) / (MidZ - nearZ);
                xMax = nearXMax + t * (midXMax - nearXMax);
                yMax = nearYMax + t * (midYMax - nearYMax);
            }
            else
            {
                // Between Mid and Far
                double t = (z - MidZ) / (farZ - MidZ);
                xMax = midXMax + t * (farXMax - midXMax);
                yMax = midYMax + t * (farYMax - midYMax);
            }
        }

        /// <summary>
        /// Converts a 0-10 percent FOV increase into a multiplicative scale factor (1.0 - 1.1).
        /// Clamped to the valid 0-10 range. Growing the FOV (rather than shrinking it) lets the
        /// user relax the already-conservative design margin for analysis purposes.
        /// </summary>
        private static double GetScaleFactor(double fovScalePercent)
        {
            double clamped = Math.Max(0.0, Math.Min(10.0, fovScalePercent));
            return 1.0 + (clamped / 100.0);
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