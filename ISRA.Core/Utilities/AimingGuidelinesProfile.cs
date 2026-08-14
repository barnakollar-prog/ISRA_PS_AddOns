using System;
using System.Collections.Generic;

namespace ISRA.Core.Utilities
{
    /// <summary>
    /// Allowed sensor rotation range for one axis.
    /// Use double.NegativeInfinity / PositiveInfinity for Unlimited.
    /// Sensor X = TCP Rx, Sensor Y = TCP Ry, Sensor Z = TCP Rz.
    /// Source: Helix Sensor Aiming Guidelines Rev E.
    /// </summary>
    public class AxisRotationLimit
    {
        public double MinDeg { get; }
        public double MaxDeg { get; }
        public bool IsUnlimited => double.IsNegativeInfinity(MinDeg)
                                && double.IsPositiveInfinity(MaxDeg);

        public static readonly AxisRotationLimit Unlimited =
            new AxisRotationLimit(double.NegativeInfinity, double.PositiveInfinity);

        public AxisRotationLimit(double minDeg, double maxDeg)
        {
            MinDeg = minDeg;
            MaxDeg = maxDeg;
        }

        public bool IsWithinLimit(double angleDeg) =>
            IsUnlimited || (angleDeg >= MinDeg && angleDeg <= MaxDeg);

        public override string ToString() =>
            IsUnlimited ? "Unlimited" : string.Format("{0}° to {1}°", MinDeg, MaxDeg);
    }

    /// <summary>
    /// Sensor axis rotation limits for a feature type.
    /// Source: Helix Sensor Aiming Guidelines Rev E.
    /// </summary>
    public class AimingGuidelinesProfile
    {
        public string FeatureType { get; }
        public string AlgorithmName { get; } // as in MP Excel column I
        public AxisRotationLimit RxLimit { get; } // Sensor X = TCP Rx
        public AxisRotationLimit RyLimit { get; } // Sensor Y = TCP Ry
        public AxisRotationLimit RzLimit { get; } // Sensor Z = TCP Rz

        public AimingGuidelinesProfile(
            string featureType,
            string algorithmName,
            AxisRotationLimit rxLimit,
            AxisRotationLimit ryLimit,
            AxisRotationLimit rzLimit)
        {
            FeatureType = featureType;
            AlgorithmName = algorithmName;
            RxLimit = rxLimit;
            RyLimit = ryLimit;
            RzLimit = rzLimit;
        }

        public bool IsRxWithinLimit(double rx) => RxLimit.IsWithinLimit(rx);
        public bool IsRyWithinLimit(double ry) => RyLimit.IsWithinLimit(ry);
        public bool IsRzWithinLimit(double rz) => RzLimit.IsWithinLimit(rz);

        public bool IsWithinAllLimits(double rx, double ry, double rz) =>
            IsRxWithinLimit(rx) && IsRyWithinLimit(ry) && IsRzWithinLimit(rz);

        /// <summary>
        /// Returns the strictest (most restrictive) profile between this and another.
        /// Used when a sensor level has multiple algorithms — tightest wins.
        /// </summary>
        public AimingGuidelinesProfile MergeStrictest(AimingGuidelinesProfile other)
        {
            return new AimingGuidelinesProfile(
                featureType: string.Format("{0}+{1}", FeatureType, other.FeatureType),
                algorithmName: string.Format("{0}+{1}", AlgorithmName, other.AlgorithmName),
                rxLimit: StrictestLimit(RxLimit, other.RxLimit),
                ryLimit: StrictestLimit(RyLimit, other.RyLimit),
                rzLimit: StrictestLimit(RzLimit, other.RzLimit));
        }

        private static AxisRotationLimit StrictestLimit(
            AxisRotationLimit a, AxisRotationLimit b)
        {
            if (a.IsUnlimited) return b;
            if (b.IsUnlimited) return a;
            // Tightest = largest MinDeg and smallest MaxDeg
            return new AxisRotationLimit(
                Math.Max(a.MinDeg, b.MinDeg),
                Math.Min(a.MaxDeg, b.MaxDeg));
        }
    }

    /// <summary>
    /// Catalog of all feature types with aiming guidelines.
    /// Source: Helix Sensor Aiming Guidelines Rev E.
    /// Key: algorithm name as it appears in MP Excel column I (case-insensitive).
    /// </summary>
    public static class AimingGuidelinesCatalog
    {
        private static readonly Dictionary<string, AimingGuidelinesProfile> _profiles
            = new Dictionary<string, AimingGuidelinesProfile>(
                StringComparer.OrdinalIgnoreCase)
        {
            {
                "Scanned Cone",
                new AimingGuidelinesProfile(
                    "Cone", "Scanned Cone",
                    AxisRotationLimit.Unlimited,
                    new AxisRotationLimit(-30, 30),
                    new AxisRotationLimit(-10, 10))
            },
            {
                "Scanned Corner",
                new AimingGuidelinesProfile(
                    "Corner", "Scanned Corner",
                    new AxisRotationLimit(-65, 65),
                    new AxisRotationLimit(-30, 40),
                    new AxisRotationLimit(-40, 40))
            },
            {
                "Scanned S-Corner",
                new AimingGuidelinesProfile(
                    "Corner (S-Corner)", "Scanned S-Corner",
                    new AxisRotationLimit(-65, 65),
                    new AxisRotationLimit(-30, 40),
                    new AxisRotationLimit(-40, 40))
            },
            {
                "Scanned Cylinder",
                new AimingGuidelinesProfile(
                    "Cylinder", "Scanned Cylinder",
                    AxisRotationLimit.Unlimited,
                    new AxisRotationLimit(-30, 40),
                    new AxisRotationLimit(-30, 30))
            },
            {
                "Scanned Edge",
                new AimingGuidelinesProfile(
                    "Edge (Trimmed/Abrupt)", "Scanned Edge",
                    new AxisRotationLimit(0, 30),
                    new AxisRotationLimit(-30, 40),
                    new AxisRotationLimit(-40, 40))
            },
            {
                "Scanned Fixture Edge",
                new AimingGuidelinesProfile(
                    "Edge (Hemmed)", "Scanned Fixture Edge",
                    new AxisRotationLimit(30, 75),
                    new AxisRotationLimit(-30, 40),
                    new AxisRotationLimit(-40, 40))
            },
            {
                "Scanned Gap and Flush",
                new AimingGuidelinesProfile(
                    "Gap and Flush", "Scanned Gap and Flush",
                    new AxisRotationLimit(-65, 65),
                    new AxisRotationLimit(-30, 40),
                    new AxisRotationLimit(-40, 40))
            },
            {
                "Scanned Hole",
                new AimingGuidelinesProfile(
                    "Hole (Through)", "Scanned Hole",
                    new AxisRotationLimit(-40, 40),
                    new AxisRotationLimit(-30, 40),
                    AxisRotationLimit.Unlimited)
            },
            {
                "Scanned Hex-Hole",
                new AimingGuidelinesProfile(
                    "Hole (Through Hex)", "Scanned Hex-Hole",
                    new AxisRotationLimit(-40, 40),
                    new AxisRotationLimit(-30, 40),
                    AxisRotationLimit.Unlimited)
            },
            {
                "Scanned Hybrid Hole",
                new AimingGuidelinesProfile(
                    "Hole (Threaded)", "Scanned Hybrid Hole",
                    new AxisRotationLimit(-5, 5),
                    new AxisRotationLimit(5, 15),
                    AxisRotationLimit.Unlimited)
            },
            {
                "Scanned Patch",
                new AimingGuidelinesProfile(
                    "Plane", "Scanned Patch",
                    new AxisRotationLimit(-65, 65),
                    new AxisRotationLimit(-30, 40),
                    AxisRotationLimit.Unlimited)
            },
            {
                "Scanned Profile Corner",
                new AimingGuidelinesProfile(
                    "Profile Corner", "Scanned Profile Corner",
                    new AxisRotationLimit(0, 40),
                    new AxisRotationLimit(-30, 40),
                    new AxisRotationLimit(-10, 10))
            },
            {
                "Scanned Line Range",
                new AimingGuidelinesProfile(
                    "Range", "Scanned Line Range",
                    new AxisRotationLimit(-65, 65),
                    new AxisRotationLimit(-30, 40),
                    AxisRotationLimit.Unlimited)
            },
            {
                "Scanned Min-Range",
                new AimingGuidelinesProfile(
                    "Range (Min)", "Scanned Min-Range",
                    new AxisRotationLimit(-65, 65),
                    new AxisRotationLimit(-30, 40),
                    AxisRotationLimit.Unlimited)
            },
            {
                "Scanned Round Slot",
                new AimingGuidelinesProfile(
                    "Slot (Round)", "Scanned Round Slot",
                    new AxisRotationLimit(-40, 40),
                    new AxisRotationLimit(-30, 40),
                    new AxisRotationLimit(30, 60))
            },
            {
                "Scanned Square Slot",
                new AimingGuidelinesProfile(
                    "Slot (Square)", "Scanned Square Slot",
                    new AxisRotationLimit(-40, 40),
                    new AxisRotationLimit(-30, 40),
                    new AxisRotationLimit(30, 60))
            },
            {
                "Scanned Sphere",
                new AimingGuidelinesProfile(
                    "Sphere", "Scanned Sphere",
                    AxisRotationLimit.Unlimited,
                    AxisRotationLimit.Unlimited,
                    AxisRotationLimit.Unlimited)
            },
            {
                "Scanned Rivet",
                new AimingGuidelinesProfile(
                    "Stud (Rivet)", "Scanned Rivet",
                    new AxisRotationLimit(-10, 10),
                    new AxisRotationLimit(-10, 10),
                    AxisRotationLimit.Unlimited)
            },
            {
                "Scanned Stud",
                new AimingGuidelinesProfile(
                    "Stud", "Scanned Stud",
                    AxisRotationLimit.Unlimited,
                    new AxisRotationLimit(50, 70),
                    new AxisRotationLimit(-10, 10))
            },
        };

        /// <summary>
        /// Look up a profile by algorithm name (case-insensitive).
        /// Returns null if not found.
        /// </summary>
        public static AimingGuidelinesProfile GetByAlgorithm(string algorithmName)
        {
            if (string.IsNullOrEmpty(algorithmName)) return null;
            AimingGuidelinesProfile profile;
            return _profiles.TryGetValue(algorithmName.Trim(), out profile)
                ? profile
                : null;
        }

        /// <summary>
        /// Returns all registered algorithm names.
        /// </summary>
        public static IEnumerable<string> AllAlgorithmNames => _profiles.Keys;
    }
}