namespace ISRA.Calculations.TempComp
{
    /// <summary>
    /// Defines the filtering mode for measurement point detection.
    /// </summary>
    public enum FilterMode
    {
        /// <summary>No filtering — all locations are included.</summary>
        NoFilter,

        /// <summary>Automatic filtering using built-in prefixes and OLP keywords.</summary>
        Auto,

        /// <summary>Custom filtering using user-defined prefixes and OLP keywords.</summary>
        Custom
    }
}