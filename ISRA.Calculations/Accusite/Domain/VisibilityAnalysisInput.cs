using Tecnomatix.Engineering;
using ISRA.Components.AccuSite.Stars;
using ISRA.Components.AccuSite.Trackers;

namespace ISRA.Calculations.AccuSite.Domain
{
    /// <summary>
    /// Input data for AccuSite visibility analysis.
    /// </summary>
    public class VisibilityAnalysisInput
    {
        /// <summary>Tracker locatable object (contains position and orientation)</summary>
        public ITxLocatableObject Tracker { get; set; }

        /// <summary>Tracker world transformation</summary>
        public TxTransformation TrackerWorld { get; set; }

        /// <summary>Tracker configuration (920-0005)</summary>
        public ITracker TrackerConfiguration { get; set; }

        /// <summary>Star 1 locatable object</summary>
        public ITxLocatableObject Star1 { get; set; }

        /// <summary>Star 2 locatable object</summary>
        public ITxLocatableObject Star2 { get; set; }

        /// <summary>Star 3 locatable object</summary>
        public ITxLocatableObject Star3 { get; set; }

        /// <summary>Star configuration (515-0139)</summary>
        public IStar StarConfiguration { get; set; }

        /// <summary>Maximum emitter angle threshold (degrees, default 40)</summary>
        public double MaxEmitterAngle { get; set; }

        public VisibilityAnalysisInput()
        {
            MaxEmitterAngle = 40.0;
        }
    }
}
