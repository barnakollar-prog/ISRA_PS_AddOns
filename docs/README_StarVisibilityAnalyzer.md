# Star Visibility Analyzer

Process Simulate Add-On for validating AccuSite star placement
relative to the Perceptron tracker field of view.

## Purpose
Automatically validates whether measurement stars are correctly
positioned and visible from the tracker cameras, replacing manual
checks previously done in MetrIQ software.

## Usage
1. Select Tracker (920-0005)
2. Select Star 1, Star 2, Star 3 (515-0139)
3. Set max emitter angle (default 40 deg)
4. Click Analyze
5. Optionally: Export to Excel

## Validation Criteria
| # | Criterion | Requirement |
|---|---|---|
| 1 | Star Position Zone | Local Z > 0 optimal, -803..0 warning, < -803 NOK |
| 2 | Emitter Visibility | Min 3/4 emitters visible from ALL 3 cameras |
| 3 | Line of Sight | No geometry blocking camera-to-star cylinder |
| 4 | Triangle Height | Min 500mm (apex to longest side) |

Triangle analysis only performed if all 3 stars pass criteria 1-3.

## Hardware Definitions
### Tracker: 920-0005
Camera positions (local coordinates):
| Camera | X | Y | Z |
|---|---|---|---|
| Camera_1 (right) | +524mm | 0mm | -1776mm |
| Camera_2 (center) | 0mm | 0mm | -1776mm |
| Camera_3 (left) | -525mm | 0mm | -1776mm |

FOV zones (local Z):
- Near Field : Z < -803mm → NOK
- Mid Field  : -803mm ≤ Z ≤ 0mm → Warning
- Far Field  : Z > 0mm → Optimal

### Star: 515-0139
LED center offset: (0, 0, 4mm) from self origin

Emitters:
| Emitter | X | Y | Z | Orientation |
|---|---|---|---|---|
| Emitter_1 | -13.76mm | 0mm | 2.05mm | Ry=-10 deg |
| Emitter_2 | 0mm | -13.76mm | 2.05mm | Rx=+10 deg |
| Emitter_3 | +13.76mm | 0mm | 2.05mm | Ry=+10 deg |
| Emitter_4 | 0mm | +13.76mm | 2.05mm | Rx=-10 deg |

## Emitter Visibility
Angle at emitter E between E→Camera and emitter Z vector:

`angle = arccos((E→K · E→Z) / (|E→K| × |E→Z|))`

Default max angle: 40 deg (user adjustable).

## Line of Sight Check
Temporary cylinder (radius 5mm) created from each camera
to star self origin (+10mm Z offset on both ends).

**Important:** Hide the Tracker Design FOV entity before
running analysis to avoid false BLOCKED results.

Cylinder color in PS:
- 🟢 Green: line of sight clear
- 🔴 Red: line of sight blocked

## Results
- Color-coded rows: green/yellow/red per star
- Collapsible detail rows (double-click to expand/collapse)
- NOK results auto-expanded

## Excel Export
Exports per-star results including position, zone,
visibility status and triangle height.

## Files
- `LedVisibilityAddon/Startup.cs` — UI and main logic
- `LedVisibilityAddon/ExcelExporter.cs` — Excel export
- `LedVisibilityAddon/HelpAbout.cs` — Help/About dialog
- `ISRA.Components/AccuSite/Stars/star_515_0139.cs` — star geometry
- `ISRA.Components/AccuSite/Trackers/tracker_920_0005.cs` — tracker geometry
- `ISRA.Calculations/AccuSite/GeometryCalculations.cs` — calculation logic
- `ISRA.Calculations/AccuSite/CollisionCheck.cs` — LOS check