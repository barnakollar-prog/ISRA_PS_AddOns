# ISRA_PS_AddOns

Process Simulate Add-Ons developed by ISRA Vision / CAD & Simulation Team.

## Quick Start

### For End Users (No Visual Studio Required)
Download the latest release from the [Releases page](https://github.com/barnakollar-prog/ISRA_PS_AddOns/releases/latest).

1. Extract the ZIP file
2. Run `INSTALL.bat` as Administrator
3. Open Process Simulate — the add-ons will be available automatically

### For Developers
Clone this repository and open `ISRA_PS_AddOns.slnx` in Visual Studio.

---

## Add-Ons

| Add-On | Description | Status |
|---|---|---|
| **TempComp Validator** | Validates Temp Comp measurement path coverage with Excel export | v1.1 |
| **LED Visibility Analyzer** | Validates AccuSite star placement vs tracker FOV with Excel export | v1.0 |

---

## TempComp Validator

Validates whether a Temp Comp measurement program adequately covers the robot pose range of the Bodypart measurement paths.

### Validation Checks

| # | Criterion | Description |
|---|---|---|
| 1 | J2-3 Angle Coverage | At least 2 TC points must reach body max AND body min |
| 2 | J2-3 Range | TC J2-3 range must span at least 75° |
| 3 | J5 Symmetry | TC poses must have balanced positive and negative J5 values |
| 4 | J4 Max Coverage | TC max J4 (abs) must cover body max J4 (abs) |
| 5 | J5 Max Coverage | TC max J5 (abs) must cover body max J5 (abs) |
| 6 | J6 Max Coverage | TC max J6 (abs) must cover body max J6 (abs) |

### Robot Types

Robot type is auto-detected from the robot 3D file path and can be overridden manually.

| Robot | J2-3 Formula |
|---|---|
| Fanuc | J2 + J3 + 90 |
| Kuka | (-1) * J3 + 180 |
| ABB | (-1) * J3 + 90 |

### Axis Normalization

J4 and J6 can rotate 360° — the same physical position is reachable with different axis values. All J4/J6 values and differences are normalized to +/-180° (shortest arc).

### Measurement Point Filter

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

### Result Tabs

**Validation Tab** — Summary of all 6 validation criteria with Bodypart and Temp Comp values side by side.

**Nearest TC Point Tab** — For each body measurement point, the nearest TC point is shown with axis differences. Color coding:
- Green: difference < threshold
- Yellow: between threshold and 2x threshold
- Red: above 2x threshold

**Raw Data Tab** — All body and TC poses with joint values. J4/J6 displayed normalized (+/-180°).

### Excel Export

Exports all results to an `.xlsx` file with 3 sheets:
- **Validation Results** — all 6 criteria with Bodypart / Temp Comp values and status
- **Nearest TC** — body-to-TC point mapping with color-coded differences
- **Raw Data** — all poses with joint values and path names

---

## LED Visibility Analyzer

Validates AccuSite star placement against tracker field of view (FOV).

- Star visibility validation vs tracker FOV
- Excel export with visibility data
- Triangle calculations for complex scenarios
- Collapsible detail rows

---

## Solution Structure

```
ISRA_PS_AddOns/
├── TempCompAddon/          # TempComp Validator — UI & Presentation
├── LedVisibilityAddon/     # LED Visibility Analyzer — UI & Presentation
├── ISRA.Core/              # Shared infrastructure (MVP, validation, utilities)
├── ISRA.Components/        # AccuSite component definitions (Stars, Trackers)
├── ISRA.Calculations/      # Business logic (geometry, TempComp analysis)
└── Release_Package/        # Installer scripts and release artifacts
```

---

## Requirements

### End Users
- Siemens Process Simulate 2206 / 2408 / 2502
- .NET Framework 4.8 (usually pre-installed)
- Windows Administrator privileges (for installation only)

### Developers
- Visual Studio 2022+
- .NET Framework 4.8 SDK
- Process Simulate installation for testing

---

*ISRA Vision / CAD & Simulation Team — Last updated: June 2026*