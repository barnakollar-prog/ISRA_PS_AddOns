using System;
using System.Collections.Generic;
using Tecnomatix.Engineering;

namespace ISRA.Core.Utilities
{
    /// <summary>
    /// Result of robot + sensor holder collision check at a single measurement point.
    /// </summary>
    public class RobotCollisionResult
    {
        public bool HasCollision { get; set; }
        public string LocationName { get; set; }
        public string Label { get; set; }
    }

    /// <summary>
    /// Checks collision between robot + mounted tools vs. user-selected scene objects
    /// (fixture, bodypart, etc.) at each measurement point after jump.
    ///
    /// Scene objects are selected by the user in PS before running the analysis
    /// (ActiveSelection pattern — same as RobotProgramSelection in RobotProgramManipulator).
    /// </summary>
    public static class RobotCollisionCheck
    {
        /// <summary>
        /// Reads the current PS ActiveSelection and returns all selected TxComponents
        /// as the collision B-set (fixture, bodypart, etc.).
        /// Call this before starting path analysis, store the result, and pass it
        /// to CheckCollision() for each point.
        /// </summary>
        public static List<TxComponent> GetCollisionObjectsFromSelection()
        {
            var result = new List<TxComponent>();
            TxObjectList selected = TxApplication.ActiveSelection.GetItems();

            foreach (ITxObject obj in selected)
            {
                var comp = obj as TxComponent;
                if (comp != null)
                    result.Add(comp);
            }

            return result;
        }

        /// <summary>
        /// Checks collision between robot + mounted tools (A-set)
        /// vs. user-selected scene objects (B-set).
        /// Call after JumpRobotToLocation(), before visibility check.
        /// </summary>
        public static RobotCollisionResult CheckCollision(
            TxRobot robot,
            string locationName,
            List<TxComponent> sceneObjects)
        {
            if (sceneObjects == null || sceneObjects.Count == 0)
            {
                return new RobotCollisionResult
                {
                    HasCollision = false,
                    LocationName = locationName,
                    Label = "SKIPPED — no collision objects selected"
                };
            }

            try
            {
                // A-set: robot + all mounted tools
                TxObjectList robotSet = new TxObjectList();
                robotSet.Add(robot);

                foreach (ITxObject tool in robot.MountedTools)
                {
                    var comp = tool as TxComponent;
                    if (comp != null)
                        robotSet.Add(comp);
                }

                // B-set: user-selected scene objects
                TxObjectList sceneList = new TxObjectList();
                foreach (var comp in sceneObjects)
                {
                    if (comp != null && comp.IsValid())
                        sceneList.Add(comp);
                }

                var queryParams = new TxCollisionQueryParams
                {
                    Mode = TxCollisionQueryParams.TxCollisionQueryMode.All,
                    StopQueryAfterFirstCollision = true,
                    ReportLevel = TxCollisionQueryParams.TxCollisionReportLevel.ComponentLevel
                };

                bool hasCollision = TxApplication.ActiveDocument.CollisionRoot
                    .HasCollidingObjectsFromLists(robotSet, sceneList, queryParams);

                return new RobotCollisionResult
                {
                    HasCollision = hasCollision,
                    LocationName = locationName,
                    Label = hasCollision ? "COLLISION" : "CLEAR"
                };
            }
            catch (Exception ex)
            {
                return new RobotCollisionResult
                {
                    HasCollision = false,
                    LocationName = locationName,
                    Label = string.Format("ERROR — {0}", ex.Message)
                };
            }
        }
    }
}