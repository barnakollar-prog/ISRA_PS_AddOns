using System.Collections.Generic;
using ISRA.Calculations.AccuSite;

namespace LedVisibilityAddon.Presentation
{
    /// <summary>
    /// Data transfer object for exporting visibility analysis results.
    /// </summary>
    public class ExportData
    {
        public List<ExportRow> Rows { get; set; }
        public string TrackerName { get; set; }
        public GeometryCalculations.TriangleResult Triangle { get; set; }

        public ExportData()
        {
            Rows = new List<ExportRow>();
        }
    }

    /// <summary>
    /// Represents a single star's data for export.
    /// </summary>
    public class ExportRow
    {
        public string StarName { get; set; }
        public double LocalX { get; set; }
        public double LocalY { get; set; }
        public double LocalZ { get; set; }
        public string Zone { get; set; }
        public bool InsideFov { get; set; }
        public string TrackerName { get; set; }
    }
}
