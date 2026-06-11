using System.Drawing;
using System.Windows.Forms;

namespace TempCompAddon
{
    public static class HelpAbout
    {
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

DEVELOPED BY
------------
   ISRA Vision / CAD & Simulation Team
   Last updated: June 2026";

        public static void ShowAbout()
        {
            var form = new Form
            {
                Text = "Temp Comp Validator - Help / About",
                Width = 640,
                Height = 700,
                StartPosition = FormStartPosition.CenterScreen,
                MinimizeBox = false,
                MaximizeBox = false
            };

            var txt = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 9),
                Text = AboutText.Replace("\n", "\r\n"),
                BackColor = Color.White
            };

            form.Controls.Add(txt);
            form.Show();
        }
    }
}