using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ISRA.Core.Utilities
{
    public class MpFeatureEntry
    {
        public int Level { get; set; }
        public string Algorithm { get; set; }
    }

    /// <summary>
    /// Loads MP feature JSON and resolves Level ID → AimingGuidelinesProfile.
    /// If a level has multiple algorithms, the strictest profile wins.
    /// JSON format: [{"level":1,"algorithm":"Scanned Edge"}, ...]
    /// </summary>
    public class MpFeatureLookup
    {
        private readonly Dictionary<int, AimingGuidelinesProfile> _levelProfiles
            = new Dictionary<int, AimingGuidelinesProfile>();

        public static MpFeatureLookup LoadFromJson(string jsonFilePath)
        {
            if (!File.Exists(jsonFilePath))
                throw new FileNotFoundException(
                    "MP feature JSON file not found.", jsonFilePath);

            string json = File.ReadAllText(jsonFilePath, Encoding.UTF8);
            var entries = ParseJson(json);

            var lookup = new MpFeatureLookup();

            foreach (var entry in entries)
            {
                if (string.IsNullOrEmpty(entry.Algorithm)) continue;

                var profile = AimingGuidelinesCatalog.GetByAlgorithm(entry.Algorithm);
                if (profile == null) continue;

                if (!lookup._levelProfiles.ContainsKey(entry.Level))
                {
                    lookup._levelProfiles[entry.Level] = profile;
                }
                else
                {
                    lookup._levelProfiles[entry.Level] =
                        lookup._levelProfiles[entry.Level].MergeStrictest(profile);
                }
            }

            return lookup;
        }

        /// <summary>
        /// Resolves a PS location name to an AimingGuidelinesProfile.
        /// PS location name format: "mp_<Level>_<MPname>"
        /// Returns null if level not found or name format invalid.
        /// </summary>
        public AimingGuidelinesProfile GetProfileForLocation(string locationName)
        {
            int level = ExtractLevelFromLocationName(locationName);
            if (level < 0) return null;

            AimingGuidelinesProfile profile;
            return _levelProfiles.TryGetValue(level, out profile) ? profile : null;
        }

        public static int ExtractLevelFromLocationName(string locationName)
        {
            if (string.IsNullOrEmpty(locationName)) return -1;

            var parts = locationName.Split('_');
            if (parts.Length < 2) return -1;
            if (!parts[0].Equals("mp", StringComparison.OrdinalIgnoreCase)) return -1;

            int level;
            return int.TryParse(parts[1], out level) ? level : -1;
        }

        public int LevelCount => _levelProfiles.Count;
        public IEnumerable<int> AllLevels => _levelProfiles.Keys;

        // ── Minimal JSON parser ───────────────────────────────────
        // Handles: [{"level":1,"algorithm":"Scanned Edge"}, ...]
        // No external dependencies.

        private static List<MpFeatureEntry> ParseJson(string json)
        {
            var result = new List<MpFeatureEntry>();
            if (string.IsNullOrWhiteSpace(json)) return result;

            // Strip outer array brackets
            json = json.Trim();
            if (json.StartsWith("[")) json = json.Substring(1);
            if (json.EndsWith("]")) json = json.Substring(0, json.Length - 1);

            // Split by object boundaries
            var objects = SplitObjects(json);

            foreach (var obj in objects)
            {
                var entry = new MpFeatureEntry();
                var trimmed = obj.Trim().Trim('{', '}');

                foreach (var pair in trimmed.Split(','))
                {
                    var kv = pair.Split(new char[] { ':' }, 2);
                    if (kv.Length != 2) continue;

                    string key = kv[0].Trim().Trim('"');
                    string value = kv[1].Trim().Trim('"');

                    if (key.Equals("level", StringComparison.OrdinalIgnoreCase))
                    {
                        int level;
                        if (int.TryParse(value, out level))
                            entry.Level = level;
                    }
                    else if (key.Equals("algorithm", StringComparison.OrdinalIgnoreCase))
                    {
                        entry.Algorithm = value;
                    }
                }

                if (entry.Level > 0 && !string.IsNullOrEmpty(entry.Algorithm))
                    result.Add(entry);
            }

            return result;
        }

        private static List<string> SplitObjects(string json)
        {
            var objects = new List<string>();
            int depth = 0;
            int start = 0;

            for (int i = 0; i < json.Length; i++)
            {
                if (json[i] == '{') { if (depth == 0) start = i; depth++; }
                else if (json[i] == '}')
                {
                    depth--;
                    if (depth == 0)
                        objects.Add(json.Substring(start, i - start + 1));
                }
            }

            return objects;
        }
    }
}