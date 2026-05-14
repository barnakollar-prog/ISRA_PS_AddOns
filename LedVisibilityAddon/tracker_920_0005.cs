using System;
using Tecnomatix.Engineering;


namespace LedVisibilityAddon
{
    /// <summary>
    /// Defines the FOV geometry of tracker 920-0005.
    /// All coordinates are local to the tracker self origin.
    /// MiddleField = Z origin (Z=0)
    /// </summary>
    public class tracker_920_0005
    {
        // ── FOV field definitions (local coordinates in mm) ───────

        // Near field plane (Z = -803)
        private const double NearZ = -803;
        private const double NearXMax = 278;
        private const double NearYMax = 265;

        // Middle field plane (Z = 0 = tracker origin)
        private const double MidZ = 0;
        private const double MidXMax = 850;
        private const double MidYMax = 440;

        // Far field plane (Z = +1200)
        private const double FarZ = 1200;
        private const double FarXMax = 1670;
        private const double FarYMax = 870;

        // ── Position zone constants ───────────────────────────────
        public const double ZoneOptimalMin = 0;      // Z > 0 → Optimal
        public const double ZoneWarnMin = -803;   // -803 < Z < 0 → Warning
        // Z < -803 → NOK

        // ── Position zone enum ────────────────────────────────────
        public enum PositionZone
        {
            Optimal,  // Z > 0
            Warning,  // -803 < Z <= 0
            NOK       // Z < -803
        }

        // ── Public methods ────────────────────────────────────────

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
                case PositionZone.Optimal: return "Optimal";
                case PositionZone.Warning: return "Near Field";
                default: return "NOK";
            }
        }

        /// <summary>
        /// Transforms a world-space point into FOV local coordinate system.
        /// </summary>
        public TxVector ToLocalCoordinates(TxVector worldPoint,
            TxTransformation fovWorldTransform)
        {
            return WorldToLocal(worldPoint, fovWorldTransform);
        }

        /// <summary>
        /// Returns the name of the FOV zone at a given local Z.
        /// </summary>
        public string GetZoneName(TxVector localPoint)
        {
            if (localPoint.Z < NearZ) return "Before Near Field";
            if (localPoint.Z > FarZ) return "Beyond Far Field";
            if (localPoint.Z <= MidZ) return "Near-Mid Zone";
            return "Mid-Far Zone";
        }

        /// <summary>
        /// Extracts the Z axis direction vector from a TxTransformation.
        /// </summary>
        public static TxVector GetZVector(TxTransformation tx)
        {
            // Z column of rotation matrix = [2,0], [2,1], [2,2]
            return new TxVector(tx[0, 2], tx[1, 2], tx[2, 2]);
        }

        // ── Private helpers ───────────────────────────────────────

        private TxVector WorldToLocal(TxVector worldPoint,
            TxTransformation fovWorld)
        {
            return fovWorld.Inverse.Transform(worldPoint);
        }
    }
}