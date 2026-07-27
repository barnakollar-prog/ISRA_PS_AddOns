# ISRA_PS_AddOns

Process Simulate Add-Ons developed by ISRA Vision / CAD & Simulation Team.

## Quick Start

### For End Users
1. Copy the latest release folder from the network share:
   `Z:\2000__Design-KnowHow-Database\9990_Programmierung\031_PS_DOT_NET_API\`
2. Paste the contents to your PS installation:
   `C:\Program Files\Tecnomatix_2408\eMPower\DotNetCommands\`
3. Run `UNBLOCK.bat` to remove Windows file blocking
4. Register the add-ons using `commandreg.exe` (first install only)
5. Open Process Simulate — the add-ons will be available automatically

### For Developers
See [Developer Onboarding Guide](https://dev.azure.com/ac-it-mvs/cad-simulation/_wiki/wikis/cad-simulation.wiki/ISRA_CAD_AddOns_Developer_Onboarding) for full setup instructions.

Clone this repository and open `ISRA_PS_AddOns.slnx` in Visual Studio.

Repository: `https://dev.azure.com/ac-it-mvs/cad-simulation/_git/ISRA_PS_AddOns`

---

## Add-Ons

| Add-On | Description | Status |
|---|---|---|
| **TempComp Validator** | Validates Temp Comp measurement path coverage with Excel export | v1.3 |
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
| 4 | J4 Max Coverage | Min 2 TC points >= body positive max AND min 2 TC points <= body negative min |
| 5 | J5 Max Coverage | Min 2 TC points >= body positive max AND min 2 TC points <= body negative min |
| 6 | J6 Max Coverage | Min 2 TC points >= body positive max AND min 2 TC points <= body negative min |

### Robot Types

Robot type is auto-detected from the robot 3D file path and can be overridden manually.

| Robot | J2-3 Formula |
|---|---|
| Fanuc | J2 + J3 + 90 |
| Kuka | (-1) * J3 + 180 |
| ABB | (-1) * J3 + 90 |

### Axis Values

J4/J6 values are displayed and evaluated as raw (non-normalized) values in the Validation tab, Raw Data tab, and Gap Analysis tab.

J4/J6 **differences** in the Nearest TC Point tab are normalized to +/-180° (shortest arc) for distance calculation purposes only.

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

### Path Selection

Supports multiple operation types:
- **WeldOperation** — added directly
- **Compound Operation** — child WeldOperations expanded automatically
- **Generic Robotic Operation** — child WeldOperations expanded automatically

### Result Tabs

**Validation Tab** — Summary of all 6 validation criteria with Bodypart and Temp Comp values side by side. J4/J5/J6 show positive and negative peak values (+X° / -Y°) and coverage counts per direction.

**Nearest TC Point Tab** *(Experimental)* — For each body measurement point, the nearest TC point is shown with axis differences and source path/point name. Color coding:
- Green: difference < threshold
- Yellow: between threshold and 2x threshold
- Red: above 2x threshold

**Raw Data Tab** — All body and TC poses with joint values and path names. J4/J6 displayed as raw (non-normalized) values. Columns can be sorted by clicking the header.

**Gap Analysis Tab** — TC measurement point values sorted per axis (J2, J3, J4, J5, J6, J2-3). User-configurable gap threshold per axis (default 25°). Red cells indicate gaps exceeding the threshold. Each value shows source program and point name for traceability.

### Excel Export

Exports all results to an `.xlsx` file with 4 sheets:
- **Validation Results** — all 6 criteria with Bodypart / Temp Comp values and status
- **Nearest TC** — body-to-TC point mapping with color-coded differences and path names
- **Raw Data** — all poses with raw joint values and path names
- **Gap Analysis** — sorted TC and Body values per axis with gap highlighting and line charts

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

## Known Issues

### ABB Robot — Missing Joint Values
When ABB robots have inconsistent robot configuration data (config flags
do not match stored joint values), the analyzer cannot read joint values
for affected locations. These locations will be skipped in the analysis.

**Workaround:** Manually teach (touch up) the affected locations in PS
before running the analysis.

**Status:** Siemens support ticket raised. Investigating programmatic solution
via PS API.

---

## Requirements

### End Users
- Siemens Process Simulate 2408
- .NET Framework 4.8 (usually pre-installed)
- Windows Administrator privileges (for CommandReg registration only)

### Developers
- Visual Studio 2022+
- .NET Framework 4.8 SDK
- Process Simulate 2408 installation for testing
- Azure DevOps access (atlascopco-itba organization)

---

*ISRA Vision / CAD & Simulation Team — Last updated: July 2026*
