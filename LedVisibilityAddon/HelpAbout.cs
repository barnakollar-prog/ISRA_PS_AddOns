using System;
using System.Windows.Forms;
using System.Drawing;
using ISRA.Components.AccuSite.Trackers;
using ISRA.Components.AccuSite.Stars;
using ISRA.Calculations.AccuSite;

namespace LedVisibilityAddon
{
    /// <summary>
    /// Help and About dialog for the LED Visibility Analyzer.
    /// Update this class to reflect changes in validation methodology.
    /// </summary>
    public static class HelpAbout
    {
        // ── Version info ──────────────────────────────────────────
        public const string Version = "1.0.0";
        public const string LastUpdated = "May 2026";
        public const string Author = "ISRA Vision / CAD & Simulation";

        // ── About text ────────────────────────────────────────────
        public const string AboutText = @"Star Visibility Analyzer - v1.0.0
Process Simulate Add-On for AccuSite Star Validation

VALIDATION CRITERIA
-------------------

1. STAR ORIENTATION
   The Star self-origin Z vector must point opposite to the
   Tracker self-origin Z vector, within the following tolerances:

   - XZ plane (rotation around Y axis): max +/- 25 degrees
   - YZ plane (rotation around X axis): max +/- 40 degrees

   The Star Z vector is expressed in the Tracker local coordinate
   system and evaluated separately per plane:
   - XZ deviation = atan2(|localX|, |localZ|)
   - YZ deviation = atan2(|localY|, |localZ|)

   The Star Z must point opposite to Tracker Z (localZ < 0).

   Note: These tolerances are preliminary and will be validated
   on the Munich demo cell.

2. STAR POSITION ZONE
   Star position is evaluated in the Tracker local coordinate system:

   - Optimal : Star local Z > 0 (Mid-Far zone, beyond tracker origin)
   - Warning : Star local Z between -803mm and 0mm (Near-Mid zone)
   - NOK     : Star local Z < -803mm (Near Field)

3. TRIANGLE CALIBRATION CRITERION
   The triangle formed by the 3 Star LED centers must have a
   minimum height of 500mm (perpendicular from apex to longest side).

   - OK   : Height >= 500mm
   - FAIL : Height < 500mm

   Triangle analysis is only performed if all 3 stars pass
   criteria 1 and 2.

TRACKER
-------
   Model : 920-0005
   FOV zones (local Z):
   - Near Field  : Z < -803mm
   - Mid Field   : -803mm <= Z <= 0mm
   - Far Field   : Z > 0mm up to +1200mm

STAR
----
   Model  : 515-0139
   LED center offset from self origin: (0, 0, 4mm)

DEVELOPED BY
------------
   ISRA Vision / CAD & Simulation Team
   Last updated: May 2026";

        /// <summary>
        /// Shows the About dialog.
        /// </summary>
        public static void ShowAbout()
        {
            var dlg = new Form
            {
                Text = "About - Star Visibility Analyzer",
                Width = 520,
                Height = 580,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                StartPosition = FormStartPosition.CenterScreen
            };

            // Title label
            var lblTitle = new Label
            {
                Text = "Star Visibility Analyzer",
                Left = 16,
                Top = 12,
                Width = 470,
                Height = 24,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(180, 0, 0)
            };

            var lblVersion = new Label
            {
                Text = string.Format("Version {0}  |  {1}  |  {2}",
                    Version, LastUpdated, Author),
                Left = 16,
                Top = 38,
                Width = 470,
                Height = 18,
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.Gray
            };

            // Separator
            var sep = new Label
            {
                Left = 16,
                Top = 60,
                Width = 470,
                Height = 2,
                BorderStyle = BorderStyle.Fixed3D
            };

            // About text
            var txtAbout = new TextBox
            {
                Text = AboutText,
                Left = 16,
                Top = 70,
                Width = 470,
                Height = 420,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Consolas", 8),
                BackColor = Color.White,
                BorderStyle = BorderStyle.None
            };

            // Close button
            var btnClose = new Button
            {
                Text = "Close",
                Left = 390,
                Top = 500,
                Width = 96,
                Height = 28,
                DialogResult = DialogResult.OK
            };
            btnClose.Click += (s, e) => dlg.Close();

            dlg.Controls.Add(lblTitle);
            dlg.Controls.Add(lblVersion);
            dlg.Controls.Add(sep);
            dlg.Controls.Add(txtAbout);
            dlg.Controls.Add(btnClose);
            dlg.ShowDialog();
        }
    }
}