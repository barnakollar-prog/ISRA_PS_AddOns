# ISRA_PS_AddOns

A collection of Process Simulate Add-Ons developed by ISRA Vision 
for the CAD & Simulation team.

## Solution Structure

### ISRA.Components (Class Library)
Reusable hardware component definitions for PS Add-Ons.
- **AccuSite/Trackers** — Tracker geometry and FOV definitions (920-0005)
- **AccuSite/Stars** — Star LED geometry definitions (515-0139)
- **AccuSite/SensorHolders** — Sensor holder base classes (placeholder)

### ISRA.Calculations (Class Library)
Reusable calculation and validation utilities.
- **AccuSite/GeometryCalculations** — Triangle, angle and visualization calculations
- **AccuSite/CollisionCheck** — Line-of-sight and occlusion detection (placeholder)

### AccuSite.StarValidation (PS Add-On)
Validates the placement and orientation of AccuSite measurement stars
relative to a tracker in Process Simulate.

**Validation criteria:**
- Star Z vector orientation vs Tracker Z vector (XZ: max +-25 deg, YZ: max +-40 deg)
- Star position zone (Optimal / Warning / NOK)
- Triangle calibration criterion (height >= 500mm)

## Add-Ons Overview

| Add-On | Status | Description |
|---|---|---|
| AccuSite Star Validation | In Development | Star visibility and orientation validation |
| Temp Comp Validator | Planned | Temperature compensation path validation |

## Requirements
- Process Simulate 2206
- Visual Studio 2022/2026 (.NET Framework 4.8)
- EPPlusFree NuGet package

## Installation
1. Build the solution in Visual Studio (run as Administrator)
2. Register the Add-On using commandreg.exe
3. Restart Process Simulate