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
        private AnalysisReport _lastReport;
        private List<NearestTcResult> _lastNearestResults;
        private List<RobotPose> _lastBodyPoses;
        private List<RobotPose> _lastTempCompPoses;
        private PoseStatistics _lastBodyStats;
        private PoseStatistics _lastTempCompStats;
        private IRobotConfiguration _lastRobotConfig;
        private double _lastThreshold;
        private GapAnalysisResult _lastGapResult;

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
                    _view.FilterMode,
                    _view.FilterMode == FilterMode.Custom
                        ? _view.CustomBodyPrefixes
                        : MeasurementPointFilter.BodyPrefixes,
                    _view.FilterMode == FilterMode.Custom
                        ? _view.CustomOlpKeywords
                        : null);

                var tempCompPoses = poseReader.ReadPosesFromPrograms(
                    _view.TempCompPrograms,
                    _view.FilterMode,
                    _view.FilterMode == FilterMode.Custom
                        ? _view.CustomTcPrefixes
                        : MeasurementPointFilter.TcPrefixes,
                    _view.FilterMode == FilterMode.Custom
                        ? _view.CustomOlpKeywords
                        : null);

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

                // Run gap analysis
                var gapService = new GapAnalysisService();
                var gapResult = gapService.Analyze(
                    tempCompPoses,
                    robotConfig,
                    _view.J2GapThreshold,
                    _view.J3GapThreshold,
                    _view.J4GapThreshold,
                    _view.J5GapThreshold,
                    _view.J6GapThreshold);

                
                

                // Store results for export
                _lastReport = report;
                _lastNearestResults = nearestResults;
                _lastBodyPoses = bodyPoses;
                _lastTempCompPoses = tempCompPoses;
                _lastBodyStats = bodyStats;
                _lastTempCompStats = tcStats;
                _lastRobotConfig = robotConfig;
                _lastThreshold = input.MaxAngleThreshold;
                _lastGapResult = gapResult;

                // Display results through view
                _view.DisplayValidationResults(report);
                _view.DisplayNearestTcResults(nearestResults, input.MaxAngleThreshold, robotConfig);
                _view.DisplayRawData(bodyPoses, tempCompPoses, robotConfig, input.MaxAngleThreshold);
                _view.DisplayGapAnalysis(gapResult);
            }
            catch (Exception ex)
            {
                _view.ShowError($"Analysis failed: {ex.Message}", "Error");
            }
        }

        /// <summary>
        /// Executes the export workflow.
        /// </summary>
        public void Export()
        {
            // Validate that analysis has been run
            if (!_view.HasResults)
            {
                _view.ShowError("No results to export. Please run the analysis first.", "No Data");
                return;
            }

            try
            {
                // Prepare export data
                var exportData = PrepareExportData();

                // Delegate to view to show export dialog
                _view.ShowExportDialog(exportData);
            }
            catch (Exception ex)
            {
                _view.ShowError($"Export preparation failed: {ex.Message}", "Export Error");
            }
        }
        public void Evaluate()
        {
            if (_lastReport == null || _lastGapResult == null)
            {
                _view.ShowError("Please run the analysis first.", "No Data");
                return;
            }

            var service = new TempCompEvaluationService();
            var result = service.Evaluate(
                _lastReport,
                _lastGapResult,
                _lastBodyPoses,
                _lastTempCompPoses,
                _lastRobotConfig);

            _view.ShowEvaluationResult(result);
        }

        /// <summary>
        /// Prepares export data from the last analysis results.
        /// </summary>
        private TempCompExportData PrepareExportData()
        {
            return new TempCompExportData
            {
                ValidationReport = _lastReport,
                NearestTcResults = _lastNearestResults,
                BodyPoses = _lastBodyPoses,
                TempCompPoses = _lastTempCompPoses,
                RobotConfiguration = _lastRobotConfig,
                MaxAngleThreshold = _lastThreshold,
                BodyStatistics = _lastBodyStats,
                TempCompStatistics = _lastTempCompStats,
                GapAnalysis = _lastGapResult
            };
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

        // Gap Analysis thresholds
        double J2GapThreshold { get; }
        double J3GapThreshold { get; }
        double J4GapThreshold { get; }
        double J5GapThreshold { get; }
        double J6GapThreshold { get; }

        // Filter properties
        FilterMode FilterMode { get; }                  // ← ÚJ
        string[] CustomBodyPrefixes { get; }            // ← ÚJ
        string[] CustomTcPrefixes { get; }              // ← ÚJ
        string[] CustomOlpKeywords { get; }             // ← ÚJ


        // Display methods
        void DisplayValidationResults(AnalysisReport report);
        void DisplayNearestTcResults(List<NearestTcResult> results, double threshold, IRobotConfiguration config);
        void DisplayRawData(List<RobotPose> bodyPoses, List<RobotPose> tcPoses, IRobotConfiguration config, double threshold);
        void DisplayGapAnalysis(GapAnalysisResult result);

        // Export support
        bool HasResults { get; }
        void ShowExportDialog(TempCompExportData data);

        void ShowEvaluationResult(EvaluationResult result);

        // Error handling
        void ShowError(string message, string title);
    }
}
