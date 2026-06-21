namespace ISRA.Calculations.AccuSite.Domain.Results
{
    /// <summary>
    /// Result of triangle geometry calculation (formed by 3 stars).
    /// </summary>
    public class TriangleResult
    {
        /// <summary>Distance between stars A and B (mm)</summary>
        public double SideAB { get; set; }

        /// <summary>Distance between stars B and C (mm)</summary>
        public double SideBC { get; set; }

        /// <summary>Distance between stars C and A (mm)</summary>
        public double SideCA { get; set; }

        /// <summary>Length of the longest side (mm)</summary>
        public double LongestSide { get; set; }

        /// <summary>Name of the longest side (e.g., "AB", "BC", "CA")</summary>
        public string LongestSideName { get; set; }

        /// <summary>Triangle area (mm²)</summary>
        public double Area { get; set; }

        /// <summary>Height from longest side (mm)</summary>
        public double Height { get; set; }

        /// <summary>True if triangle meets geometry requirements</summary>
        public bool IsValid { get; set; }
    }
}
