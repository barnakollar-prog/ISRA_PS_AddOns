using System;
using Tecnomatix.Engineering;

namespace ISRA.Core.Utilities
{
    /// <summary>
    /// Common geometry and math helper methods.
    /// </summary>
    public static class GeometryHelper
    {
        /// <summary>
        /// Calculates the angle between two vectors in degrees.
        /// </summary>
        public static double AngleBetweenVectors(TxVector v1, TxVector v2)
        {
            double dot = v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z;
            double mag1 = Math.Sqrt(v1.X * v1.X + v1.Y * v1.Y + v1.Z * v1.Z);
            double mag2 = Math.Sqrt(v2.X * v2.X + v2.Y * v2.Y + v2.Z * v2.Z);

            if (mag1 == 0 || mag2 == 0) return 0.0;

            double cos = dot / (mag1 * mag2);
            cos = Math.Max(-1.0, Math.Min(1.0, cos));
            return Math.Acos(cos) * (180.0 / Math.PI);
        }

        /// <summary>
        /// Normalizes an angle to ±180° range (shortest arc).
        /// </summary>
        public static double NormalizeAngle180(double angle)
        {
            double a = (angle + 180.0) % 360.0;
            if (a < 0) a += 360.0;
            return a - 180.0;
        }

        /// <summary>
        /// Normalizes an angle to 0-360° range.
        /// </summary>
        public static double NormalizeAngle360(double angle)
        {
            double a = angle % 360.0;
            if (a < 0) a += 360.0;
            return a;
        }

        /// <summary>
        /// Calculates the distance between two points.
        /// </summary>
        public static double Distance(TxVector p1, TxVector p2)
        {
            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;
            double dz = p2.Z - p1.Z;
            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        /// <summary>
        /// Normalizes a vector (returns unit vector in same direction).
        /// </summary>
        public static TxVector Normalize(TxVector v)
        {
            double mag = Math.Sqrt(v.X * v.X + v.Y * v.Y + v.Z * v.Z);
            if (mag == 0) return new TxVector(0, 0, 0);
            return new TxVector(v.X / mag, v.Y / mag, v.Z / mag);
        }

        /// <summary>
        /// Calculates the magnitude (length) of a vector.
        /// </summary>
        public static double Magnitude(TxVector v)
        {
            return Math.Sqrt(v.X * v.X + v.Y * v.Y + v.Z * v.Z);
        }

        /// <summary>
        /// Calculates the dot product of two vectors.
        /// </summary>
        public static double DotProduct(TxVector v1, TxVector v2)
        {
            return v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z;
        }

        /// <summary>
        /// Calculates the cross product of two vectors.
        /// </summary>
        public static TxVector CrossProduct(TxVector v1, TxVector v2)
        {
            return new TxVector(
                v1.Y * v2.Z - v1.Z * v2.Y,
                v1.Z * v2.X - v1.X * v2.Z,
                v1.X * v2.Y - v1.Y * v2.X
            );
        }
    }
}
