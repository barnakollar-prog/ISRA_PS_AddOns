namespace ISRA.Calculations.TempComp.Domain.Results
{
    /// <summary>
    /// Summary statistics for a collection of robot poses.
    /// </summary>
    public class PoseStatistics
    {
        public double J1_Min { get; set; }
        public double J1_Max { get; set; }
        public double J2_Min { get; set; }
        public double J2_Max { get; set; }
        public double J3_Min { get; set; }
        public double J3_Max { get; set; }
        public double J4_Min { get; set; }
        public double J4_Max { get; set; }
        public double J5_Min { get; set; }
        public double J5_Max { get; set; }
        public double J6_Min { get; set; }
        public double J6_Max { get; set; }
        public int J5_NegCount { get; set; }
        public int J5_PosCount { get; set; }

        public double J1_Range => J1_Max - J1_Min;
        public double J2_Range => J2_Max - J2_Min;
        public double J3_Range => J3_Max - J3_Min;
        public double J4_Range => J4_Max - J4_Min;
        public double J5_Range => J5_Max - J5_Min;
        public double J6_Range => J6_Max - J6_Min;
    }
}
