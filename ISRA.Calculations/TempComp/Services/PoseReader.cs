using System;
using System.Collections.Generic;
using Tecnomatix.Engineering;
using ISRA.Calculations.TempComp.Domain;

namespace ISRA.Calculations.TempComp.Services
{
    public class PoseReader
    {
        public sealed class SnapshotReconcileResult
        {
            public int ProcessedCount { get; set; }
            public int TouchedCount { get; set; }
            public int RestoredCount { get; set; }
            public int MissingSnapshotCount { get; set; }
            public List<string> RestoredKeys { get; set; }
            public List<RobotPose> RestoredPoses { get; set; }

            public SnapshotReconcileResult()
            {
                RestoredKeys = new List<string>();
                RestoredPoses = new List<RobotPose>();
            }
        }

        private sealed class LocationContext
        {
            public ITxRoboticLocationOperation Location { get; set; }
            public string PathName { get; set; }
        }

        private readonly TxRobot _robot;

        public PoseReader(TxRobot robot)
        {
            _robot = robot ?? throw new ArgumentNullException(nameof(robot));
        }

        public List<RobotPose> ReadPosesFromProgram(
            TxWeldOperation program,
            FilterMode filterMode,
            string[] namePrefixes,
            string[] olpKeywords = null)
        {
            var result = new List<RobotPose>();

            if (program == null)
                return result;

            string pathName = program.Name;

            foreach (var locationContext in GetLocationsFromWeldOperation(program))
            {
                var loc = locationContext.Location;
                if (loc == null) continue;

                // Apply filter based on mode
                if (filterMode == FilterMode.Auto)
                {
                    if (!MeasurementPointFilter.IsMeasurementPoint(loc, namePrefixes))
                        continue;
                }
                else if (filterMode == FilterMode.Custom)
                {
                    if (!MeasurementPointFilter.IsMeasurementPoint(loc, namePrefixes, olpKeywords))
                        continue;
                }
                // NoFilter: minden location bekerül

                try
                {
                    TxRobotConfigurationData originalConfig = null;
                    try
                    {
                        originalConfig = loc.RobotConfigurationData;
                    }
                    catch
                    {
                    }

                    try
                    {
                        loc.RobotConfigurationData = null;
                    }
                    catch
                    {
                    }

                    var poseData = _robot.GetPoseAtLocation(loc);

                    try
                    {
                        if (originalConfig != null)
                        {
                            loc.RobotConfigurationData = originalConfig;
                        }
                    }
                    catch
                    {
                    }

                    if (poseData?.JointValues == null)
                    {
                        continue;
                    }

                    var joints = poseData.JointValues;
                    if (joints.Count < 6)
                    {
                        continue;
                    }

                    result.Add(new RobotPose
                    {
                        Name = loc.Name,
                        PathName = pathName ?? string.Empty,
                        ConfigurationSignature = loc.RobotConfigurationData == null
                            ? string.Empty
                            : loc.RobotConfigurationData.ToString(),
                        J1 = (double)joints[0] * (180.0 / Math.PI),
                        J2 = (double)joints[1] * (180.0 / Math.PI),
                        J3 = (double)joints[2] * (180.0 / Math.PI),
                        J4 = (double)joints[3] * (180.0 / Math.PI),
                        J5 = (double)joints[4] * (180.0 / Math.PI),
                        J6 = (double)joints[5] * (180.0 / Math.PI),
                    });
                }
                catch
                {
                    // Silently skip invalid locations
                }
            }

            return result;
        }

        public Dictionary<string, TxRobotConfigurationData> CreatePoseSnapshot(
            IEnumerable<ITxObject> operations,
            FilterMode filterMode,
            string[] namePrefixes,
            string[] olpKeywords = null)
        {
            var snapshot = new Dictionary<string, TxRobotConfigurationData>(StringComparer.OrdinalIgnoreCase);
            foreach (var locationContext in GetLocationsFromOperations(operations))
            {
                var loc = locationContext.Location;
                if (loc == null)
                {
                    continue;
                }

                if (!PassesFilter(loc, filterMode, namePrefixes, olpKeywords))
                {
                    continue;
                }

                try
                {
                    var key = RobotPose.BuildLocationKey(locationContext.PathName, loc.Name);
                    snapshot[key] = loc.RobotConfigurationData;
                }
                catch
                {
                    // Skip invalid locations
                }
            }

            return snapshot;
        }

        public SnapshotReconcileResult ReconcileLocationsWithSnapshot(
            IEnumerable<ITxObject> operations,
            IDictionary<string, TxRobotConfigurationData> snapshot,
            FilterMode filterMode,
            string[] namePrefixes,
            string[] olpKeywords = null,
            bool useTaughtLocation = false)
        {
            var result = new SnapshotReconcileResult();
            if (operations == null || snapshot == null || snapshot.Count == 0)
            {
                return result;
            }

            foreach (var locationContext in GetLocationsFromOperations(operations))
            {
                var loc = locationContext.Location;
                if (loc == null)
                {
                    continue;
                }

                if (!PassesFilter(loc, filterMode, namePrefixes, olpKeywords))
                {
                    continue;
                }

                var key = RobotPose.BuildLocationKey(locationContext.PathName, loc.Name);
                result.ProcessedCount++;

                if (!snapshot.TryGetValue(key, out var originalConfig))
                {
                    result.MissingSnapshotCount++;
                    continue;
                }

                try
                {
                    if (useTaughtLocation)
                    {
                        loc.TeachCurrentRobotConfigurationUsingTaughtLocation();
                    }
                    else
                    {
                        loc.TeachCurrentRobotConfiguration();
                    }

                    result.TouchedCount++;

                    loc.RobotConfigurationData = originalConfig;
                    result.RestoredCount++;
                    result.RestoredKeys.Add(key);

                    var poseAfterRestore = ReadPoseFromLocation(loc, locationContext.PathName);
                    if (poseAfterRestore != null)
                    {
                        result.RestoredPoses.Add(poseAfterRestore);
                    }
                }
                catch
                {
                    continue;
                }
            }

            return result;
        }

        public List<ITxRoboticLocationOperation> CollectLocationsFromOperations(IEnumerable<ITxObject> operations)
        {
            var result = new List<ITxRoboticLocationOperation>();
            if (operations == null)
            {
                return result;
            }

            foreach (var ctx in GetLocationsFromOperations(operations))
            {
                if (ctx.Location != null)
                {
                    result.Add(ctx.Location);
                }
            }

            return result;
        }

        private IEnumerable<LocationContext> GetLocationsFromWeldOperation(TxWeldOperation program)
        {
            if (program == null)
            {
                yield break;
            }

            var pathName = program.Name;
            var locations = program.GetAllDescendants(new TxTypeFilter(typeof(ITxRoboticLocationOperation)));
            foreach (ITxObject item in locations)
            {
                var loc = item as ITxRoboticLocationOperation;
                if (loc == null)
                {
                    continue;
                }

                yield return new LocationContext
                {
                    Location = loc,
                    PathName = pathName
                };
            }
        }

        private IEnumerable<LocationContext> GetLocationsFromOperations(IEnumerable<ITxObject> operations)
        {
            if (operations == null)
            {
                yield break;
            }

            var visited = new HashSet<ITxObject>();
            var stack = new Stack<ITxObject>();

            foreach (var op in operations)
            {
                if (op != null)
                {
                    stack.Push(op);
                }
            }

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                if (current == null || visited.Contains(current))
                {
                    continue;
                }

                visited.Add(current);

                var weld = current as TxWeldOperation;
                if (weld != null)
                {
                    foreach (var item in GetLocationsFromWeldOperation(weld))
                    {
                        yield return item;
                    }

                    continue;
                }

                var location = current as ITxRoboticLocationOperation;
                if (location != null)
                {
                    yield return new LocationContext
                    {
                        Location = location,
                        PathName = ResolvePathName(current)
                    };
                }

                var compound = current as ITxCompoundOperation;
                if (compound != null)
                {
                    var children = compound.GetAllDescendants(new TxTypeFilter(typeof(ITxOperation)));
                    foreach (ITxObject child in children)
                    {
                        if (child != null)
                        {
                            stack.Push(child);
                        }
                    }
                }
            }
        }

        private RobotPose ReadPoseFromLocation(ITxRoboticLocationOperation loc, string pathName)
        {
            var poseData = _robot.GetPoseAtLocation(loc);
            if (poseData?.JointValues == null)
            {
                return null;
            }

            var joints = poseData.JointValues;
            if (joints.Count < 6)
            {
                return null;
            }

            return new RobotPose
            {
                Name = loc.Name,
                PathName = pathName ?? string.Empty,
                ConfigurationSignature = loc.RobotConfigurationData == null
                    ? string.Empty
                    : loc.RobotConfigurationData.ToString(),
                J1 = (double)joints[0] * (180.0 / Math.PI),
                J2 = (double)joints[1] * (180.0 / Math.PI),
                J3 = (double)joints[2] * (180.0 / Math.PI),
                J4 = (double)joints[3] * (180.0 / Math.PI),
                J5 = (double)joints[4] * (180.0 / Math.PI),
                J6 = (double)joints[5] * (180.0 / Math.PI),
            };
        }

        private string ResolvePathName(ITxObject operation)
        {
            return operation == null ? string.Empty : operation.Name;
        }

        private bool PassesFilter(
            ITxRoboticLocationOperation loc,
            FilterMode filterMode,
            string[] namePrefixes,
            string[] olpKeywords)
        {
            if (loc == null)
            {
                return false;
            }

            if (filterMode == FilterMode.Auto)
            {
                return MeasurementPointFilter.IsMeasurementPoint(loc, namePrefixes);
            }

            if (filterMode == FilterMode.Custom)
            {
                return MeasurementPointFilter.IsMeasurementPoint(loc, namePrefixes, olpKeywords);
            }

            return true;
        }

        public List<RobotPose> ReadPosesFromPrograms(
            IEnumerable<TxWeldOperation> programs,
            FilterMode filterMode,
            string[] namePrefixes,
            string[] olpKeywords = null)
        {
            var result = new List<RobotPose>();

            if (programs == null)
                return result;

            foreach (var program in programs)
            {
                result.AddRange(ReadPosesFromProgram(program, filterMode, namePrefixes, olpKeywords));
            }

            return result;
        }
    }
}