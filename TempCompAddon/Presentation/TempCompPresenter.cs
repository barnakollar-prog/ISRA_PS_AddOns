using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Tecnomatix.Engineering;
using ISRA.Core.UI;
using ISRA.Core.Domain;
using ISRA.Calculations.TempComp;
using ISRA.Calculations.TempComp.Domain;
using ISRA.Calculations.TempComp.Domain.Results;
using ISRA.Calculations.TempComp.RobotConfiguration;
using ISRA.Calculations.TempComp.Services;

namespace TempCompAddon.Presentation
{
    /// <summary>
    /// Presenter for TempComp addon following MVP pattern.
    /// Handles business logic and coordinates between services and view.
    /// </summary>
    public class TempCompPresenter : IPresenter
    {
        private readonly ITempCompView _view;

        public TempCompPresenter(ITempCompView view)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
        }

        /// <summary>
        /// Executes the TempComp analysis.
        /// </summary>
        public void Analyze()
        {
            // Validate inputs
            if (_view.BodyPrograms.Count == 0)
            {
                _view.ShowError("Please select at least one Bodypart Path.", "Missing Input");
                return;
            }

            if (_view.TempCompPrograms.Count == 0)
            {
                _view.ShowError("Please select at least one Temp Comp Path.", "Missing Input");
                return;
            }

            if (_view.SelectedRobot == null)
            {
                _view.ShowError("Please select a Robot.", "Missing Input");
                return;
            }

            try
            {
                // Get robot configuration
                IRobotConfiguration robotConfig = GetRobotConfiguration();

                // Read poses using PoseReader service
                var poseReader = new PoseReader(_view.SelectedRobot);

                var bodyPoses = poseReader.ReadPosesFromPrograms(
                    _view.BodyPrograms,
                    MeasurementPointFilter.BodyPrefixes);

                var tempCompPoses = poseReader.ReadPosesFromPrograms(
                    _view.TempCompPrograms,
                    MeasurementPointFilter.TcPrefixes);

                // Validate poses were found
                if (bodyPoses.Count == 0)
                {
                    _view.ShowError("No poses found in Bodypart Paths.", "Error");
                    return;
                }

                if (tempCompPoses.Count == 0)
                {
                    _view.ShowError("No poses found in Temp Comp Paths.", "Error");
                    return;
                }

                // Create validation input
                var input = new TempCompValidationInput
                {
                    BodyPoses = bodyPoses,
                    TempCompPoses = tempCompPoses,
                    RobotConfiguration = robotConfig,
                    MaxAngleThreshold = _view.MaxAngleThreshold
                };

                // Run analysis
                var analyzer = new TempCompAnalyzer();
                var report = analyzer.Analyze(input);

                // Calculate nearest TC points
                var nearestResults = analyzer.CalculateNearestTcPoints(input);

                // Calculate statistics
                var bodyStats = analyzer.CalculateStatistics(bodyPoses);
                var tcStats = analyzer.CalculateStatistics(tempCompPoses);

                // Display results through view
                _view.DisplayValidationResults(report);
                _view.DisplayNearestTcResults(nearestResults, input.MaxAngleThreshold, robotConfig);
                _view.DisplayRawData(bodyPoses, tempCompPoses, robotConfig, input.MaxAngleThreshold);
            }
            catch (Exception ex)
            {
                _view.ShowError($"Analysis failed: {ex.Message}", "Error");
            }
        }

        /// <summary>
        /// Gets the robot configuration based on view selection.
        /// </summary>
        private IRobotConfiguration GetRobotConfiguration()
        {
            switch (_view.SelectedRobotType)
            {
                case "Kuka":
                    return new KukaConfiguration();
                case "ABB":
                    return new AbbConfiguration();
                case "Fanuc":
                default:
                    return new FanucConfiguration();
            }
        }
    }

    /// <summary>
    /// Interface for TempComp view (form).
    /// Defines what the presenter needs from the view.
    /// </summary>
    public interface ITempCompView
    {
        // Input properties
        List<TxWeldOperation> BodyPrograms { get; }
        List<TxWeldOperation> TempCompPrograms { get; }
        TxRobot SelectedRobot { get; }
        string SelectedRobotType { get; }
        double MaxAngleThreshold { get; }

        // Display methods
        void DisplayValidationResults(AnalysisReport report);
        void DisplayNearestTcResults(List<NearestTcResult> results, double threshold, IRobotConfiguration config);
        void DisplayRawData(List<RobotPose> bodyPoses, List<RobotPose> tcPoses, IRobotConfiguration config, double threshold);

        // Error handling
        void ShowError(string message, string title);
    }
}
