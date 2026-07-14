using System.Collections.Generic;

namespace ISRA.Calculations.TempComp.Domain.Results
{
    public class AxisGapResult
    {
        public string AxisName { get; set; }

        /// <summary>Sorted pose data (value, pose name, path name) ascending by value</summary>
        public List<(double Value, string PoseName, string PathName)> SortedValues { get; set; }

        /// <summary>Indices where a gap occurs (between index i and i+1)</summary>
        public List<int> GapIndices { get; set; }

        public double MaxGap { get; set; }
        public double Threshold { get; set; }

        public bool IsValid => Threshold <= 0 || MaxGap <= Threshold;

        public AxisGapResult()
        {
            SortedValues = new List<(double, string, string)>();
            GapIndices = new List<int>();
        }
    }

    public class GapAnalysisResult
    {
        public AxisGapResult J2 { get; set; }
        public AxisGapResult J3 { get; set; }
        public AxisGapResult J4 { get; set; }
        public AxisGapResult J5 { get; set; }
        public AxisGapResult J6 { get; set; }
        public AxisGapResult J23 { get; set; }

        public List<AxisGapResult> AllAxes => new List<AxisGapResult>
            { J2, J3, J4, J5, J6, J23 };

        public GapAnalysisResult()
        {
            J2 = new AxisGapResult { AxisName = "J2", Threshold = 0 };
            J3 = new AxisGapResult { AxisName = "J3", Threshold = 0 };
            J4 = new AxisGapResult { AxisName = "J4", Threshold = 25 };
            J5 = new AxisGapResult { AxisName = "J5", Threshold = 25 };
            J6 = new AxisGapResult { AxisName = "J6", Threshold = 25 };
            J23 = new AxisGapResult { AxisName = "J2-3", Threshold = 0 };
        }
    }
}