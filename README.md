# ISRA_PS_AddOns

Process Simulate Add-Ons developed by ISRA Vision / CAD & Simulation Team.

## Add-Ons

| Add-On | Description | Status |
|---|---|---|
| Star Visibility Analyzer | Validates AccuSite star placement vs tracker FOV | v1.0 stable |
| Temp Comp Validator | Validates Temp Comp measurement path coverage | active dev |

## Documentation
- [Star Visibility Analyzer](docs/README_StarVisibilityAnalyzer.md)
- [Temp Comp Validator](docs/README_TempCompValidator.md)

## Solution Structure
- `LedVisibilityAddon` — Star Visibility Analyzer Add-On
- `TempCompAddon` — Temp Comp Validator Add-On
- `ISRA.Components` — AccuSite component definitions (Stars, Trackers)
- `ISRA.Calculations` — Geometry and TempComp calculation logic

## Requirements
- Siemens Process Simulate 2206 / 2408 / 2502
- .NET Framework 4.8
- Visual Studio 2019+