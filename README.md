# ISRA_PS_AddOns

A collection of Process Simulate Add-Ons developed by ISRA Vision
for the CAD & Simulation team.

## Solution Structure

### ISRA.Components (Class Library)
Reusable hardware component definitions for PS Add-Ons.
- **AccuSite/Trackers** — Tracker geometry, FOV and camera definitions (920-0005)
- **AccuSite/Stars** — Star LED and emitter geometry definitions (515-0139)
- **AccuSite/SensorHolders** — Sensor holder base classes (placeholder)

### ISRA.Calculations (Class Library)
Reusable calculation and validation utilities.
- **AccuSite/GeometryCalculations** — Triangle, angle and visualization calculations
- **AccuSite/CollisionCheck** — Line-of-sight cylinder collision detection

### AccuSite.StarValidation (PS Add-On)
Validates the placement and orientation of AccuSite measurement stars
relative to a tracker in Process Simulate.

## Validation Criteria

### 1. Star Position Zone
Star LED center position evaluated in Tracker local coordinate system:
- Optimal : Star local Z > 0 (Mid-Far zone)
- Warning : Star local Z between -803mm and 0mm (Near-Mid zone)
- NOK     : Star local Z < -803mm (Near Field)

### 2. Emitter Visibility
Each star has 4 emitters, tracker has 3 cameras.
Min 3/4 emitters must be visible from ALL cameras.
Angle at emitter E between E->K and emitter Z vector must be <= max angle (default 40 deg).
Note: tolerance to be validated on Munich demo cell.

### 3. Line of Sight Check
Cylinder (radius 5mm) created from each camera to star self origin.
Collision with scene geometry = BLOCKED.
IMPORTANT: Tracker Design FOV entity must be hidden before analysis.

### 4. Triangle Calibration Criterion
Triangle formed by 3 Star LED centers must have height >= 500mm.
Only evaluated if all 3 stars pass criteria 1, 2 and 3.

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

## Important Notes
- Before running analysis: hide the Tracker Design FOV entity in PS
- Emitter visibility max angle (default 40 deg) to be validated on Munich demo cell