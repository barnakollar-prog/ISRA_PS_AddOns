using System;
using System.Collections.Generic;
using Tecnomatix.Engineering;
using ISRA.Components.AccuSite.Trackers;
using ISRA.Components.AccuSite.Stars;

namespace ISRA.Calculations.AccuSite
{
    /// <summary>
    /// Line-of-sight collision check between Star self origin and Tracker cameras.
    /// Uses temporary cylinder geometry for collision detection.
    /// </summary>
    public static class CollisionCheck
    {
        // Cylinder radius in mm
        private const double CylinderRadius = 5.0;

        /// <summary>
        /// Result of a single camera-star line-of-sight check.
        /// </summary>
        public class LineOfSightResult
        {
            public string CameraName { get; set; }
            public string StarName { get; set; }
            public bool IsBlocked { get; set; }
            public string Label { get; set; }
        }

        /// <summary>
        /// Result of full line-of-sight check for one star (all 3 cameras).
        /// </summary>
        public class StarLineOfSightResult
        {
            public LineOfSightResult[] CameraResults { get; set; }
            public bool IsValid { get; set; } // all cameras clear
            public string Label { get; set; }
        }

        /// <summary>
        /// Checks line-of-sight from all 3 tracker cameras to the star self origin.
        /// Creates temporary cylinders and checks for collision with scene geometry.
        /// Cylinders are kept visible and stored in the output component list.
        /// </summary>
        public static StarLineOfSightResult CheckLineOfSight(
            ITxLocatableObject starLoc,
            TxTransformation trackerWorld,
            List<TxComponent> cylinderComponents)
        {
            var cameras = tracker_920_0005.GetCameras();
            var results = new LineOfSightResult[cameras.Length];
            bool allClear = true;

            // Get star world position (self origin) + 5mm offset in star Z direction
            TxTransformation starWorld = starLoc.AbsoluteLocation;
            TxVector starZ = new TxVector(starWorld[0, 2], starWorld[1, 2], starWorld[2, 2]);
            TxVector starWorldPos = new TxVector(
                starWorld.Translation.X + starZ.X * 10.0,
                starWorld.Translation.Y + starZ.Y * 10.0,
                starWorld.Translation.Z + starZ.Z * 10.0);

            for (int c = 0; c < cameras.Length; c++)
            {
                // Camera world position + offset toward star (avoid collision with tracker body)
                TxVector cameraWorldPos = tracker_920_0005
                    .GetCameraWorldPosition(trackerWorld, cameras[c]);

                // Direction from camera to star
                TxVector camToStar = new TxVector(
                    starWorldPos.X - cameraWorldPos.X,
                    starWorldPos.Y - cameraWorldPos.Y,
                    starWorldPos.Z - cameraWorldPos.Z);

                // Normalize
                double len = Math.Sqrt(camToStar.X * camToStar.X +
                                       camToStar.Y * camToStar.Y +
                                       camToStar.Z * camToStar.Z);
                if (len > 0)
                {
                    camToStar = new TxVector(
                        camToStar.X / len,
                        camToStar.Y / len,
                        camToStar.Z / len);
                }

                // Apply 10mm offset
                cameraWorldPos = new TxVector(
                    cameraWorldPos.X + camToStar.X * 10.0,
                    cameraWorldPos.Y + camToStar.Y * 10.0,
                    cameraWorldPos.Z + camToStar.Z * 10.0);

                bool blocked = false;
                TxComponent cylComp = null;

                try
                {
                    // Create temporary cylinder component
                    var compData = new TxLocalComponentCreationData(
                        string.Format("_LOS_{0}_{1}", starLoc.Name, cameras[c].Name));
                    cylComp = TxApplication.ActiveDocument.PhysicalRoot
                        .CreateLocalComponent(compData);

                    // Create cylinder inside component
                    var cylData = new TxCylinderCreationData(
                        "cylinder",
                        cameraWorldPos,
                        starWorldPos,
                        CylinderRadius);
                    cylData.SetAsDisplay();

                    var cyl = cylComp.CreateSolidCylinder(cylData);

                    // Color initially blue (checking)
                    cyl.Color = new TxColor(0, 100, 255);
                    TxApplication.RefreshDisplay();

                    // Build scene object list (all except our cylinder and stars)
                    TxObjectList cylList = new TxObjectList();
                    cylList.Add(cylComp);

                    TxObjectList sceneList = new TxObjectList();
                    var allObjects = TxApplication.ActiveDocument.PhysicalRoot
                        .GetAllDescendants(new TxTypeFilter(typeof(TxComponent)));

                    foreach (ITxObject obj in allObjects)
                    {
                        var comp = obj as TxComponent;
                        if (comp == null) continue;
                        if (comp.Equals(cylComp)) continue;
                        // Skip our other cylinder components
                        if (comp.Name.StartsWith("_LOS_")) continue;
                        if (comp.Name.StartsWith("_LED_")) continue;
                        sceneList.Add(comp);
                    }

                    // Collision check
                    var queryParams = new TxCollisionQueryParams
                    {
                        Mode = TxCollisionQueryParams.TxCollisionQueryMode.All,
                        StopQueryAfterFirstCollision = true,
                        ReportLevel = TxCollisionQueryParams.TxCollisionReportLevel.ComponentLevel
                    };

                    blocked = TxApplication.ActiveDocument.CollisionRoot
                        .HasCollidingObjectsFromLists(cylList, sceneList, queryParams);

                    // Color based on result
                    cyl.Color = blocked
                        ? new TxColor(220, 0, 0)    // red = blocked
                        : new TxColor(0, 220, 0);   // green = clear

                    if (blocked) cyl.Transparency = 0.5;
                    else cyl.Transparency = 0.7;

                    TxApplication.RefreshDisplay();

                    // Store component for later cleanup
                    cylinderComponents.Add(cylComp);
                }
                catch (Exception ex)
                {
                    // If collision check fails, assume clear
                    blocked = false;
                    if (cylComp != null)
                    {
                        try { cylComp.Delete(); } catch { }
                    }
                }

                if (blocked) allClear = false;

                results[c] = new LineOfSightResult
                {
                    CameraName = cameras[c].Name,
                    StarName = starLoc.Name,
                    IsBlocked = blocked,
                    Label = blocked
                        ? string.Format("BLOCKED ({0})", cameras[c].Name)
                        : string.Format("CLEAR ({0})", cameras[c].Name)
                };
            }

            return new StarLineOfSightResult
            {
                CameraResults = results,
                IsValid = allClear,
                Label = allClear ? "CLEAR" : "BLOCKED"
            };
        }

        /// <summary>
        /// Safely deletes all cylinder visualization components.
        /// </summary>
        public static void DeleteCylinderVisualizations(List<TxComponent> cylinderComponents)
        {
            foreach (var comp in cylinderComponents)
            {
                try
                {
                    if (comp != null && comp.IsValid())
                        comp.Delete();
                }
                catch { }
            }
            cylinderComponents.Clear();
        }
    }
}