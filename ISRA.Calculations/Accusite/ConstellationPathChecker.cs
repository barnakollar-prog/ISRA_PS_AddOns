using ISRA.Components.AccuSite.SensorHolders;
using ISRA.Components.AccuSite.Trackers;
using ISRA.Core.Utilities;
using System;
using System.Collections.Generic;
using Tecnomatix.Engineering;
using Tecnomatix.Engineering.Olp.OLP_Utilities;
using Tecnomatix.Engineering.OLP;
using ISRA.Core.Utilities;

namespace ISRA.Calculations.AccuSite
{
    /// <summary>
    /// Result of constellation check for a single measurement point.
    /// </summary>
    public class ConstellationPointResult
    {
        public string LocationName { get; set; }
        public bool RobotReached { get; set; }
        public bool HasCollision { get; set; }
        public ConstellationVisibilityResult Visibility { get; set; }
        public ConstellationCriteriaEngine.CriteriaResult Criteria { get; set; }
        public string Label { get; set; }
    }

    /// <summary>
    /// Result of full path constellation check.
    /// </summary>
    public class ConstellationPathResult
    {
        public string ProgramName { get; set; }
        public List<ConstellationPointResult> PointResults { get; set; }
        public int OkCount { get; set; }
        public int NokCount { get; set; }
        public int SkippedCount { get; set; } // robot could not reach
    }

    /// <summary>
    /// Runs constellation check along a robotic path.
    /// For each location: jump → find holder → visibility check → criteria eval.
    /// Pattern follows TempComp path traversal.
    /// </summary>
    public class ConstellationPathChecker
    {
        private readonly TxRobot _robot;
        private readonly ITracker _tracker;
        private readonly TxTransformation _trackerWorld;
        private readonly double _maxAngleDeg;
        private readonly List<TxComponent> _collisionObjects;

        public ConstellationPathChecker(
            TxRobot robot,
            ITracker tracker,
            TxTransformation trackerWorld,
            List<TxComponent> collisionObjects,
            double maxAngleDeg = 40.0)
        {
            _robot = robot;
            _tracker = tracker;
            _trackerWorld = trackerWorld;
            _collisionObjects = collisionObjects;
            _maxAngleDeg = maxAngleDeg;
        }

        /// <summary>
        /// Runs constellation check for all measurement locations in a program.
        /// visComponents receives all visualization components for later cleanup.
        /// </summary>
        public ConstellationPathResult CheckPath(
            TxWeldOperation program,
            List<TxComponent> visComponents)
        {
            var pointResults = new List<ConstellationPointResult>();
            int ok = 0, nok = 0, skipped = 0;

            // Get all robotic locations in program
            TxObjectList locations = program.GetAllDescendants(
                new TxTypeFilter(typeof(ITxRoboticLocationOperation)));

            var followMode = new TxOlpRobotFollowMode(_robot);

            foreach (ITxObject obj in locations)
            {
                var loc = obj as ITxRoboticLocationOperation;
                if (loc == null) continue;

                // 1. Jump
                bool reached = followMode.JumpRobotToLocation(loc);
                TxApplication.RefreshDisplay();

                if (!reached)
                {
                    pointResults.Add(new ConstellationPointResult
                    {
                        LocationName = loc.Name,
                        RobotReached = false,
                        Label = "SKIPPED — robot could not reach location"
                    });
                    skipped++;
                    continue;
                }

                // 2. Holder detektálás
                ISensorHolder holder = null;
                ITxLocatableObject holderLoc = null;

                foreach (ITxObject tool in _robot.MountedTools)
                {
                    var comp = tool as TxComponent;
                    if (comp == null) continue;

                    var instance = CreateHolderInstance(comp.Name);
                    if (instance == null) continue;

                    holder = instance;
                    holderLoc = comp;
                    break;
                }

                if (holder == null || holderLoc == null)
                {
                    pointResults.Add(new ConstellationPointResult
                    {
                        LocationName = loc.Name,
                        RobotReached = true,
                        Label = "SKIPPED — no recognized sensor holder mounted on robot"
                    });
                    skipped++;
                    continue;
                }

                // 3. Collision check
                var collisionResult = RobotCollisionCheck.CheckCollision(
                    _robot, loc.Name, _collisionObjects);

                if (collisionResult.HasCollision)
                {
                    pointResults.Add(new ConstellationPointResult
                    {
                        LocationName = loc.Name,
                        RobotReached = true,
                        HasCollision = true,
                        Label = "NOK — COLLISION"
                    });
                    nok++;
                    continue;
                }

                // 4. Visibility check
                var visibility = ConstellationVisibilityChecker.Check(
                    holderLoc, holder, _trackerWorld, _tracker,
                    visComponents, _robot);

                // 5. Criteria
                var criteria = ConstellationCriteriaEngine.Evaluate(visibility);

                if (criteria.IsOk) ok++; else nok++;

                pointResults.Add(new ConstellationPointResult
                {
                    LocationName = loc.Name,
                    RobotReached = true,
                    HasCollision = false,
                    Visibility = visibility,
                    Criteria = criteria,
                    Label = criteria.Label
                });
            }

            return new ConstellationPathResult
            {
                ProgramName = program.Name,
                PointResults = pointResults,
                OkCount = ok,
                NokCount = nok,
                SkippedCount = skipped
            };
        }

        // ── Private: holder factory ───────────────────────────────

        /// <summary>
        /// Returns an ISensorHolder instance for the given type ID.
        /// Add new holder types here as the catalog grows.
        /// </summary>
        private static ISensorHolder CreateHolderInstance(string componentName)
        {
            if (string.IsNullOrEmpty(componentName)) return null;
            string name = componentName.ToLower();
            if (name.Contains("01-03944-10"))
                return new SensorHolder_Perc_01_03944_10();
            return null;
        }
    }
}