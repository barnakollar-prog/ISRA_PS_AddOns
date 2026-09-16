using System;
using System.Drawing;
using System.Windows.Forms;

namespace ConstellationAddon
{
    /// <summary>
    /// Help and About dialog for the Constellation Validator.
    /// Update this class to reflect changes in validation methodology.
    /// </summary>
    public static class HelpAbout
    {
        // ── Version info ──────────────────────────────────────────
        public const string Version = "1.1.0";
        public const string LastUpdated = "September 2026";
        public const string Author = "ISRA Vision / CAD & Simulation";

        // ── About text ────────────────────────────────────────────
        public const string AboutText = @"Accusite Constellation Validator - v1.0.0
Process Simulate Add-On for AccuSite Constellation Validation

WHAT IT DOES
------------
Validates whether the Accusite sensor holder LED constellation is
visible from the external tracker at each measurement point along
a robotic path. Checks angle, field-of-view, and line-of-sight
per emitter and tracker camera.

HOW TO USE
----------
1. Select Robot        — pick the robot from the PS scene
2. Select Sensor Holder Type — choose from dropdown (catalog-based)
3. Select Sensor Holder Component — pick the holder component in PS
                         (provides exact self-origin position)
4. Select Tracker(s)   — pick up to 4 trackers (Tracker 1 required)
                         Analysis tries each in order; first OK wins
5. Select Path         — pick a robotic path (TxWeldOperation)
6. Set Collision Pair  — select from PS Collision Viewer (optional)
7. Click Analyze

RESULTS TAB
-----------
- Location    : measurement point name
- Status      : OK / NOK / COLLISION / SKIPPED
- Tracker     : which tracker satisfied criteria (T1-T4)
- Plane       : which tracking plane satisfied (A/B/C/D)
- Visible LEDs: emitters visible from ALL 3 cameras
- FOV         : constellation within tracker field-of-view?
- Sight       : line-of-sight clear? (OK / NOK / N/A)

Click any result row to jump to that point and show visualization.

ANGLE DETAILS TAB
-----------------
Expandable tree per point. Shows per-emitter angle values for
each camera (Cam1/Cam2/Cam3), FOV status, and pass/fail.

VISUALIZATION FILTERS
---------------------
After clicking a result row:
- Show OK lines (green)    : emitters within angle limit
- Show NOK lines (red)     : emitters outside angle limit
- Show FOV outside (gray)  : emitters outside tracker FOV

TRACKING PLANES
---------------
At least ONE plane must be satisfied for OK result.
All emitters must be visible from ALL 3 tracker cameras.

  Plane A (Primary 1):
    NAUO3 >= 1  AND  NAUO4 >= 1  AND  (NAUO1 >= 1 OR NAUO2 >= 1)

  Plane B (Primary 2):
    NAUO5 >= 1  AND  NAUO7 >= 1  AND  (NAUO6 >= 1 OR NAUO8 >= 1)

  Plane C (Secondary 1):
    At least 3 of {NAUO2, NAUO4, NAUO7, NAUO8} each >= 1

  Plane D (Secondary 2):
    At least 3 of {NAUO1, NAUO3, NAUO5, NAUO6} each >= 1

ANGLE LIMIT
-----------
  Max emitter-to-camera angle: 55 degrees
  Source: Perceptron / Planner Builder specification
          (Pavel Machacek / Vitek)

TRACKER
-------
  Model: 920-0005
  Cameras (local coordinates):
  - Camera_1 : X=+524mm, Y=0mm, Z=-1776mm (right)
  - Camera_2 : X=0mm,    Y=0mm, Z=-1776mm (center)
  - Camera_3 : X=-525mm, Y=0mm, Z=-1776mm (left)

SENSOR HOLDER
-------------
  Currently supported: perc_01-03944-10
  8 NAUO groups x 5 emitters = 40 LEDs total
  All coordinates relative to holder self-origin (= robot flange)

MEASUREMENT POINT FILTER
------------------------
Three filter modes are available (selectable in the UI) to decide
which locations on the selected path(s) are analyzed:

NO FILTER
   All locations in the selected path(s) are included.
   No filtering is applied.

AUTO (default)
   Points are identified by two methods:

   1. NAME PREFIX (primary)
      Points starting with ""mp"" (case-insensitive)

   2. OLP COMMAND TEXT (fallback, robot backup programs)
      Points containing any of the following keywords in their
      OLP command text are treated as measurement points:
      - meas, cmeas   : generic / conditional measurement
      - inline        : inline measurement
      - VW_USER       : VW specific
      - TECH10        : Perceptron
      - PRC_IMT       : IMT measurement process

CUSTOM
   User-defined prefixes and OLP keywords.
   Enter comma-separated values in the filter fields:
   - Prefixes     : name prefixes for measurement points
   - OLP keywords : fallback keywords in OLP command text

KNOWN LIMITATIONS
-----------------
- Collision check requires pre-defined pair in PS Collision Viewer
- Only one path per analysis run
- Auto sensor reorientation for NOK points: planned (#93906)
- Excel result export: planned (#93907)
- Additional sensor holder types: planned

DEVELOPED BY
------------
  ISRA Vision / CAD & Simulation Team
  Last updated: July 2026
  ADO: dev.azure.com/ac-it-mvs/cad-simulation — Issue #93907";

        /// <summary>
        /// Shows the About dialog.
        /// </summary>
        public static void ShowAbout()
        {
            var dlg = new Form
            {
                Text = "About - Constellation Validator",
                Width = 560,
                Height = 640,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                StartPosition = FormStartPosition.CenterScreen
            };

            var lblTitle = new Label
            {
                Text = "Accusite Constellation Validator",
                Left = 16,
                Top = 12,
                Width = 510,
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
                Width = 510,
                Height = 18,
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.Gray
            };

            var sep = new Label
            {
                Left = 16,
                Top = 60,
                Width = 510,
                Height = 2,
                BorderStyle = BorderStyle.Fixed3D
            };

            var txtAbout = new TextBox
            {
                Text = AboutText,
                Left = 16,
                Top = 70,
                Width = 510,
                Height = 470,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Consolas", 8),
                BackColor = Color.White,
                BorderStyle = BorderStyle.None
            };

            var btnClose = new Button
            {
                Text = "Close",
                Left = 430,
                Top = 552,
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