using System;
using System.Drawing;
using System.Windows.Forms;

namespace TempCompAddon
{
    public static class HelpAbout
    {
        // ── Version info ──────────────────────────────────────────
        public const string Version = "1.0.0";
        public const string LastUpdated = "June 2026";
        public const string Author = "ISRA Vision / CAD & Simulation";

        // ── About text ────────────────────────────────────────────
        public const string AboutText = @"Temp Comp Validator - v1.0.0
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
1. J2-J3 Angle Max : min 2 TC points >= body max
2. J2-J3 Angle Min : min 2 TC points <= body min
3. J2-3 Range      : TC range >= 75 deg
4. J5 Symmetry     : balanced negative/positive J5 values
5. J4 / J5 / J6 Max: TC max (abs) >= body max (abs)

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
   - OLP keywords   : fallback keywords in OLP command text"";
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