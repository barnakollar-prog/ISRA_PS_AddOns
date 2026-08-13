using System;
using System.Collections.Generic;
using Tecnomatix.Engineering;
using ISRA.Components.AccuSite.SensorHolders;
using ISRA.Components.AccuSite.Trackers;

namespace ISRA.Calculations.AccuSite
{
    /// <summary>
    /// Result of constellation visibility check for a single measurement point.
    /// </summary>
    public class ConstellationVisibilityResult
    {
        /// <summary>All 40 emitter angle results (emitter x camera)</summary>
        public EmitterCameraAngleResult[,] AngleResults { get; set; }

        /// <summary>Emitters that passed angle check AND line-of-sight (max ~12)</summary>
        public List<VisibleEmitterResult> VisibleEmitters { get; set; }

        /// <summary>Group name → visible emitter count in that group</summary>
        public Dictionary<string, int> VisibleCountPerGroup { get; set; }

        /// <summary>Total visible emitter count across all groups</summary>
        public int TotalVisibleCount { get; set; }
    }

    public class EmitterCameraAngleResult
    {
        public string EmitterName { get; set; }
        public string Group { get; set; }
        public string CameraName { get; set; }
        public double AngleDeg { get; set; }
        public bool PassedAngle { get; set; }
    }

    public class VisibleEmitterResult
    {
        public string EmitterName { get; set; }
        public string Group { get; set; }
        public TxVector WorldPos { get; set; }
        public TxVector WorldZVec { get; set; }
        /// <summary>Which cameras have clear line-of-sight to this emitter</summary>
        public List<string> VisibleFromCameras { get; set; }
    }

    /// <summary>
    /// Checks which LEDs of a sensor holder constellation are visible
    /// from a tracker at a given measurement point.
    ///
    /// Phase 1 — angle filter (all 40 LEDs, no PS geometry)
    /// Phase 2 — line-of-sight collision check (angle candidates only, ~12 max, no display)
    /// Phase 3 — visualization: 12x12mm green square at each visible LED frame
    /// </summary>
    public static class ConstellationVisibilityChecker
    {
        private const double DefaultMaxAngleDeg = 40.0;
        private const double CylinderRadius = 5.0;
        private const double LedSquareHalfSize = 6.0; // 12x12mm → ±6mm

        // ── Public entry point ────────────────────────────────────

        /// <summary>
        /// Runs all three phases and returns the visibility result.
        /// Visualization components are added to visComponents for later cleanup.
        /// </summary>
        public static ConstellationVisibilityResult Check(
            ITxLocatableObject holderLoc,
            ISensorHolder holder,
            TxTransformation trackerWorld,
            ITracker tracker,
            List<TxComponent> visComponents,
            double maxAngleDeg = DefaultMaxAngleDeg)
        {
            var emitters = holder.GetEmitters();
            var cameras = tracker.GetCameras();

            // ── Phase 1: angle filter ─────────────────────────────
            var angleResults = RunAngleFilter(
                holderLoc, holder, emitters, trackerWorld, tracker, cameras, maxAngleDeg);

            // Collect candidates: emitters that pass angle check for at least 1 camera
            var candidates = GetAngleCandidates(holderLoc, holder, emitters, angleResults, cameras);

            // ── Phase 2: line-of-sight collision ──────────────────
            var visibleEmitters = RunLineOfSightFilter(candidates, trackerWorld, tracker, cameras);

            // ── Phase 3: visualization ────────────────────────────
            foreach (var vis in visibleEmitters)
                CreateLedSquare(vis.WorldPos, vis.WorldZVec, visComponents);

            // ── Aggregate results ─────────────────────────────────
            var visibleCountPerGroup = new Dictionary<string, int>();
            foreach (var vis in visibleEmitters)
            {
                if (!visibleCountPerGroup.ContainsKey(vis.Group))
                    visibleCountPerGroup[vis.Group] = 0;
                visibleCountPerGroup[vis.Group]++;
            }

            return new ConstellationVisibilityResult
            {
                AngleResults = angleResults,
                VisibleEmitters = visibleEmitters,
                VisibleCountPerGroup = visibleCountPerGroup,
                TotalVisibleCount = visibleEmitters.Count
            };
        }

        // ── Phase 1: angle filter ─────────────────────────────────

        private static EmitterCameraAngleResult[,] RunAngleFilter(
            ITxLocatableObject holderLoc,
            ISensorHolder holder,
            SensorEmitterData[] emitters,
            TxTransformation trackerWorld,
            ITracker tracker,
            CameraData[] cameras,
            double maxAngleDeg)
        {
            int eCount = emitters.Length; // 40
            int cCount = cameras.Length;  // 3
            var results = new EmitterCameraAngleResult[eCount, cCount];

            for (int e = 0; e < eCount; e++)
            {
                TxVector emitterWorldPos = holder.GetEmitterWorldPosition(holderLoc, emitters[e]);
                TxVector emitterWorldZ = holder.GetEmitterWorldZVector(holderLoc, emitters[e]);

                for (int c = 0; c < cCount; c++)
                {
                    TxVector cameraWorldPos = tracker.GetCameraWorldPosition(trackerWorld, cameras[c]);
                    double angle = GeometryCalculations.CalculateEmitterAngle(
                                                emitterWorldPos, emitterWorldZ, cameraWorldPos);

                    results[e, c] = new EmitterCameraAngleResult
                    {
                        EmitterName = emitters[e].Name,
                        Group = emitters[e].Group,
                        CameraName = cameras[c].Name,
                        AngleDeg = angle,
                        PassedAngle = angle <= maxAngleDeg
                    };
                }
            }

            return results;
        }

        // Collect emitters that passed angle for ≥1 camera, with world coords
        private static List<(SensorEmitterData emitter, TxVector worldPos, TxVector worldZ, List<string> candidateCameras)>
            GetAngleCandidates(
                ITxLocatableObject holderLoc,
                ISensorHolder holder,
                SensorEmitterData[] emitters,
                EmitterCameraAngleResult[,] angleResults,
                CameraData[] cameras)
        {
            var candidates = new List<(SensorEmitterData, TxVector, TxVector, List<string>)>();
            int cCount = cameras.Length;

            for (int e = 0; e < emitters.Length; e++)
            {
                var passedCameras = new List<string>();
                for (int c = 0; c < cCount; c++)
                    if (angleResults[e, c].PassedAngle)
                        passedCameras.Add(cameras[c].Name);

                if (passedCameras.Count == 0) continue;

                TxVector worldPos = holder.GetEmitterWorldPosition(holderLoc, emitters[e]);
                TxVector worldZ = holder.GetEmitterWorldZVector(holderLoc, emitters[e]);
                candidates.Add((emitters[e], worldPos, worldZ, passedCameras));
            }

            return candidates;
        }

        // ── Phase 2: line-of-sight ────────────────────────────────

        private static List<VisibleEmitterResult> RunLineOfSightFilter(
            List<(SensorEmitterData emitter, TxVector worldPos, TxVector worldZ, List<string> candidateCameras)> candidates,
            TxTransformation trackerWorld,
            ITracker tracker,
            CameraData[] cameras)
        {
            var visible = new List<VisibleEmitterResult>();

            // Build scene object list once (reuse for all cylinders)
            TxObjectList sceneList = BuildSceneList();

            foreach (var (emitter, worldPos, worldZ, candidateCameras) in candidates)
            {
                var clearCameras = new List<string>();

                foreach (var cameraName in candidateCameras)
                {
                    // Find camera
                    CameraData cam = null;
                    foreach (var c in cameras)
                        if (c.Name == cameraName) { cam = c; break; }
                    if (cam == null) continue;

                    TxVector cameraWorldPos = tracker.GetCameraWorldPosition(trackerWorld, cam);

                    bool blocked = CheckLineOfSight(cameraWorldPos, worldPos, sceneList);
                    if (!blocked)
                        clearCameras.Add(cameraName);
                }

                if (clearCameras.Count > 0)
                {
                    visible.Add(new VisibleEmitterResult
                    {
                        EmitterName = emitter.Name,
                        Group = emitter.Group,
                        WorldPos = worldPos,
                        WorldZVec = worldZ,
                        VisibleFromCameras = clearCameras
                    });
                }
            }

            return visible;
        }

        private static bool CheckLineOfSight(
            TxVector cameraWorldPos,
            TxVector emitterWorldPos,
            TxObjectList sceneList)
        {
            // Direction from camera to emitter, normalized
            TxVector dir = Normalize(new TxVector(
                emitterWorldPos.X - cameraWorldPos.X,
                emitterWorldPos.Y - cameraWorldPos.Y,
                emitterWorldPos.Z - cameraWorldPos.Z));

            // 10mm offsets to avoid self-collision
            TxVector camOffset = new TxVector(
                cameraWorldPos.X + dir.X * 10.0,
                cameraWorldPos.Y + dir.Y * 10.0,
                cameraWorldPos.Z + dir.Z * 10.0);
            TxVector emOffset = new TxVector(
                emitterWorldPos.X - dir.X * 10.0,
                emitterWorldPos.Y - dir.Y * 10.0,
                emitterWorldPos.Z - dir.Z * 10.0);

            TxComponent cylComp = null;
            try
            {
                var compData = new TxLocalComponentCreationData("_CONST_LOS_temp");
                cylComp = TxApplication.ActiveDocument.PhysicalRoot
                    .CreateLocalComponent(compData);

                var cylData = new TxCylinderCreationData(
                    "cylinder", camOffset, emOffset, CylinderRadius);
                cylData.SetAsDisplay();
                cylComp.CreateSolidCylinder(cylData);

                TxObjectList cylList = new TxObjectList();
                cylList.Add(cylComp);

                var queryParams = new TxCollisionQueryParams
                {
                    Mode = TxCollisionQueryParams.TxCollisionQueryMode.All,
                    StopQueryAfterFirstCollision = true,
                    ReportLevel = TxCollisionQueryParams.TxCollisionReportLevel.ComponentLevel
                };

                bool blocked = TxApplication.ActiveDocument.CollisionRoot
                    .HasCollidingObjectsFromLists(cylList, sceneList, queryParams);

                return blocked;
            }
            catch
            {
                return false; // assume clear on error
            }
            finally
            {
                // Always delete — no display
                try { cylComp?.Delete(); } catch { }
            }
        }

        private static TxObjectList BuildSceneList()
        {
            var sceneList = new TxObjectList();
            var allObjects = TxApplication.ActiveDocument.PhysicalRoot
                .GetAllDescendants(new TxTypeFilter(typeof(TxComponent)));

            foreach (ITxObject obj in allObjects)
            {
                var comp = obj as TxComponent;
                if (comp == null) continue;
                if (comp.Name.StartsWith("_CONST_")) continue;
                if (comp.Name.StartsWith("_LOS_")) continue;
                if (comp.Name.StartsWith("_LED_")) continue;
                sceneList.Add(comp);
            }

            return sceneList;
        }

        // ── Phase 3: visualization ────────────────────────────────

        /// <summary>
        /// Creates a 12x12mm green square in world space, centered at worldPos,
        /// oriented so its normal = worldZVec (square plane ⊥ emission direction).
        /// </summary>
        private static void CreateLedSquare(
            TxVector worldPos,
            TxVector worldZVec,
            List<TxComponent> visComponents)
        {
            try
            {
                // Build local coordinate frame: Z = emission direction
                TxVector zAxis = Normalize(worldZVec);
                TxVector xAxis = GetPerpendicularVector(zAxis);
                TxVector yAxis = Cross(zAxis, xAxis);

                // 4 corners of 12x12mm square
                TxVector c1 = Offset(worldPos, xAxis, LedSquareHalfSize, yAxis, LedSquareHalfSize);
                TxVector c2 = Offset(worldPos, xAxis, -LedSquareHalfSize, yAxis, LedSquareHalfSize);
                TxVector c3 = Offset(worldPos, xAxis, -LedSquareHalfSize, yAxis, -LedSquareHalfSize);
                TxVector c4 = Offset(worldPos, xAxis, LedSquareHalfSize, yAxis, -LedSquareHalfSize);

                var compData = new TxLocalComponentCreationData("_CONST_LED_vis");
                var comp = TxApplication.ActiveDocument.PhysicalRoot
                    .CreateLocalComponent(compData);

                TxTransformation identity = new TxTransformation();
                TxColor green = new TxColor(0, 220, 0);

                var l1 = comp.CreateLine(new TxLineCreationData("s1", identity, c1, c2)); l1.Color = green;
                var l2 = comp.CreateLine(new TxLineCreationData("s2", identity, c2, c3)); l2.Color = green;
                var l3 = comp.CreateLine(new TxLineCreationData("s3", identity, c3, c4)); l3.Color = green;
                var l4 = comp.CreateLine(new TxLineCreationData("s4", identity, c4, c1)); l4.Color = green;

                TxApplication.RefreshDisplay();
                visComponents.Add(comp);
            }
            catch { }
        }

        // ── Vector helpers ────────────────────────────────────────

        private static TxVector Normalize(TxVector v)
        {
            double len = Math.Sqrt(v.X * v.X + v.Y * v.Y + v.Z * v.Z);
            if (len < 1e-10) return new TxVector(0, 0, 1);
            return new TxVector(v.X / len, v.Y / len, v.Z / len);
        }

        private static TxVector Cross(TxVector a, TxVector b)
        {
            return new TxVector(
                a.Y * b.Z - a.Z * b.Y,
                a.Z * b.X - a.X * b.Z,
                a.X * b.Y - a.Y * b.X);
        }

        /// <summary>Returns an arbitrary vector perpendicular to v.</summary>
        private static TxVector GetPerpendicularVector(TxVector v)
        {
            TxVector candidate = Math.Abs(v.X) < 0.9
                ? new TxVector(1, 0, 0)
                : new TxVector(0, 1, 0);
            return Normalize(Cross(v, candidate));
        }

        private static TxVector Offset(
            TxVector origin,
            TxVector ax, double ax_scale,
            TxVector ay, double ay_scale)
        {
            return new TxVector(
                origin.X + ax.X * ax_scale + ay.X * ay_scale,
                origin.Y + ax.Y * ax_scale + ay.Y * ay_scale,
                origin.Z + ax.Z * ax_scale + ay.Z * ay_scale);
        }
    }
}