using System;
using Tecnomatix.Engineering;
using ISRA.Components.AccuSite.Stars;
using ISRA.Components.AccuSite.Trackers;

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

        /// <summary>
        /// Result of emitter visibility check from a single camera.
        /// </summary>
        public class EmitterVisibilityResult
        {
            public string EmitterName { get; set; }
            public string CameraName { get; set; }
            public double AngleDeg { get; set; }
            public bool IsVisible { get; set; } // angle <= maxAngleDeg
            public string Label { get; set; }
        }

        /// <summary>
        /// Result of full star emitter visibility check
        /// (all emitters x all cameras).
        /// </summary>
        public class StarEmitterVisibilityResult
        {
            public EmitterVisibilityResult[,] Results { get; set; }
            // [emitterIndex, cameraIndex]
            public bool[] EmitterVisibleFromAllCameras { get; set; }
            public int VisibleEmitterCount { get; set; }
            public bool IsValid { get; set; }
            // IsValid = at least 3 emitters visible from all cameras
        }

        // ── Emitter visibility check ──────────────────────────────

        /// <summary>
        /// Calculates the angle at emitter E between:
        /// - vector E->K (emitter to camera)
        /// - emitter Z vector (E->F direction)
        /// Max allowed angle defined by user (default 40 deg).
        /// </summary>
        public static double CalculateEmitterAngle(
            TxVector emitterPos,
            TxVector emitterZVector,
            TxVector cameraPos)
        {
            // Vector E->K
            TxVector EK = new TxVector(
                cameraPos.X - emitterPos.X,
                cameraPos.Y - emitterPos.Y,
                cameraPos.Z - emitterPos.Z);

            // Angle between EK and emitter Z vector at E
            return AngleBetweenVectors(EK, emitterZVector);
        }

        /// <summary>
        /// Checks visibility of all 4 emitters from all 3 cameras.
        /// An emitter is visible from a camera if angle at E <= maxAngleDeg.
        /// Star is valid if at least 3 emitters are visible from ALL cameras.
        /// </summary>
        public static StarEmitterVisibilityResult CheckStarEmitterVisibility(
            ITxLocatableObject starLoc,
            TxTransformation trackerWorld,
            double maxAngleDeg = 40.0)
        {
            var emitters = star_515_0139.GetEmitters();
            var cameras = tracker_920_0005.GetCameras();

            int eCount = emitters.Length; // 4
            int cCount = cameras.Length;  // 3

            var results = new EmitterVisibilityResult[eCount, cCount];
            var emitterVisibleFromAll = new bool[eCount];

            for (int e = 0; e < eCount; e++)
            {
                // Get emitter world position and Z vector
                TxVector emitterWorldPos = star_515_0139
                    .GetEmitterWorldPosition(starLoc, emitters[e]);
                TxVector emitterWorldZ = star_515_0139
                    .GetEmitterWorldZVector(starLoc, emitters[e]);

                bool visibleFromAll = true;

                for (int c = 0; c < cCount; c++)
                {
                    // Get camera world position
                    TxVector cameraWorldPos = tracker_920_0005
                        .GetCameraWorldPosition(trackerWorld, cameras[c]);

                    // Calculate angle at emitter E
                    double angle = CalculateEmitterAngle(
                        emitterWorldPos, emitterWorldZ, cameraWorldPos);

                    bool visible = angle <= maxAngleDeg;
                    if (!visible) visibleFromAll = false;

                    results[e, c] = new EmitterVisibilityResult
                    {
                        EmitterName = emitters[e].Name,
                        CameraName = cameras[c].Name,
                        AngleDeg = angle,
                        IsVisible = visible,
                        Label = visible
                            ? string.Format("OK ({0:F1} deg)", angle)
                            : string.Format("NOK ({0:F1} deg)", angle)
                    };
                }

                emitterVisibleFromAll[e] = visibleFromAll;
            }

            // Count emitters visible from ALL cameras
            int visibleCount = 0;
            foreach (bool v in emitterVisibleFromAll)
                if (v) visibleCount++;

            return new StarEmitterVisibilityResult
            {
                Results = results,
                EmitterVisibleFromAllCameras = emitterVisibleFromAll,
                VisibleEmitterCount = visibleCount,
                IsValid = visibleCount >= 3
            };
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
            TxVector starZ = GetZVector(starWorldTransform);

            TxVector trackerX = new TxVector(
                trackerWorldTransform[0, 0],
                trackerWorldTransform[1, 0],
                trackerWorldTransform[2, 0]);
            TxVector trackerY = new TxVector(
                trackerWorldTransform[0, 1],
                trackerWorldTransform[1, 1],
                trackerWorldTransform[2, 1]);
            TxVector trackerZ = GetZVector(trackerWorldTransform);

            double localX = starZ.X * trackerX.X +
                            starZ.Y * trackerX.Y +
                            starZ.Z * trackerX.Z;
            double localY = starZ.X * trackerY.X +
                            starZ.Y * trackerY.Y +
                            starZ.Z * trackerY.Z;
            double localZ = starZ.X * trackerZ.X +
                            starZ.Y * trackerZ.Y +
                            starZ.Z * trackerZ.Z;

            bool pointingCorrectly = localZ < 0;

            double angleXZ = Math.Atan2(Math.Abs(localX), Math.Abs(localZ))
                             * (180.0 / Math.PI);
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
        /// </summary>
        public static TxComponent CreateTriangleVisualization(
            TxVector p1, TxVector p2, TxVector p3, bool isValid)
        {
            TxColor lineColor = isValid
                ? new TxColor(0, 220, 0)
                : new TxColor(220, 0, 0);
            TxColor heightColor = new TxColor(180, 0, 255);

            var compData = new TxLocalComponentCreationData("_LED_Triangle_Temp");
            TxComponent comp = TxApplication.ActiveDocument.PhysicalRoot
                .CreateLocalComponent(compData);

            TxTransformation identity = new TxTransformation();

            var line1 = comp.CreateLine(
                new TxLineCreationData("TriLine_1_2", identity, p1, p2));
            line1.Color = lineColor;

            var line2 = comp.CreateLine(
                new TxLineCreationData("TriLine_2_3", identity, p2, p3));
            line2.Color = lineColor;

            var line3 = comp.CreateLine(
                new TxLineCreationData("TriLine_3_1", identity, p3, p1));
            line3.Color = lineColor;

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