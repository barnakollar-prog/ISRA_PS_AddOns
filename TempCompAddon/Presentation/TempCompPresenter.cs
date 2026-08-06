using ISRA.Calculations.TempComp;
using ISRA.Calculations.TempComp.Domain;
using ISRA.Calculations.TempComp.Domain.Results;
using ISRA.Calculations.TempComp.RobotConfiguration;
using ISRA.Calculations.TempComp.Services;
using ISRA.Core.Domain;
using ISRA.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Tecnomatix.Engineering;
using Tecnomatix.Engineering.Olp;
using Tecnomatix.Engineering.Olp.OLP_Utilities;

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

    internal static class NativeMethods
    {
        private const int SW_RESTORE = 9;
        private const uint INPUT_KEYBOARD = 1;
        private const uint KEYEVENTF_KEYUP = 0x0002;
        private const ushort VK_MENU = 0x12;
        private const ushort VK_G = 0x47;

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public uint type;
            public InputUnion U;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct InputUnion
        {
            [FieldOffset(0)]
            public KEYBDINPUT ki;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        public static void SendAltG(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero)
                return;

            ShowWindow(hWnd, SW_RESTORE);
            SetForegroundWindow(hWnd);
            System.Threading.Thread.Sleep(50);

            INPUT[] inputs =
            {
                new INPUT
                {
                    type = INPUT_KEYBOARD,
                    U = new InputUnion
                    {
                        ki = new KEYBDINPUT { wVk = VK_MENU, wScan = 0, dwFlags = 0, time = 0, dwExtraInfo = IntPtr.Zero }
                    }
                },
                new INPUT
                {
                    type = INPUT_KEYBOARD,
                    U = new InputUnion
                    {
                        ki = new KEYBDINPUT { wVk = VK_G, wScan = 0, dwFlags = 0, time = 0, dwExtraInfo = IntPtr.Zero }
                    }
                },
                new INPUT
                {
                    type = INPUT_KEYBOARD,
                    U = new InputUnion
                    {
                        ki = new KEYBDINPUT { wVk = VK_G, wScan = 0, dwFlags = KEYEVENTF_KEYUP, time = 0, dwExtraInfo = IntPtr.Zero }
                    }
                },
                new INPUT
                {
                    type = INPUT_KEYBOARD,
                    U = new InputUnion
                    {
                        ki = new KEYBDINPUT { wVk = VK_MENU, wScan = 0, dwFlags = KEYEVENTF_KEYUP, time = 0, dwExtraInfo = IntPtr.Zero }
                    }
                }
            };

            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
        }
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

            // 1. Rule-based evaluation
            var service = new TempCompEvaluationService();
            var ruleResult = service.Evaluate(
                _lastReport,
                _lastGapResult,
                _lastBodyPoses,
                _lastTempCompPoses,
                _lastRobotConfig);

            // 2. JSON export for Copilot CLI
            var jsonExporter = new TempCompJsonExporter();
            string robotType = _view.SelectedRobotType ?? "Unknown";
            string json = jsonExporter.ExportForEvaluation(
                _lastReport,
                _lastGapResult,
                robotType);

            // 3. Copilot CLI call
            string copilotOutput = null;
            string tokenInfo = null;
            try
            {
                string prompt =
                    "You are a robot measurement expert. Analyze this TempComp validation result " +
                    "and provide concrete optimization suggestions. Focus on which TC measurement points " +
                    "should be modified and in which direction, and which body points could potentially " +
                    "be measured from an alternative sensor position to reduce the required range. " +
                    "Be concise and practical. Data: " + json;

                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "copilot",
                    Arguments = $"--prompt \"{prompt.Replace("\"", "\\\"")}\" --allow-all-tools --silent --no-color --no-auto-update --disable-builtin-mcps",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    StandardOutputEncoding = System.Text.Encoding.UTF8  // ← ÚJ
                };

                using (var process = System.Diagnostics.Process.Start(psi))
                {
                    var stdOutTask = process.StandardOutput.ReadToEndAsync();
                    var stdErrTask = process.StandardError.ReadToEndAsync();
                    process.WaitForExit();
                    copilotOutput = stdOutTask.Result;
                    stdErrTask.Wait();
                }

                // Extract token info from output
                var lines = copilotOutput.Split('\n');
                var tokenLine = Array.Find(lines, l => l.TrimStart().StartsWith("Tokens"));
                var creditLine = Array.Find(lines, l => l.TrimStart().StartsWith("AI Credits"));
                if (tokenLine != null || creditLine != null)
                    tokenInfo = $"{creditLine?.Trim()} | {tokenLine?.Trim()}";

                // Remove metadata lines from output
                copilotOutput = string.Join("\n", lines
                    .Where(l => !l.StartsWith("AI Credits") &&
                                !l.StartsWith("Tokens") &&
                                !l.StartsWith("Changes") &&
                                !l.StartsWith("Resume"))
                    .ToArray()).Trim();
            }
            catch (Exception ex)
            {
                copilotOutput = $"Copilot CLI not available: {ex.Message}";
            }

            // 4. Show result
            _view.ShowEvaluationResult(ruleResult, copilotOutput, tokenInfo);
        }
        public void JumpToLocation(string pathName, string poseName, bool isBodyPoint = false)
        {
            try
            {
                // Find the location in TC programs
                ITxRoboticLocationOperation targetLoc = null;
                var programs = isBodyPoint
                    ? (IEnumerable<TxWeldOperation>)_view.BodyPrograms
                    : _view.TempCompPrograms;
                foreach (var program in programs)
                {
                    if (program.Name != pathName) continue;

                    var locations = program.GetAllDescendants(
                        new TxTypeFilter(typeof(ITxRoboticLocationOperation)));

                    foreach (ITxObject obj in locations)
                    {
                        var loc = obj as ITxRoboticLocationOperation;
                        if (loc != null && loc.Name == poseName)
                        {
                            targetLoc = loc;
                            break;
                        }
                    }
                    if (targetLoc != null) break;
                }

                if (targetLoc == null)
                {
                    _view.ShowError(
                        $"Location '{poseName}' not found in path '{pathName}'.",
                        "Jump Failed");
                    return;
                }

                // Jump to location
                var followMode = new TxOlpRobotFollowMode(_view.SelectedRobot);
                bool reached = followMode.JumpRobotToLocation(targetLoc);
                TxApplication.RefreshDisplay();

                if (reached)
                {
                    // 1. Location kijelölése az Operation Tree-ben
                    var selection = new TxObjectList();
                    selection.Add(targetLoc);
                    TxApplication.ActiveSelection.SetItems(selection);
                    TxApplication.RefreshDisplay();

                    // 2. PS főablaknak fókusz
                    var psProcess = System.Diagnostics.Process.GetProcessesByName("tune").FirstOrDefault();
                    if (psProcess != null && psProcess.MainWindowHandle != IntPtr.Zero)
                    {
                        SendAltGToWindow(psProcess.MainWindowHandle);
                    }
                }
                else
                {
                    _view.ShowError($"Robot cannot reach location '{poseName}'.", "Jump Failed");
                }
            }
            catch (Exception ex)
            {
                _view.ShowError($"Jump error: {ex.Message}", "Error");
            }
        }

        private static void SendAltGToWindow(IntPtr hWnd)
        {
            NativeMethods.SendAltG(hWnd);
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

    internal static class NativeMethods
    {
        private const int SW_RESTORE = 9;
        private const uint INPUT_KEYBOARD = 1;
        private const uint KEYEVENTF_KEYUP = 0x0002;
        private const ushort VK_MENU = 0x12;
        private const ushort VK_G = 0x47;

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public uint type;
            public InputUnion U;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct InputUnion
        {
            [FieldOffset(0)]
            public KEYBDINPUT ki;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        public static void SendAltG(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero)
                return;

            ShowWindow(hWnd, SW_RESTORE);
            SetForegroundWindow(hWnd);
            System.Threading.Thread.Sleep(50);

            INPUT[] inputs =
            {
                new INPUT
                {
                    type = INPUT_KEYBOARD,
                    U = new InputUnion
                    {
                        ki = new KEYBDINPUT { wVk = VK_MENU, wScan = 0, dwFlags = 0, time = 0, dwExtraInfo = IntPtr.Zero }
                    }
                },
                new INPUT
                {
                    type = INPUT_KEYBOARD,
                    U = new InputUnion
                    {
                        ki = new KEYBDINPUT { wVk = VK_G, wScan = 0, dwFlags = 0, time = 0, dwExtraInfo = IntPtr.Zero }
                    }
                },
                new INPUT
                {
                    type = INPUT_KEYBOARD,
                    U = new InputUnion
                    {
                        ki = new KEYBDINPUT { wVk = VK_G, wScan = 0, dwFlags = KEYEVENTF_KEYUP, time = 0, dwExtraInfo = IntPtr.Zero }
                    }
                },
                new INPUT
                {
                    type = INPUT_KEYBOARD,
                    U = new InputUnion
                    {
                        ki = new KEYBDINPUT { wVk = VK_MENU, wScan = 0, dwFlags = KEYEVENTF_KEYUP, time = 0, dwExtraInfo = IntPtr.Zero }
                    }
                }
            };

            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
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

        void ShowEvaluationResult(EvaluationResult result, string copilotOutput, string tokenInfo);

        // Error handling
        void ShowError(string message, string title);
    }
}
