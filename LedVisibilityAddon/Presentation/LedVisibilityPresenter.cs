using System;
using System.Collections.Generic;
using Tecnomatix.Engineering;
using Tecnomatix.Engineering.DataTypes.Graphics;
using ISRA.Core.Domain;
using ISRA.Core.UI;
using ISRA.Components.AccuSite.Stars;
using ISRA.Components.AccuSite.Trackers;
using ISRA.Calculations.AccuSite;

namespace LedVisibilityAddon.Presentation
{
    /// <summary>
    /// MVP Presenter for LED Visibility analysis.
    /// </summary>
    public class LedVisibilityPresenter : IPresenter
    {
        private readonly ILedVisibilityView _view;
        private readonly Tracker920_0005 _trackerFov;
        private readonly Star515_0139 _starGeometry;

        public LedVisibilityPresenter(ILedVisibilityView view)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _trackerFov = new Tracker920_0005();
            _starGeometry = new Star515_0139();
        }

        /// <summary>
        /// Execute the visibility analysis.
        /// </summary>
        public void Analyze()
        {
            // Clear previous results
            _view.ClearResults();
            _view.DeleteVisualizations();

            // Validate inputs
            if (_view.SelectedTracker == null)
            {
                _view.ShowError("Please select the Tracker.", "Missing Input");
                return;
            }

            var trackerLoc = _view.SelectedTracker as ITxLocatableObject;
            if (trackerLoc == null)
            {
                _view.ShowError("Selected tracker is not locatable.", "Invalid Input");
                return;
            }

            var selectedStars = _view.SelectedStars;
            if (selectedStars.Count == 0)
            {
                _view.ShowError("Please select at least one Star.", "Missing Input");
                return;
            }

            TxTransformation trackerWorld = trackerLoc.AbsoluteLocation;
            double maxAngle = _view.MaxAngle;

            var starResults = new List<StarAnalysisResult>();
            var localPositions = new List<TxVector>();
            var worldPositions = new List<TxVector>();
            bool allOK = true;

            // Analyze each star
            foreach (var starObj in selectedStars)
            {
                var starLoc = starObj as ITxLocatableObject;
                if (starLoc == null) continue;

                // Get LED world position
                TxVector ledWorldPos = _starGeometry.GetLedWorldPosition(starLoc);
                TxVector localPt = _trackerFov.ToLocalCoordinates(ledWorldPos, trackerWorld);
                localPositions.Add(localPt);
                worldPositions.Add(ledWorldPos);

                // Criterion 1: Position zone
                var zone = _trackerFov.GetPositionZone(localPt);
                string zoneLabel = _trackerFov.GetPositionZoneLabel(localPt);
                string zoneName = _trackerFov.GetZoneName(localPt);

                // Criterion 2: Emitter visibility
                var emitterResult = GeometryCalculations.CheckStarEmitterVisibility(
                    starLoc, trackerWorld, maxAngle);

                // Criterion 3: Line of sight check
                var losResult = CollisionCheck.CheckLineOfSight(
                    starLoc, trackerWorld, _view.CylinderComponents);

                bool starOK = zone != PositionZone.NOK &&
                              emitterResult.IsValid &&
                              losResult.IsValid;

                if (!starOK) allOK = false;

                var result = new StarAnalysisResult
                {
                    StarObject = starObj,
                    StarName = starObj.Name,
                    LocalPosition = localPt,
                    WorldPosition = ledWorldPos,
                    Zone = zone,
                    ZoneLabel = zoneLabel,
                    ZoneName = zoneName,
                    EmitterResult = emitterResult,
                    LineOfSightResult = losResult,
                    IsValid = starOK,
                    MaxAngle = maxAngle
                };

                starResults.Add(result);

                // Color star in Process Simulate
                _view.ColorizeStarInScene(starObj, zone, emitterResult.IsValid);
            }

            // Display star results
            _view.DisplayStarResults(starResults);

            // Triangle analysis (only if 3 stars and all OK)
            if (starResults.Count == 3 && allOK)
            {
                var tri = GeometryCalculations.CalculateTriangleHeight(
                    localPositions[0], localPositions[1], localPositions[2]);

                var triangleResult = new TriangleAnalysisResult
                {
                    Height = tri.Height,
                    SideAB = tri.SideAB,
                    SideBC = tri.SideBC,
                    SideCA = tri.SideCA,
                    LongestSide = tri.LongestSide,
                    IsValid = tri.IsValid,
                    WorldPositions = worldPositions
                };

                _view.DisplayTriangleResult(triangleResult);
            }
            else if (starResults.Count > 0 && !allOK)
            {
                _view.DisplayTriangleSkipped("Not all stars OK");
            }

            _view.RefreshDisplay();
        }
    }

    /// <summary>
    /// View interface for LED Visibility addon.
    /// </summary>
    public interface ILedVisibilityView
    {
        /// <summary>
        /// The selected tracker object.
        /// </summary>
        ITxObject SelectedTracker { get; }

        /// <summary>
        /// List of selected star objects.
        /// </summary>
        List<ITxObject> SelectedStars { get; }

        /// <summary>
        /// Maximum angle threshold (degrees).
        /// </summary>
        double MaxAngle { get; }

        /// <summary>
        /// Cylinder components for collision checking.
        /// </summary>
        List<TxComponent> CylinderComponents { get; }

        /// <summary>
        /// Clear all result displays.
        /// </summary>
        void ClearResults();

        /// <summary>
        /// Delete all visualizations (triangles, emitters, cylinders).
        /// </summary>
        void DeleteVisualizations();

        /// <summary>
        /// Display the analysis results for all stars.
        /// </summary>
        void DisplayStarResults(List<StarAnalysisResult> results);

        /// <summary>
        /// Display the triangle analysis result.
        /// </summary>
        void DisplayTriangleResult(TriangleAnalysisResult result);

        /// <summary>
        /// Display a message that triangle analysis was skipped.
        /// </summary>
        void DisplayTriangleSkipped(string reason);

        /// <summary>
        /// Colorize a star object in the scene based on its validation status.
        /// </summary>
        void ColorizeStarInScene(ITxObject starObj, PositionZone zone, bool emitterValid);

        /// <summary>
        /// Refresh the Process Simulate display.
        /// </summary>
        void RefreshDisplay();

        /// <summary>
        /// Show an error message to the user.
        /// </summary>
        void ShowError(string message, string title);
    }

    /// <summary>
    /// Result of analyzing a single star.
    /// </summary>
    public class StarAnalysisResult
    {
        public ITxObject StarObject { get; set; }
        public string StarName { get; set; }
        public TxVector LocalPosition { get; set; }
        public TxVector WorldPosition { get; set; }
        public PositionZone Zone { get; set; }
        public string ZoneLabel { get; set; }
        public string ZoneName { get; set; }
        public StarEmitterVisibilityResult EmitterResult { get; set; }
        public StarLineOfSightResult LineOfSightResult { get; set; }
        public bool IsValid { get; set; }
        public double MaxAngle { get; set; }
    }

    /// <summary>
    /// Result of triangle height analysis.
    /// </summary>
    public class TriangleAnalysisResult
    {
        public double Height { get; set; }
        public double SideAB { get; set; }
        public double SideBC { get; set; }
        public double SideCA { get; set; }
        public double LongestSide { get; set; }
        public bool IsValid { get; set; }
        public List<TxVector> WorldPositions { get; set; }
    }
}
