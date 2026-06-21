using System;
using System.Drawing;

namespace ISRA.Core.Utilities
{
    /// <summary>
    /// Standard color palette for ISRA addons.
    /// </summary>
    public static class ColorPalette
    {
        // ── WinForms Colors ───────────────────────────────────────

        /// <summary>Highlight color (orange)</summary>
        public static readonly Color Highlight = Color.FromArgb(255, 200, 0);

        /// <summary>Success/OK color (green)</summary>
        public static readonly Color OK = Color.FromArgb(0, 200, 0);

        /// <summary>Success/OK color (light green for backgrounds)</summary>
        public static readonly Color OKLight = Color.LightGreen;

        /// <summary>Warning color (orange)</summary>
        public static readonly Color Warning = Color.FromArgb(255, 165, 0);

        /// <summary>Warning color (gold for backgrounds)</summary>
        public static readonly Color WarningLight = Color.Gold;

        /// <summary>Error/NOK color (red)</summary>
        public static readonly Color NOK = Color.FromArgb(200, 0, 0);

        /// <summary>Error/NOK color (light coral for backgrounds)</summary>
        public static readonly Color NOKLight = Color.LightCoral;

        /// <summary>Info color (light blue)</summary>
        public static readonly Color Info = Color.LightBlue;

        /// <summary>Neutral/disabled color (gray)</summary>
        public static readonly Color Neutral = Color.FromArgb(100, 100, 100);

        /// <summary>Primary action button color</summary>
        public static readonly Color ButtonPrimary = Color.FromArgb(0, 100, 180);

        /// <summary>Success action button color</summary>
        public static readonly Color ButtonSuccess = Color.FromArgb(0, 160, 0);

        /// <summary>Danger action button color</summary>
        public static readonly Color ButtonDanger = Color.FromArgb(180, 0, 0);

        /// <summary>Warning action button color</summary>
        public static readonly Color ButtonWarning = Color.FromArgb(180, 120, 0);

        // ── Color Helper Methods ──────────────────────────────────

        /// <summary>
        /// Returns appropriate color based on validation status.
        /// </summary>
        public static Color GetStatusColor(bool isValid)
        {
            return isValid ? OKLight : NOKLight;
        }

        /// <summary>
        /// Returns appropriate color based on difference threshold.
        /// </summary>
        /// <param name="value">Absolute value to check</param>
        /// <param name="threshold">Green threshold</param>
        /// <param name="warningThreshold">Warning threshold (typically 2x threshold)</param>
        public static Color GetDifferenceColor(double value, double threshold, double warningThreshold = 0)
        {
            if (warningThreshold == 0) warningThreshold = threshold * 2;

            value = Math.Abs(value);
            if (value < threshold) return OKLight;
            if (value < warningThreshold) return WarningLight;
            return NOKLight;
        }
    }
}
