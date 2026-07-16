# Temp Comp Validator

Process Simulate Add-On for validating Temp Comp measurement path coverage.

## Purpose
Validates whether a Temp Comp measurement program adequately covers
the robot pose range of the Bodypart measurement paths.

## Usage
1. Select robot (auto-detects type from 3D file path)
2. Pick Bodypart measurement paths (WeldOperation, Compound Operation, or Generic Robotic Operation)
3. Pick Temp Comp paths
4. Set filter mode (No Filter / Auto / Custom)
5. Set gap thresholds per axis (default 25°)
6. Click Analyze
7. Optionally: Export to Excel

## Validation Criteria
| # | Criterion | Requirement |
|---|---|---|
| 1 | J2-3 Angle Coverage | Min 2 TC points >= body max AND min 2 TC points <= body min |
| 2 | J2-3 Range | TC range >= 75° |
| 3 | J5 Symmetry | Balanced negative/positive J5 values |
| 4 | J4 Max Coverage | Min 2 TC points reach body max J4 (abs) |
| 5 | J5 Max Coverage | Min 2 TC points reach body max J5 (abs) |
| 6 | J6 Max Coverage | Min 2 TC points reach body max J6 (abs) |

## J2-3 Angle Formula
| Robot | Formula |
|---|---|
| Fanuc | J2 + J3 + 90 |
| Kuka | (-1) * J3 + 180 |
| ABB | (-1) * J3 + 90 |

Robot type is auto-detected from the robot 3D file path (cojt path)
and can be overridden manually via radio buttons.

## J4/J6 Normalization
J4 and J6 are normalized to ±180° (shortest arc):
`dA = ((d + 180) mod 360) - 180`

## Path Selection
Supports multiple operation types:
- **WeldOperation** — added directly
- **Compound Operation** — child WeldOperations expanded automatically
- **Generic Robotic Operation** — child WeldOperations expanded automatically

Multi-select supported (Shift+click) in PS 2408.

## Measurement Point Filter
Three filter modes are available:

| Mode | Description |
|---|---|
| No Filter | All locations in the selected paths are included |
| Auto (default) | Name prefix matching + OLP keyword fallback |
| Custom | User-defined prefixes and OLP keywords |

**Auto mode defaults:**
- Body paths: points starting with `mp`
- TC paths: points starting with `art` or `temp`
- OLP keywords: `meas`, `cmeas`, `inline`, `VW_USER`, `TECH10`, `PRC_IMT`

In Custom mode, all three lists are editable (comma-separated) in the UI.

## Nearest TC Point
For each body point the nearest TC point is selected using weighted
Euclidean distance over (dJ2-3, dJ4, dJ5, dJ6), J2-3 dominant:

Weights: W_J23=2.0, W_J4=W_J5=W_J6=1.0

Color coding (default threshold 35°):
- Green: difference < threshold
- Yellow: between threshold and 2x threshold
- Red: above 2x threshold

Max Diff column = largest single axis difference.

## Gap Analysis
TC measurement point values sorted ascending per axis (J2, J3, J4, J5, J6, J2-3).
User-configurable gap threshold per axis (default 25°).

- Red cells indicate gaps between consecutive TC values exceeding the threshold
- Each value shows source program and point name (e.g. UP101 - ART1_1)
- Status row at the bottom: OK (green) / NOK (red) per axis
- J2-3 has no threshold (display only)

This analysis complements the Nearest TC method by verifying that the TC
program covers the full axis range with sufficient density.

## Result Tabs

**Validation Tab** — All 6 criteria with Bodypart and Temp Comp values
side by side, color-coded green (OK) / red (NOK).

**Nearest TC Point Tab** — Body-to-TC point mapping with axis differences
and path names (Body Path / TC Path columns).

**Raw Data Tab** — All body and TC poses with joint values and path names.
J4/J6 displayed normalized (±180°). Body J2-3: max = green, min = light blue.
TC J2-3: top 2 largest / 2 smallest highlighted by coverage status.

**Gap Analysis Tab** — Sorted TC values per axis with gap highlighting,
source point/path names, and configurable thresholds.

## Excel Export
Exports all results to `.xlsx` with 4 sheets:
- **Validation Results** — all criteria with Bodypart / Temp Comp values and status
- **Nearest TC** — body-to-TC mapping with color-coded differences and path names
- **Raw Data** — all poses with joint values and path names
- **Gap Analysis** — sorted TC and Body values per axis with gap highlighting and line charts (TC vs Body per axis)

## Architecture
Follows MVP pattern:

| File | Role |
|---|---|
| `TempCompAddon/Startup.cs` | UI (TempCompForm) — ITempCompView implementation |
| `TempCompAddon/Presentation/TempCompPresenter.cs` | Presenter + ITempCompView interface |
| `TempCompAddon/Presentation/TempCompReportFormatter.cs` | ListView + DataGridView formatting |
| `TempCompAddon/Services/TempCompExcelExporter.cs` | Excel export (4 sheets) |
| `TempCompAddon/HelpAbout.cs` | Help/About dialog |
| `ISRA.Calculations/TempComp/Services/PoseReader.cs` | Reads robot poses from PS |
| `ISRA.Calculations/TempComp/Services/TempCompAnalyzer.cs` | Orchestrates validation |
| `ISRA.Calculations/TempComp/Services/GapAnalysisService.cs` | Gap analysis per axis |
| `ISRA.Calculations/TempComp/MeasurementPointFilter.cs` | Point filter logic |
| `ISRA.Calculations/TempComp/Validators/` | Individual validator classes |
| `ISRA.Calculations/TempComp/Domain/Results/GapAnalysisResult.cs` | Gap analysis domain model |
| `ISRA.Core/Domain/IValidationResult.cs` | Validation result interface |