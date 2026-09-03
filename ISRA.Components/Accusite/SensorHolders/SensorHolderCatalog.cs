using System.Collections.Generic;

namespace ISRA.Components.AccuSite.SensorHolders
{
    /// <summary>
    /// Registry of all known sensor holder types.
    /// Key: type ID used in PS component name and for factory lookup.
    /// Value: display name shown in UI dropdowns.
    /// Add new holder types here as they are implemented.
    /// </summary>
    public static class SensorHolderCatalog
    {
        public static IReadOnlyDictionary<string, string> All { get; } =
            new Dictionary<string, string>
            {
                { "perc_01-03944-10", "Perceptron 01-03944-10 (40 LEDs, 8 groups)" },
                // { "perc_01-03944-20", "Perceptron 01-03944-20" },  // future
            };

        /// <summary>Returns true if the given type ID is in the catalog.</summary>
        public static bool Contains(string typeId) =>
            !string.IsNullOrEmpty(typeId) && All.ContainsKey(typeId);
    }
}