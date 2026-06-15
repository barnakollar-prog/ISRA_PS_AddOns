using System.Collections;
using Tecnomatix.Engineering;

namespace ISRA.Calculations.TempComp
{
    /// <summary>
    /// Two-level filter for identifying measurement points in robot paths.
    /// Level 1: Name prefix matching (fast, reliable when naming convention exists)
    /// Level 2: OLP keyword matching via PS Attributes (fallback for robot backups)
    /// </summary>
    public static class MeasurementPointFilter
    {
        // ── Level 1: Name prefix filters ─────────────────────────
        /// <summary>Bodypart measurement path prefixes (case-insensitive)</summary>
        public static readonly string[] BodyPrefixes = { "mp" };

        /// <summary>Temp Comp measurement path prefixes (case-insensitive)</summary>
        public static readonly string[] TcPrefixes = { "art", "temp" };

        // ── Level 2: OLP keyword filters ─────────────────────────
        /// <summary>
        /// OLP command keywords indicating a measurement point.
        /// Extend this list as new robot brands/programs are encountered.
        /// Checked against both attribute names and values (case-insensitive contains).
        /// </summary>
        public static readonly string[] OlpKeywords = new string[]
        {
            "meas",       // generic measurement
            "cmeas",      // conditional measurement
            "inline",     // inline measurement
            "VW_USER",    // VW specific
            "TECH10",     // Perceptron
            "PRC_IMT",    // IMT measurement process
        };

        /// <summary>
        /// Returns true if the location is a measurement point,
        /// based on name prefix (Level 1) or OLP keywords in Attributes (Level 2).
        /// If namePrefixes is null or empty, only OLP keyword check is performed.
        /// </summary>
        public static bool IsMeasurementPoint(
            ITxRoboticLocationOperation loc,
            string[] namePrefixes)
        {
            if (loc == null) return false;

            // ── Level 1: Name prefix ──────────────────────────────
            if (namePrefixes != null && namePrefixes.Length > 0)
            {
                string nameLower = loc.Name.ToLower();
                foreach (var prefix in namePrefixes)
                {
                    if (!string.IsNullOrEmpty(prefix) &&
                        nameLower.StartsWith(prefix.ToLower()))
                        return true;
                }
            }

            // ── Level 2: OLP keywords in Attributes ──────────────

            try
            {
                var roboLoc = loc as ITxRoboticOperation;
                if (roboLoc != null && roboLoc.Commands != null)
                {
                    foreach (ITxObject cmdObj in roboLoc.Commands)
                    {
                        var cmd = cmdObj as TxRoboticCommand;
                        if (cmd == null) continue;

                        string cmdText = (cmd.Text ?? "").ToLower();
                        string cmdName = (cmd.Name ?? "").ToLower();

                        foreach (var kw in OlpKeywords)
                        {
                            string kwLower = kw.ToLower();
                            if (cmdText.Contains(kwLower) || cmdName.Contains(kwLower))
                                return true;
                        }
                    }
                }
            }
            catch { }

            return false;
        }
    }
}