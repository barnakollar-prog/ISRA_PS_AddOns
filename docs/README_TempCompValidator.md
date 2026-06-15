# Temp Comp Validator

Process Simulate Add-On for validating Temp Comp measurement path coverage.

## Purpose
Validates whether a Temp Comp measurement program adequately covers
the robot pose range of the Bodypart measurement paths.

## Usage
1. Select robot (auto-detects type from 3D file path)
2. Pick Bodypart measurement paths
3. Pick Temp Comp paths
4. Click Analyze

## Validation Criteria
| # | Criterion | Requirement |
|---|---|---|
| 1 | J2-J3 Angle Max | Min 2 TC points >= body max |
| 2 | J2-J3 Angle Min | Min 2 TC points <= body min |
| 3 | J2-3 Range | TC range >= 75 deg |
| 4 | J5 Symmetry | Balanced neg/pos J5 values |
| 5 | J4 Max | TC max (abs) >= body max (abs) |
| 6 | J5 Max | TC max (abs) >= body max (abs) |
| 7 | J6 Max | TC max (abs) >= body max (abs) |

## J2-3 Angle Formula
| Robot | Formula |
|---|---|
| Fanuc | J2 + J3 + 90 |
| Kuka | (-1) * J3 + 180 |
| ABB | (-1) * J3 + 90 |

## J4/J6 Normalization
J4 and J6 are normalized to ±180° (shortest arc):
`dA = ((d + 180) mod 360) - 180`

## Measurement Point Filter
Points are filtered by:
1. **Name prefix**: `mp` (body), `art`/`temp` (TC)
2. **OLP command text** (fallback): `meas`, `cmeas`, `inline`, `VW_USER`, `TECH10`, `PRC_IMT`

Extend keywords in `MeasurementPointFilter.cs`.

## Nearest TC Point
Weighted Euclidean distance: W_J23=2.0, W_J4=W_J5=W_J6=1.0

Color coding (default threshold 35 deg):
- 🟢 Green: diff < threshold
- 🟡 Yellow: threshold – 2x threshold  
- 🔴 Red: > 2x threshold

## Files
- `TempCompAddon/Startup.cs` — UI and main logic
- `ISRA.Calculations/TempComp/TempCompCalculations.cs` — calculation logic
- `ISRA.Calculations/TempComp/MeasurementPointFilter.cs` — point filter