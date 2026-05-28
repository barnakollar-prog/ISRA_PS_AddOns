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

VALIDATION CRITERIA
-------------------

1. STAR POSITION ZONE
   Star LED center position is evaluated in the Tracker local
   coordinate system (MiddleField = Z origin):

   - Optimal : Star local Z > 0 (Mid-Far zone)
   - Warning : Star local Z between -803mm and 0mm (Near-Mid zone)
   - NOK     : Star local Z < -803mm (Near Field)

2. EMITTER VISIBILITY
   Each star has 4 emitters. The tracker has 3 cameras.
   At least 3 out of 4 emitters must be visible from ALL 3 cameras.

   Visibility is determined by the angle at the emitter (E)
   between the emitter-to-camera vector (E->K) and the
   emitter Z vector (E->F):

   - angle at E = arccos((E->K . E->Z) / (|E->K| x |E->Z|))
   - Default max angle: 40 degrees (user adjustable)

   Note: The max angle tolerance should be validated
   on the Munich demo cell.

3. LINE OF SIGHT CHECK
   For each star, a cylinder (radius 5mm) is created from each
   tracker camera to the star self origin (+10mm Z offset).
   If any cylinder collides with scene geometry, the star is
   marked as BLOCKED.

   IMPORTANT: The Tracker Design FOV entity must be hidden
   before running the analysis, otherwise false BLOCKED
   results may occur.

   Cylinder color:
   - Green     : line of sight clear
   - Red       : line of sight blocked

4. TRIANGLE CALIBRATION CRITERION
   The triangle formed by the 3 Star LED centers must have a
   minimum height of 500mm (perpendicular from apex to longest side).

   - OK   : Height >= 500mm
   - FAIL : Height < 500mm

   Triangle analysis is only performed if all 3 stars pass
   criteria 1, 2 and 3.

TRACKER
-------
   Model : 920-0005
   Cameras (local coordinates):
   - Camera_1 : X=+524mm, Y=0mm,  Z=-1776mm (right)
   - Camera_2 : X=0mm,    Y=0mm,  Z=-1776mm (center)
   - Camera_3 : X=-525mm, Y=0mm,  Z=-1776mm (left)

   FOV zones (local Z):
   - Near Field : Z < -803mm
   - Mid Field  : -803mm <= Z <= 0mm (tracker origin)
   - Far Field  : Z > 0mm up to +1200mm

STAR
----
   Model  : 515-0139
   LED center offset from self origin: (0, 0, 4mm)
   Emitters:
   - Emitter_1 : X=-13.76mm, Y=0mm,    Z=2.05mm, Ry=-10 deg
   - Emitter_2 : X=0mm,      Y=-13.76, Z=2.05mm, Rx=+10 deg
   - Emitter_3 : X=+13.76mm, Y=0mm,    Z=2.05mm, Ry=+10 deg
   - Emitter_4 : X=0mm,      Y=+13.76, Z=2.05mm, Rx=-10 deg

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