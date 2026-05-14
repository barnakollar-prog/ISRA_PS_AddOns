using System;
using Tecnomatix.Engineering;

namespace ISRA.Calculations.AccuSite
{
    /// <summary>
    /// Collection of geometry calculations and PS geometry creation utilities.
    /// </summary>
    public static class GeometryCalculations
    {
        /// <summary>
        /// Result of triangle height calculation.
        /// </summary>
        public class TriangleResult
        {
            public double SideAB { get; set; }
            public double SideBC { get; set; }
            public double SideCA { get; set; }
            public double LongestSide { get; set; }
            public string LongestSideName { get; set; }
            public double Area { get; set; }
            public double Height { get; set; }
            public bool IsValid { get; set; }
        }

        /// <summary>
        /// Result of Star-Tracker Z vector angle check.
        /// XZ plane (Y axis rotation): max +/- 25 deg
        /// YZ plane (X axis rotation): max +/- 40 deg
        /// Note: tolerances to be validated on Munich demo cell.
        /// </summary>
        public class StarTrackerAngleResult
        {
            public double AngleXZ { get; set; }
            public double AngleYZ { get; set; }
            public bool IsValidXZ { get; set; }
            public bool IsValidYZ { get; set; }
            public bool IsValid { get; set; }
            public string LabelXZ { get; set; }
            public string LabelYZ { get; set; }
            public string Label { get; set; }
        }

        // ── Star-Tracker angle check ──────────────────────────────

        /// <summary>
        /// Calculates the Star Z vector deviation from Tracker negative Z axis,
        /// separately in XZ plane (max 25 deg) and YZ plane (max 40 deg).
        /// Note: tolerances to be validated on Munich demo cell.
        /// </summary>
        public static StarTrackerAngleResult CalculateStarTrackerAngle(
            TxTransformation starWorldTransform,
            TxTransformation trackerWorldTransform)
        {
            // Get star Z vector in world space
            TxVector starZ = GetZVector(starWorldTransform);

            // Get tracker local axes in world space
            TxVector trackerX = new TxVector(
                trackerWorldTransform[0, 0],
                trackerWorldTransform[1, 0],
                trackerWorldTransform[2, 0]);
            TxVector trackerY = new TxVector(
                trackerWorldTransform[0, 1],
                trackerWorldTransform[1, 1],
                trackerWorldTransform[2, 1]);
            TxVector trackerZ = GetZVector(trackerWorldTransform);

            // Express star Z vector in tracker local coordinate system
            double localX = starZ.X * trackerX.X +
                            starZ.Y * trackerX.Y +
                            starZ.Z * trackerX.Z;
            double localY = starZ.X * trackerY.X +
                            starZ.Y * trackerY.Y +
                            starZ.Z * trackerY.Z;
            double localZ = starZ.X * trackerZ.X +
                            starZ.Y * trackerZ.Y +
                            starZ.Z * trackerZ.Z;

            // Star Z must point opposite to tracker Z (localZ must be negative)
            bool pointingCorrectly = localZ < 0;

            // XZ plane deviation (rotation around Y axis): max 25 deg
            double angleXZ = Math.Atan2(Math.Abs(localX), Math.Abs(localZ))
                             * (180.0 / Math.PI);

            // YZ plane deviation (rotation around X axis): max 40 deg
            double angleYZ = Math.Atan2(Math.Abs(localY), Math.Abs(localZ))
                             * (180.0 / Math.PI);

            bool isValidXZ = pointingCorrectly && angleXZ <= 25.0;
            bool isValidYZ = pointingCorrectly && angleYZ <= 40.0;
            bool isValid = isValidXZ && isValidYZ;

            return new StarTrackerAngleResult
            {
                AngleXZ = angleXZ,
                AngleYZ = angleYZ,
                IsValidXZ = isValidXZ,
                IsValidYZ = isValidYZ,
                IsValid = isValid,
                LabelXZ = isValidXZ
                    ? string.Format("OK ({0:F1} deg)", angleXZ)
                    : string.Format("NOK ({0:F1} deg)", angleXZ),
                LabelYZ = isValidYZ
                    ? string.Format("OK ({0:F1} deg)", angleYZ)
                    : string.Format("NOK ({0:F1} deg)", angleYZ),
                Label = isValid ? "OK" : "NOK"
            };
        }

        // ── Triangle calculation ──────────────────────────────────

        /// <summary>
        /// Calculates the height of a triangle formed by 3 points,
        /// dropped from the vertex opposite to the longest side.
        /// All points must be in FOV local coordinate system.
        /// </summary>
        public static TriangleResult CalculateTriangleHeight(
            TxVector a, TxVector b, TxVector c)
        {
            double ab = Distance(a, b);
            double bc = Distance(b, c);
            double ca = Distance(c, a);

            double longest = ab;
            string longestName = "Star1-Star2";
            if (bc > longest) { longest = bc; longestName = "Star2-Star3"; }
            if (ca > longest) { longest = ca; longestName = "Star3-Star1"; }

            TxVector ab_vec = new TxVector(b.X - a.X, b.Y - a.Y, b.Z - a.Z);
            TxVector ac_vec = new TxVector(c.X - a.X, c.Y - a.Y, c.Z - a.Z);

            double crossX = ab_vec.Y * ac_vec.Z - ab_vec.Z * ac_vec.Y;
            double crossY = ab_vec.Z * ac_vec.X - ab_vec.X * ac_vec.Z;
            double crossZ = ab_vec.X * ac_vec.Y - ab_vec.Y * ac_vec.X;
            double area = 0.5 * Math.Sqrt(
                crossX * crossX + crossY * crossY + crossZ * crossZ);
            double height = longest > 0 ? (2.0 * area) / longest : 0;

            return new TriangleResult
            {
                SideAB = ab,
                SideBC = bc,
                SideCA = ca,
                LongestSide = longest,
                LongestSideName = longestName,
                Area = area,
                Height = height,
                IsValid = height >= 500.0
            };
        }

        // ── PS Visualization ──────────────────────────────────────

        /// <summary>
        /// Creates a temporary triangle visualization in PS from 3 world points.
        /// Includes triangle sides and the height line (purple).
        /// Returns the created component so it can be deleted later.
        /// </summary>
        public static TxComponent CreateTriangleVisualization(
            TxVector p1, TxVector p2, TxVector p3, bool isValid)
        {
            TxColor lineColor = isValid
                ? new TxColor(0, 220, 0)
                : new TxColor(220, 0, 0);
            TxColor heightColor = new TxColor(180, 0, 255); // purple

            var compData = new TxLocalComponentCreationData("_LED_Triangle_Temp");
            TxComponent comp = TxApplication.ActiveDocument.PhysicalRoot
                .CreateLocalComponent(compData);

            TxTransformation identity = new TxTransformation();

            // Triangle sides
            var line1 = comp.CreateLine(
                new TxLineCreationData("TriLine_1_2", identity, p1, p2));
            line1.Color = lineColor;

            var line2 = comp.CreateLine(
                new TxLineCreationData("TriLine_2_3", identity, p2, p3));
            line2.Color = lineColor;

            var line3 = comp.CreateLine(
                new TxLineCreationData("TriLine_3_1", identity, p3, p1));
            line3.Color = lineColor;

            // Height line
            double ab = Distance(p1, p2);
            double bc = Distance(p2, p3);
            double ca = Distance(p3, p1);

            TxVector sideStart, sideEnd, apex;
            if (ab >= bc && ab >= ca)
            { sideStart = p1; sideEnd = p2; apex = p3; }
            else if (bc >= ab && bc >= ca)
            { sideStart = p2; sideEnd = p3; apex = p1; }
            else
            { sideStart = p3; sideEnd = p1; apex = p2; }

            TxVector foot = ProjectPointOnLine(apex, sideStart, sideEnd);
            var heightLine = comp.CreateLine(
                new TxLineCreationData("TriHeight", identity, apex, foot));
            heightLine.Color = heightColor;

            TxApplication.RefreshDisplay();
            return comp;
        }

        /// <summary>
        /// Safely deletes a temporary triangle visualization component.
        /// </summary>
        public static void DeleteTriangleVisualization(ref TxComponent comp)
        {
            try
            {
                if (comp != null && comp.IsValid())
                    comp.Delete();
                comp = null;
            }
            catch { }
        }

        // ── Public helpers ────────────────────────────────────────

        /// <summary>
        /// Extracts the Z axis direction vector from a TxTransformation.
        /// </summary>
        public static TxVector GetZVector(TxTransformation tx)
        {
            return new TxVector(tx[0, 2], tx[1, 2], tx[2, 2]);
        }

        // ── Private helpers ───────────────────────────────────────

        private static double Distance(TxVector p1, TxVector p2)
        {
            return Math.Sqrt(
                Math.Pow(p2.X - p1.X, 2) +
                Math.Pow(p2.Y - p1.Y, 2) +
                Math.Pow(p2.Z - p1.Z, 2));
        }

        private static double AngleBetweenVectors(TxVector a, TxVector b)
        {
            double dot = a.X * b.X + a.Y * b.Y + a.Z * b.Z;
            double magA = Math.Sqrt(a.X * a.X + a.Y * a.Y + a.Z * a.Z);
            double magB = Math.Sqrt(b.X * b.X + b.Y * b.Y + b.Z * b.Z);
            if (magA < 1e-10 || magB < 1e-10) return 0;
            double cosAngle = dot / (magA * magB);
            cosAngle = Math.Max(-1.0, Math.Min(1.0, cosAngle));
            return Math.Acos(cosAngle) * (180.0 / Math.PI);
        }

        private static TxVector ProjectPointOnLine(
            TxVector point, TxVector lineStart, TxVector lineEnd)
        {
            double dx = lineEnd.X - lineStart.X;
            double dy = lineEnd.Y - lineStart.Y;
            double dz = lineEnd.Z - lineStart.Z;
            double len2 = dx * dx + dy * dy + dz * dz;
            if (len2 < 1e-10) return lineStart;
            double t = ((point.X - lineStart.X) * dx +
                        (point.Y - lineStart.Y) * dy +
                        (point.Z - lineStart.Z) * dz) / len2;
            return new TxVector(
                lineStart.X + t * dx,
                lineStart.Y + t * dy,
                lineStart.Z + t * dz);
        }
    }
}