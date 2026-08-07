using System;
using System.Collections;
using System.Collections.Generic;
using Tecnomatix.Engineering;

namespace ISRA.RobotProgramManipulator
{
    internal sealed class PrePositionProcessor
    {
        private const string PrePositionPrefix = "PrePos_";

        public PrePositionProcessingResult Process(
            IEnumerable<ITxRoboticOrderedCompoundOperation> programs,
            PrePositionOptions options)
        {
            if (programs == null)
            {
                throw new ArgumentNullException("programs");
            }

            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            ValidateOptions(options);

            var result = new PrePositionProcessingResult();
            TxUndoTransactionManager undoManager = TxApplication.ActiveUndoManager;
            bool transactionStarted = false;
            try
            {
                if (undoManager != null)
                {
                    undoManager.StartTransaction();
                    transactionStarted = true;
                }

                foreach (ITxRoboticOrderedCompoundOperation program in programs)
                {
                    try
                    {
                        if (program == null || !((ITxObject)program).IsValid())
                        {
                            result.AddError("A selected robot program is no longer valid.");
                            continue;
                        }

                        ProcessProgram(program, options, result);
                    }
                    catch (Exception exception)
                    {
                        string programName = program == null ? "Unknown program" : ((ITxObject)program).Name;
                        result.AddError(programName + ": " + exception.Message);
                    }
                }
            }
            finally
            {
                if (transactionStarted)
                {
                    undoManager.EndTransaction();
                }
            }

            TxApplication.RefreshDisplay();
            return result;
        }

        private static void ValidateOptions(PrePositionOptions options)
        {
            if (options.AxisOffsetsDegrees == null || options.AxisOffsetsDegrees.Length != 6)
            {
                throw new ArgumentException("Exactly six robot axis offsets are required.", "options");
            }

            for (int index = 0; index < options.AxisOffsetsDegrees.Length; index++)
            {
                double offset = options.AxisOffsetsDegrees[index];
                if (double.IsNaN(offset) || double.IsInfinity(offset) || offset < -360.0 || offset > 360.0)
                {
                    throw new ArgumentOutOfRangeException(
                        "options",
                        "Axis " + (index + 1) + " must be between -360 and 360 degrees.");
                }
            }
        }

        private static void ProcessProgram(
            ITxRoboticOrderedCompoundOperation program,
            PrePositionOptions options,
            PrePositionProcessingResult result)
        {
            var programObject = (ITxObject)program;
            var robot = program.Robot as TxRobot;
            var collection = program as ITxObjectCollection;
            var orderedCollection = program as ITxOrderedObjectCollection;

            if (robot == null || collection == null || orderedCollection == null)
            {
                result.AddError(programObject.Name + ": unsupported robot program type.");
                return;
            }

            IList<ITxRoboticLocationOperation> sourcePoints = GetSourcePoints(
                collection,
                orderedCollection,
                options.IncludeExistingPrePositions);
            var existingNames = GetExistingNames(collection, orderedCollection);

            foreach (ITxRoboticLocationOperation sourcePoint in sourcePoints)
            {
                ITxObject sourceObject = (ITxObject)sourcePoint;
                string newName = PrePositionPrefix + sourceObject.Name;

                if (existingNames.Contains(newName))
                {
                    result.AddSkipped(programObject.Name + " / " + sourceObject.Name +
                        ": " + newName + " already exists.");
                    continue;
                }

                ITxObject copiedObject = null;
                try
                {
                    copiedObject = CopyBeforeSource(
                        collection,
                        orderedCollection,
                        sourcePoint,
                        newName,
                        robot,
                        options.AxisOffsetsDegrees,
                        options.AddComment,
                        CreateGeneratedComment(sourceObject.Name, options));
                    existingNames.Add(newName);
                    result.AddCreated();
                }
                catch (Exception exception)
                {
                    if (copiedObject != null && copiedObject.IsValid())
                    {
                        copiedObject.Delete();
                    }

                    result.AddError(programObject.Name + " / " + sourceObject.Name +
                        ": " + exception.Message);
                }
            }
        }

        private static IList<ITxRoboticLocationOperation> GetSourcePoints(
            ITxObjectCollection collection,
            ITxOrderedObjectCollection orderedCollection,
            bool includeExistingPrePositions)
        {
            var points = new List<ITxRoboticLocationOperation>();
            for (int index = 0; index < collection.Count; index++)
            {
                var point = orderedCollection.GetChildAt(index) as ITxRoboticLocationOperation;
                if (point == null)
                {
                    continue;
                }

                string name = ((ITxObject)point).Name ?? string.Empty;
                if (name.IndexOf("HOME", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    name.IndexOf("MOVE", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    continue;
                }

                if (!includeExistingPrePositions &&
                    name.StartsWith(PrePositionPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                points.Add(point);
            }

            return points;
        }

        private static HashSet<string> GetExistingNames(
            ITxObjectCollection collection,
            ITxOrderedObjectCollection orderedCollection)
        {
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int index = 0; index < collection.Count; index++)
            {
                ITxObject child = orderedCollection.GetChildAt(index);
                if (child != null)
                {
                    names.Add(child.Name);
                }
            }

            return names;
        }

        private static ITxObject CopyBeforeSource(
            ITxObjectCollection collection,
            ITxOrderedObjectCollection orderedCollection,
            ITxRoboticLocationOperation sourcePoint,
            string newName,
            TxRobot robot,
            double[] axisOffsetsDegrees,
            bool addComment,
            string generatedComment)
        {
            ITxObject sourceObject = (ITxObject)sourcePoint;
            int sourceIndex = orderedCollection.GetIndexOfChild(sourceObject);
            ITxObject predecessor = sourceIndex > 0
                ? orderedCollection.GetChildAt(sourceIndex - 1)
                : null;

            TxPoseData adjustedPose = CreateAdjustedPose(robot, sourcePoint, axisOffsetsDegrees);
            var sourceList = new TxObjectList { sourceObject };
            if (!collection.CanPasteList(sourceList))
            {
                throw new InvalidOperationException("The point cannot be duplicated in this program.");
            }

            TxObjectList copiedObjects = collection.Paste(sourceList);
            if (copiedObjects == null || copiedObjects.Count == 0)
            {
                throw new InvalidOperationException("Process Simulate did not return a duplicated point.");
            }

            ITxObject copiedObject = copiedObjects[0];
            var copiedPoint = copiedObject as ITxRoboticLocationOperation;
            var copiedLocation = copiedObject as ITxLocatableObject;
            if (copiedPoint == null || copiedLocation == null)
            {
                copiedObject.Delete();
                throw new InvalidOperationException("The duplicated object is not a robotic location.");
            }

            try
            {
                copiedObject.Name = newName;
                copiedLocation.AbsoluteLocation = robot.GetTCPFByPoseData(adjustedPose);
                copiedPoint.RobotConfigurationData = robot.GetPoseConfiguration(adjustedPose);
                copiedPoint.TeachCurrentRobotConfigurationUsingTaughtLocation();
                ReplaceCommands(copiedObject, addComment, generatedComment);

                if (predecessor == null)
                {
                    if (!orderedCollection.CanMoveChildAfter(copiedObject, sourceObject))
                    {
                        throw new InvalidOperationException("The duplicated first point cannot be moved next to its source.");
                    }

                    orderedCollection.MoveChildAfter(copiedObject, sourceObject);

                    if (!orderedCollection.CanMoveChildAfter(sourceObject, copiedObject))
                    {
                        throw new InvalidOperationException("The duplicated first point cannot be moved before its source.");
                    }

                    orderedCollection.MoveChildAfter(sourceObject, copiedObject);
                }
                else
                {
                    if (!orderedCollection.CanMoveChildAfter(copiedObject, predecessor))
                    {
                        throw new InvalidOperationException("The duplicated point cannot be moved before its source.");
                    }

                    orderedCollection.MoveChildAfter(copiedObject, predecessor);
                }
                return copiedObject;
            }
            catch
            {
                if (copiedObject.IsValid())
                {
                    copiedObject.Delete();
                }

                throw;
            }
        }

        private static void ReplaceCommands(
            ITxObject copiedPoint,
            bool addComment,
            string generatedComment)
        {
            Type pointType = copiedPoint.GetType();
            var commandsProperty = pointType.GetProperty("Commands");
            var createCommandMethod = pointType.GetMethod(
                "CreateCommand",
                new[] { typeof(TxRoboticCommandCreationData) });

            if (commandsProperty == null || createCommandMethod == null)
            {
                throw new InvalidOperationException(
                    "The duplicated point type does not support robotic OLP commands.");
            }

            var commands = commandsProperty.GetValue(copiedPoint, null) as TxObjectList;
            if (commands == null)
            {
                throw new InvalidOperationException(
                    "Process Simulate did not return the duplicated point commands.");
            }

            var commandsToDelete = new List<ITxObject>();
            foreach (ITxObject command in commands)
            {
                commandsToDelete.Add(command);
            }

            foreach (ITxObject command in commandsToDelete)
            {
                command.Delete();
            }

            if (!addComment)
            {
                return;
            }

            var creationData = new TxRoboticCommandCreationData
            {
                Name = "Generated Pre-Position Comment",
                Text = generatedComment
            };

            if (createCommandMethod.Invoke(copiedPoint, new object[] { creationData }) == null)
            {
                throw new InvalidOperationException(
                    "Process Simulate did not create the generated OLP comment.");
            }
        }

        private static string CreateGeneratedComment(string sourcePointName, PrePositionOptions options)
        {
            string originalPointName = sourcePointName ?? string.Empty;
            while (originalPointName.StartsWith(PrePositionPrefix, StringComparison.OrdinalIgnoreCase))
            {
                originalPointName = originalPointName.Substring(PrePositionPrefix.Length);
            }

            return options.CommentDeclarationBefore +
                originalPointName.ToUpperInvariant() +
                options.CommentDeclarationAfter;
        }

        private static TxPoseData CreateAdjustedPose(
            TxRobot robot,
            ITxRoboticLocationOperation sourcePoint,
            double[] axisOffsetsDegrees)
        {
            if (axisOffsetsDegrees == null || axisOffsetsDegrees.Length != 6)
            {
                throw new InvalidOperationException("Exactly six robot axis offsets are required.");
            }

            TxPoseData sourcePose = robot.GetPoseAtLocation(sourcePoint);
            if (sourcePose == null || sourcePose.JointValues == null)
            {
                throw new InvalidOperationException("The point has no taught robot pose.");
            }

            if (sourcePose.JointValues.Count < axisOffsetsDegrees.Length)
            {
                throw new InvalidOperationException("The robot exposes fewer than six taught joint values.");
            }

            var jointValues = new ArrayList(sourcePose.JointValues.Count);
            for (int index = 0; index < sourcePose.JointValues.Count; index++)
            {
                double value = Convert.ToDouble(sourcePose.JointValues[index]);
                if (index < axisOffsetsDegrees.Length)
                {
                    value += axisOffsetsDegrees[index] * Math.PI / 180.0;
                }

                jointValues.Add(value);
            }

            var adjustedPose = new TxPoseData(sourcePose);
            adjustedPose.JointValues = jointValues;
            return adjustedPose;
        }
    }
}
