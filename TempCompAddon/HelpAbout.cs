using System;
using System.Drawing;
using System.Windows.Forms;

namespace TempCompAddon
{
    public static class HelpAbout
    {
        public const string Version = "1.2.0";
        public const string LastUpdated = "July 2026";
        public const string Author = "ISRA Vision / CAD & Simulation";
        public const string AboutText = @"Temp Comp Validator - v1.2.0
Process Simulate Add-On for Temp Comp Path Validation

PURPOSE
-------
Validates whether a Temp Comp measurement program adequately
covers the robot pose range of the Bodypart measurement paths.

J2-3 ANGLE
----------
The J2-3 angle combines the main arm axes into a single
comparable value. Formula depends on robot manufacturer:

  - Fanuc : J2 + J3 + 90
  - Kuka  : (-1) * J3 + 180
  - ABB   : (-1) * J3 + 90

Robot type is auto-detected from the robot 3D file path
(cojt path) and can be overridden manually.

AXIS NORMALIZATION (J4 / J6)
----------------------------
J4 and J6 can rotate 360 deg - the same physical position is
reachable with different axis values. All J4/J6 values and
differences are normalized to +/-180 deg (shortest arc):

  dA = ((d + 180) mod 360) - 180

VALIDATION CRITERIA
-------------------
1. J2-3 Angle Coverage : min 2 TC points >= body max AND
                         min 2 TC points <= body min
2. J2-3 Range          : TC range >= 75 deg
3. J5 Symmetry         : balanced negative/positive J5 values
4. J4 Max Coverage     : min 2 TC points reach body max (abs)
5. J5 Max Coverage     : min 2 TC points reach body max (abs)
6. J6 Max Coverage     : min 2 TC points reach body max (abs)

NEAREST TC POINT
----------------
For each body point the nearest TC point is selected using a
weighted Euclidean distance over (dJ2-3, dJ4, dJ5, dJ6),
J2-3 dominant (weight 2.0).

Color coding (threshold = Nearest TC distance setting,
default 35 deg, ranges scale with the setting):
  - Green  : axis difference < threshold
  - Yellow : between threshold and 2x threshold
  - Red    : above 2x threshold
Max Diff column = largest single axis difference.

RAW DATA TAB
------------
J4/J6 values displayed normalized (+/-180 deg).
Body J2-3 : max = green, min = light blue
TC J2-3   : only the 2 largest / 2 smallest values colored:
  - Green/Blue : covers body max/min
  - Yellow     : close but not sufficient (within threshold)
  - Red        : far from required value

GAP ANALYSIS TAB
----------------
Shows the distribution of TC measurement point values per axis
(J2, J3, J4, J5, J6, J2-3), sorted ascending.

For each axis a user-defined max gap threshold can be set
(default 25 deg). If the gap between two consecutive TC values
exceeds the threshold, a red cell is inserted between them.

This ensures that the TC program covers the full axis range
with sufficient density to compensate any body measurement
point within the given tolerance.

Status row at the bottom:
  - Green : max gap within threshold (OK)
  - Red   : max gap exceeds threshold (NOK)
  - Gray  : no threshold set (J2-3)

Each value is shown with its source program and point name
(e.g. UP101 - ART1_1) for traceability.

MEASUREMENT POINT FILTER
------------------------
Three filter modes are available (selectable in the UI):

NO FILTER
   All locations in the selected paths are included.
   No filtering is applied.

AUTO (default)
   Points are identified by two methods:

   1. NAME PREFIX (primary)
      Body paths : points starting with ""mp"" (case-insensitive)
      TC paths   : points starting with ""art"" or ""temp"" (case-insensitive)

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
   - Body prefixes  : name prefixes for body measurement points
   - TC prefixes    : name prefixes for TC measurement points
   - OLP keywords   : fallback keywords in OLP command text

KNOWN ISSUES
------------
ABB Robot - Missing Joint Values
When ABB robots have inconsistent RobotConfigurationData (config flags
do not match the stored joint values), GetPoseAtLocation() returns no
joint values for affected locations.

Workaround: Manually teach (touch up) the affected locations in PS
before running the analysis. This updates the robot configuration and
allows joint values to be read correctly.

Root cause: PS API does not expose a programmatic way to jump the robot
to a location and re-teach it from an add-on. The JumpToLocation method
does not exist in Tecnomatix.Engineering.dll (2408.17). A Siemens support
ticket has been raised to investigate a proper API solution.


DEVELOPED BY
------------
   ISRA Vision / CAD & Simulation Team
   Last updated: July 2026";

        /// <summary>
        /// Shows the About dialog.
        /// </summary>
        public static void ShowAbout()
        {
            var dlg = new Form
            {
                Text = "About - Temp Comp Validator",
                Width = 560,
                Height = 640,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                StartPosition = FormStartPosition.CenterScreen
            };

            // Title label
            var lblTitle = new Label
            {
                Text = "Temp Comp Validator",
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

            // Separator
            var sep = new Label
            {
                Left = 16,
                Top = 60,
                Width = 510,
                Height = 2,
                BorderStyle = BorderStyle.Fixed3D
            };

            // About text
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

            // Close button
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