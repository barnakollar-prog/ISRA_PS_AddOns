using System;
using System.Collections.Generic;
using Tecnomatix.Engineering;
using ISRA.Components.AccuSite.SensorHolders;
using ISRA.Components.AccuSite.Trackers;
using ISRA.Core.Utilities;

namespace ISRA.Calculations.AccuSite
{
    public class ConstellationVisibilityResult
    {
        public EmitterCameraAngleResult[,] AngleResults { get; set; }
        public List<VisibleEmitterResult> VisibleEmitters { get; set; }
        public Dictionary<string, int> VisibleCountPerGroup { get; set; }
        public int TotalVisibleCount { get; set; }
        /// <summary>True if constellation is within tracker FOV.</summary>
        public bool IsInFov { get; set; }

        /// <summary>
        /// True if LOS is blocked enough to prevent sufficient emitter visibility,
        /// even though angle filter would have passed enough emitters.
        /// N/A (null) if angle filter itself was insufficient.
        /// </summary>
        public bool? IsSightBlocked { get; set; }
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
        public List<string> VisibleFromCameras { get; set; }
    }

    public static class ConstellationVisibilityChecker
    {
        private const double DefaultMaxAngleDeg = AccuSiteConstants.EmitterMaxAngleDeg;
        private const double CylinderRadius = 5.0;
        private const double LedSquareHalfSize = 6.0;

        // ── Public entry point ────────────────────────────────────

        public static ConstellationVisibilityResult Check(
    ITxLocatableObject holderLoc,
    ISensorHolder holder,
    TxTransformation trackerWorld,
    ITracker tracker,
    List<TxComponent> visComponents,
    TxRobot robot,
    double maxAngleDeg = DefaultMaxAngleDeg)
        {
            var emitters = holder.GetEmitters();
            var cameras = tracker.GetCameras();

            var angleResults = RunAngleFilter(
                holderLoc, holder, emitters, trackerWorld, tracker, cameras, maxAngleDeg);

            var candidates = GetAngleCandidates(
                holderLoc, holder, emitters, angleResults, cameras);

            var visibleEmitters = RunLineOfSightFilter(
                candidates, trackerWorld, tracker, cameras, holderLoc, robot);

            // ── FOV check ─────────────────────────────────────────────
            // True if at least one emitter is inside the tracker FOV
            bool isInFov = false;
            TxTransformation trackerInverse = trackerWorld.Inverse;
            foreach (var emitter in emitters)
            {
                TxVector emitterLocalPos = trackerInverse.Transform(
                    holder.GetEmitterWorldPosition(holderLoc, emitter));
                if (tracker.IsInFOV(emitterLocalPos)) { isInFov = true; break; }
            }

            // ── Sight check ───────────────────────────────────────────
            // How many emitters pass angle filter from ALL cameras
            int anglePassCount = 0;
            for (int e = 0; e < emitters.Length; e++)
            {
                bool allCamsOk = true;
                for (int c = 0; c < cameras.Length; c++)
                    if (!angleResults[e, c].PassedAngle) { allCamsOk = false; break; }
                if (allCamsOk) anglePassCount++;
            }

            bool angleWouldBeOk = anglePassCount >= 3;
            bool visibleOk = visibleEmitters.Count >= 3;

            // null  = angle filter itself was insufficient (N/A)
            // true  = angle OK but LOS blocks enough to fail
            // false = angle OK and LOS OK
            bool? isSightBlocked = null;
            if (angleWouldBeOk && !visibleOk)
                isSightBlocked = true;
            else if (angleWouldBeOk && visibleOk)
                isSightBlocked = false;

            // ── Visible count per group ───────────────────────────────
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
                TotalVisibleCount = visibleEmitters.Count,
                IsInFov = isInFov,
                IsSightBlocked = isSightBlocked
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
            int eCount = emitters.Length;
            int cCount = cameras.Length;
            var results = new EmitterCameraAngleResult[eCount, cCount];

            TxTransformation trackerInverse = trackerWorld.Inverse;

            for (int e = 0; e < eCount; e++)
            {
                TxVector emitterWorldPos = holder.GetEmitterWorldPosition(holderLoc, emitters[e]);
                TxVector emitterWorldZ = holder.GetEmitterWorldZVector(holderLoc, emitters[e]);

                TxVector emitterLocalPos = trackerInverse.Transform(emitterWorldPos);
                bool inFov = tracker.IsInFOV(emitterLocalPos);

                for (int c = 0; c < cCount; c++)
                {
                    if (!inFov)
                    {
                        results[e, c] = new EmitterCameraAngleResult
                        {
                            EmitterName = emitters[e].Name,
                            Group = emitters[e].Group,
                            CameraName = cameras[c].Name,
                            AngleDeg = double.NaN,
                            PassedAngle = false
                        };
                        continue;
                    }

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
            List<(SensorEmitterData emitter, TxVector worldPos, TxVector worldZ,
          List<string> candidateCameras)> candidates,
            TxTransformation trackerWorld,
            ITracker tracker,
            CameraData[] cameras,
            ITxLocatableObject holderLoc,
            TxRobot robot)

        {
            var visible = new List<VisibleEmitterResult>();
            var sceneList = BuildSceneList(holderLoc, robot);

            foreach (var (emitter, worldPos, worldZ, candidateCameras) in candidates)
            {
                var clearCameras = new List<string>();

                foreach (var cameraName in candidateCameras)
                {
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
            TxVector dir = Normalize(new TxVector(
                emitterWorldPos.X - cameraWorldPos.X,
                emitterWorldPos.Y - cameraWorldPos.Y,
                emitterWorldPos.Z - cameraWorldPos.Z));

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

                return TxApplication.ActiveDocument.CollisionRoot
                    .HasCollidingObjectsFromLists(cylList, sceneList, queryParams);
            }
            catch { return false; }
            finally
            {
                try { cylComp?.Delete(); } catch { }
            }
        }

        private static TxObjectList BuildSceneList(
            ITxLocatableObject holderLoc,
            TxRobot robot)
        {
            var sceneList = new TxObjectList();
            string holderName = (holderLoc as TxComponent)?.Name ?? "";

            // 1. Normál scene komponensek
            var allObjects = TxApplication.ActiveDocument.PhysicalRoot
                .GetAllDescendants(new TxTypeFilter(typeof(TxComponent)));

            foreach (ITxObject obj in allObjects)
            {
                var comp = obj as TxComponent;
                if (comp == null) continue;
                if (comp.Name.StartsWith("_CONST_")) continue;
                if (comp.Name.StartsWith("_LOS_")) continue;
                if (comp.Name.StartsWith("_LED_")) continue;
                if (!string.IsNullOrEmpty(holderName)
                    && comp.Name == holderName) continue;
                sceneList.Add(comp);
            }

            // 2. Robot leszármazottai külön hozzáadva
            if (robot != null)
            {
                string linkInfo3 = "";
                foreach (ITxObject linkObj in robot.Links)
                {
                    var link = linkObj as TxKinematicLink;
                    if (link == null) continue;

                    // Minden típus
                    var all = link.GetAllDescendants(new TxTypeFilter(typeof(ITxObject)));
                    linkInfo3 += string.Format("Link all descendants: {0}\n", all.Count);
                    int c2 = 0;
                    foreach (ITxObject obj in all)
                    {
                        if (c2 < 5)
                            linkInfo3 += string.Format("  -> {0}\n", obj.GetType().Name);
                        c2++;
                    }
                }

                // Robot saját típusa és leszármazottai
                var robotAll = robot.GetAllDescendants(new TxTypeFilter(typeof(ITxObject)));
                linkInfo3 += string.Format("\nRobot direct descendants: {0}\n", robotAll.Count);
                int rc = 0;
                foreach (ITxObject obj in robotAll)
                {
                    if (rc < 10)
                        linkInfo3 += string.Format("  -> {0} : {1}\n",
                            obj.GetType().Name,
                            (obj as TxComponent)?.Name ?? "?");
                    rc++;
                }

                System.IO.File.WriteAllText(@"C:\Temp\robotLinks3_debug.txt", linkInfo3);
            }

            return sceneList;
        }

        // ── Phase 3: LED square visualization ────────────────────

        public static void CreateLedSquare(
            TxVector worldPos,
            TxVector worldZVec,
            List<TxComponent> visComponents)
        {
            try
            {
                TxVector zAxis = Normalize(worldZVec);
                TxVector xAxis = GetPerpendicularVector(zAxis);
                TxVector yAxis = Cross(zAxis, xAxis);

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

        // ── Angle visualization ───────────────────────────────────

        public static void CreateAngleVisualization(
            ITxLocatableObject holderLoc,
            ISensorHolder holder,
            TxTransformation trackerWorld,
            ITracker tracker,
            EmitterCameraAngleResult[,] angleResults,
            List<TxComponent> visComponents,
            double maxAngleDeg = 40.0)
        {
            var emitters = holder.GetEmitters();
            var cameras = tracker.GetCameras();

            TxTransformation identity = new TxTransformation();
            TxColor green = new TxColor(0, 220, 0);
            TxColor red = new TxColor(220, 0, 0);
            TxColor gray = new TxColor(150, 150, 150);

            try
            {
                var compData = new TxLocalComponentCreationData("_CONST_ANGLE_vis");
                var comp = TxApplication.ActiveDocument.PhysicalRoot
                    .CreateLocalComponent(compData);

                for (int e = 0; e < emitters.Length; e++)
                {
                    TxVector emitterWorldPos = holder.GetEmitterWorldPosition(
                        holderLoc, emitters[e]);

                    for (int c = 0; c < cameras.Length; c++)
                    {
                        TxVector cameraWorldPos = tracker.GetCameraWorldPosition(
                            trackerWorld, cameras[c]);

                        var result = angleResults[e, c];

                        TxColor lineColor;
                        if (double.IsNaN(result.AngleDeg))
                            lineColor = gray;
                        else if (result.PassedAngle)
                            lineColor = green;
                        else
                            lineColor = red;

                        string lineName = string.Format("_{0}_{1}",
                            emitters[e].Name, cameras[c].Name);

                        var line = comp.CreateLine(
                            new TxLineCreationData(lineName, identity,
                                cameraWorldPos, emitterWorldPos));
                        line.Color = lineColor;
                    }
                }

                TxApplication.RefreshDisplay();
                visComponents.Add(comp);
            }
            catch { }
        }

        public static void CreateAngleVisualizationFiltered(
    ITxLocatableObject holderLoc,
    ISensorHolder holder,
    TxTransformation trackerWorld,
    ITracker tracker,
    EmitterCameraAngleResult[,] angleResults,
    List<TxComponent> visComponents,
    bool showOk,
    bool showNok,
    bool showFov,
    double maxAngleDeg = 40.0)
        {
            var emitters = holder.GetEmitters();
            var cameras = tracker.GetCameras();

            TxTransformation identity = new TxTransformation();
            TxColor green = new TxColor(0, 220, 0);
            TxColor red = new TxColor(220, 0, 0);
            TxColor gray = new TxColor(150, 150, 150);

            try
            {
                var compData = new TxLocalComponentCreationData("_CONST_ANGLE_vis");
                var comp = TxApplication.ActiveDocument.PhysicalRoot
                    .CreateLocalComponent(compData);

                bool anyLine = false;

                for (int e = 0; e < emitters.Length; e++)
                {
                    TxVector emitterWorldPos = holder.GetEmitterWorldPosition(
                        holderLoc, emitters[e]);

                    for (int c = 0; c < cameras.Length; c++)
                    {
                        TxVector cameraWorldPos = tracker.GetCameraWorldPosition(
                            trackerWorld, cameras[c]);

                        var result = angleResults[e, c];

                        bool isFov = double.IsNaN(result.AngleDeg);
                        bool isOk = !isFov && result.PassedAngle;
                        bool isNok = !isFov && !result.PassedAngle;

                        // Filter
                        if (isFov && !showFov) continue;
                        if (isOk && !showOk) continue;
                        if (isNok && !showNok) continue;

                        TxColor lineColor = isFov ? gray : isOk ? green : red;

                        string lineName = string.Format("_{0}_{1}",
                            emitters[e].Name, cameras[c].Name);

                        var line = comp.CreateLine(
                            new TxLineCreationData(lineName, identity,
                                cameraWorldPos, emitterWorldPos));
                        line.Color = lineColor;
                        anyLine = true;
                    }
                }

                if (anyLine)
                {
                    TxApplication.RefreshDisplay();
                    visComponents.Add(comp);
                }
                else
                {
                    comp.Delete();
                }
            }
            catch { }
        }

        // ── Cleanup ───────────────────────────────────────────────

        public static void DeleteVisualizations(List<TxComponent> visComponents)
        {
            foreach (var comp in visComponents)
            {
                try { if (comp != null && comp.IsValid()) comp.Delete(); } catch { }
            }
            visComponents.Clear();
            TxApplication.RefreshDisplay();
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

    } // end class ConstellationVisibilityChecker
} // end namespace