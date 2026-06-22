# Refactoring Map: From Legacy to OOP Architecture

## 📍 Overview
This document maps the transformation from procedural code to object-oriented architecture across all refactored components.

---

## 🎯 TempCompAddon Refactoring Map

### BEFORE (Legacy Structure)

```
TempCompAddon/
└── Startup.cs (~900 lines)
	├── TempCompForm (UI + Business Logic Mixed)
	│   ├── OnAnalyze() [290+ lines of procedural code]
	│   │   ├── Input validation (inline)
	│   │   ├── Robot type detection (inline)
	│   │   ├── Pose reading (inline)
	│   │   ├── Validation logic (inline)
	│   │   ├── Distance calculations (inline)
	│   │   ├── Result formatting (inline)
	│   │   └── ListView population (inline)
	│   ├── AddValidationRow() (UI helper)
	│   └── DiffColor() (formatting helper)
	│
	└── Dependencies:
		└── ISRA.Calculations.TempComp.TempCompCalculations (static methods)
			├── ReadPosesFromProgram() (static)
			├── CheckJ23AngleCoverage() (static)
			├── CheckJ5Symmetry() (static)
			├── CheckAxisMaxCoverage() (static)
			├── FindNearestTcPoints() (static)
			└── CalculateJ23Angle() (static, uses enum for robot type)
```

### AFTER (OOP Structure)

```
TempCompAddon/
├── Startup.cs (~600 lines)
│   └── TempCompForm : TxForm, ITempCompView (UI ONLY)
│       ├── OnAnalyze() [1 line: _presenter.Analyze()]
│       ├── ITempCompView Implementation (properties + display methods)
│       └── UI Controls Management
│
└── Presentation/ [NEW]
	├── TempCompPresenter.cs (Business Logic Orchestration)
	│   ├── Analyze() (orchestrates validation workflow)
	│   ├── GetRobotConfiguration() (strategy selection)
	│   └── Dependencies:
	│       ├── ITempCompView (form contract)
	│       ├── PoseReader (data extraction)
	│       ├── TempCompAnalyzer (validation orchestration)
	│       └── IRobotConfiguration (strategy interface)
	│
	└── TempCompReportFormatter.cs (Display Logic)
		├── FormatValidationResults() (ListView formatting)
		├── FormatNearestTcResults() (with color coding)
		└── FormatRawData() (with envelope highlighting)

ISRA.Core/ [NEW PROJECT]
└── UI/
	├── IPresenter (interface)
	└── IReportFormatter (interface)

ISRA.Calculations/
└── TempComp/ [REFACTORED]
	├── Domain/ [NEW]
	│   ├── RobotPose.cs (domain model)
	│   ├── TempCompValidationInput.cs (input DTO)
	│   └── Results/ [NEW]
	│       ├── J23CoverageResult.cs
	│       ├── J5SymmetryResult.cs
	│       ├── AxisMaxCoverageResult.cs
	│       └── NearestTcResult.cs
	│
	├── RobotConfiguration/ [NEW - Strategy Pattern]
	│   ├── IRobotConfiguration.cs (interface)
	│   ├── FanucConfiguration.cs (Fanuc-specific logic)
	│   ├── KukaConfiguration.cs (Kuka-specific logic)
	│   └── AbbConfiguration.cs (ABB-specific logic)
	│
	├── Validators/ [NEW]
	│   ├── TempCompValidator.cs (base class)
	│   ├── J23AngleCoverageValidator.cs
	│   ├── J5SymmetryValidator.cs
	│   ├── J23RangeValidator.cs
	│   └── AxisMaxCoverageValidator.cs
	│       ├── CreateJ4Validator() (factory)
	│       ├── CreateJ5Validator() (factory)
	│       └── CreateJ6Validator() (factory)
	│
	├── Services/ [NEW]
	│   ├── PoseReader.cs (data extraction)
	│   │   ├── ReadPosesFromProgram()
	│   │   └── ReadPosesFromPrograms()
	│   ├── PoseStatisticsCalculator.cs (statistics)
	│   ├── DistanceCalculator.cs (distance metrics)
	│   │   ├── CalculateDistance()
	│   │   ├── FindNearest()
	│   │   └── FindNearestForAll()
	│   └── TempCompAnalyzer.cs (orchestration)
	│       ├── Analyze() (runs all validators)
	│       ├── CalculateNearestTcPoints()
	│       └── CalculateStatistics()
	│
	└── TempCompCalculations.cs [LEGACY - kept for compatibility]
		└── (static methods wrapped around new services)
```

### Transformation Summary

| Component | Before | After | Benefit |
|-----------|--------|-------|---------|
| **OnAnalyze()** | 290+ lines of mixed logic | 1 line delegation | Separation of concerns |
| **Robot Type Logic** | if/else conditionals | Strategy pattern | Open/Closed principle |
| **Validation** | Inline procedural code | 5 validator classes | Single responsibility |
| **Data Access** | Mixed with logic | PoseReader service | Testable in isolation |
| **Formatting** | Mixed with logic | Formatter class | Reusable, maintainable |

---

## 🌟 LedVisibilityAddon Refactoring Map

### BEFORE (Legacy Structure)

```
LedVisibilityAddon/
└── Startup.cs (~830 lines)
	├── VisibilityForm (UI + Business Logic Mixed)
	│   ├── OnAnalyze() [300+ lines of procedural code]
	│   │   ├── Input validation (inline)
	│   │   ├── Star position checks (inline)
	│   │   ├── Emitter visibility logic (inline)
	│   │   ├── Line of sight checks (inline)
	│   │   ├── Triangle calculations (inline)
	│   │   ├── Result formatting (inline)
	│   │   └── Collapsible row building (inline)
	│   ├── OnListDoubleClick() [30 lines of collapse logic]
	│   └── CollapsibleTag (helper class)
	│
	└── Dependencies:
		├── ISRA.Components.AccuSite.Stars.star_515_0139 (static)
		├── ISRA.Components.AccuSite.Trackers.tracker_920_0005 (static)
		├── ISRA.Calculations.AccuSite.GeometryCalculations (static)
		└── ISRA.Calculations.AccuSite.CollisionCheck (static)
```

### AFTER (OOP Structure)

```
LedVisibilityAddon/
├── Startup.cs (~600 lines)
│   └── VisibilityForm : TxForm, ILedVisibilityView (UI ONLY)
│       ├── OnAnalyze() [1 line: _presenter.Analyze()]
│       ├── OnListDoubleClick() [1 line: _formatter.ToggleCollapsibleRow()]
│       ├── ILedVisibilityView Implementation (properties + display methods)
│       └── UI Controls Management
│
└── Presentation/ [NEW]
	├── LedVisibilityPresenter.cs (Business Logic Orchestration)
	│   ├── Analyze() (orchestrates visibility analysis)
	│   ├── StarAnalysisResult (domain model)
	│   ├── TriangleAnalysisResult (domain model)
	│   └── Dependencies:
	│       ├── ILedVisibilityView (form contract)
	│       ├── Star515_0139 (component geometry)
	│       ├── Tracker920_0005 (tracker geometry)
	│       ├── GeometryCalculations (analysis service)
	│       └── CollisionCheck (collision service)
	│
	├── LedVisibilityReportFormatter.cs (Display Logic)
	│   ├── FormatStarResults() (main results)
	│   ├── FormatLineOfSightSummary() (LOS details)
	│   ├── FormatEmitterSummary() (emitter details)
	│   ├── FormatTriangleResult() (triangle display)
	│   ├── FormatTriangleSkipped() (skipped message)
	│   ├── ToggleCollapsibleRow() (expand/collapse logic)
	│   └── CollapsibleTag (encapsulated)
	│
	└── ILedVisibilityView.cs (view contract)

ISRA.Components/ [REFACTORED]
└── AccuSite/
	├── Stars/
	│   ├── IStar.cs [NEW] (interface)
	│   ├── Star515_0139.cs [NEW] (OOP class implementation)
	│   │   ├── GetEmitters()
	│   │   ├── GetLedWorldPosition()
	│   │   ├── GetEmitterWorldPosition()
	│   │   └── ComputeZVector()
	│   └── star_515_0139.cs [COMPAT] (static wrapper)
	│
	└── Trackers/
		├── ITracker.cs [NEW] (interface)
		├── Tracker920_0005.cs [NEW] (OOP class implementation)
		│   ├── GetCameras()
		│   ├── GetCameraWorldPosition()
		│   ├── IsInFOV()
		│   ├── GetPositionZone()
		│   ├── ToLocalCoordinates()
		│   └── GetZVector()
		└── tracker_920_0005.cs [COMPAT] (static wrapper)

ISRA.Calculations/
└── AccuSite/ [SCAFFOLDED]
	├── Domain/Results/ [NEW]
	│   ├── TriangleResult.cs
	│   ├── StarTrackerAngleResult.cs
	│   ├── EmitterVisibilityResult.cs
	│   ├── StarEmitterVisibilityResult.cs
	│   ├── LineOfSightResult.cs
	│   ├── StarLineOfSightResult.cs
	│   └── VisibilityAnalysisInput.cs
	│
	├── Validators/ [NEW - Scaffolded]
	│   ├── AccuSiteValidator.cs (base)
	│   └── EmitterVisibilityValidator.cs
	│
	├── Services/ [NEW - Scaffolded]
	│   └── VisibilityAnalyzer.cs
	│
	├── GeometryCalculations.cs [LEGACY - still used]
	└── CollisionCheck.cs [LEGACY - still used]
```

### Transformation Summary

| Component | Before | After | Benefit |
|-----------|--------|-------|---------|
| **OnAnalyze()** | 300+ lines of mixed logic | 1 line delegation | Clean separation |
| **Component Access** | Static method calls | Interface-based OOP | Testable, extensible |
| **Formatting** | Inline collapsible logic | Formatter class | Reusable formatting |
| **Domain Models** | Anonymous types | Proper DTOs | Type safety, clarity |
| **Collapse Logic** | 30 lines in form | 1 line delegation | Encapsulation |

---

## 🏗️ ISRA.Core: Shared Infrastructure

### Created Components

```
ISRA.Core/ [NEW PROJECT]
│
├── Domain/
│   ├── ValidationStatus.cs (enum: OK, Warning, Error)
│   ├── IValidationResult.cs (interface)
│   ├── ValidationResult.cs (implementation)
│   ├── AnalysisReport.cs (container for validation results)
│   ├── IValidator<TInput,TResult>.cs (generic validator interface)
│   └── IAnalyzer<TInput,TReport>.cs (generic analyzer interface)
│
├── UI/
│   ├── AddonFormBase.cs (base form class)
│   ├── IPresenter.cs (presenter interface)
│   └── IReportFormatter.cs (formatter interface)
│
└── Utilities/
	├── GeometryHelper.cs (shared geometry utilities)
	└── ColorPalette.cs (consistent color scheme)
		├── OKLight (green)
		├── NOKLight (red)
		├── WarningLight (yellow)
		├── Info (blue)
		└── GetDifferenceColor() (threshold-based)
```

**Purpose**: Eliminate code duplication and provide shared contracts/utilities for all addons.

---

## 🔄 Migration Patterns Applied

### Pattern 1: Procedural → Strategy Pattern

**Example: Robot Type Handling**

**Before** (TempCompCalculations):
```csharp
public static double CalculateJ23Angle(RobotPose pose, RobotType robotType)
{
	if (robotType == RobotType.Fanuc)
		return pose.J2 + pose.J3;
	else if (robotType == RobotType.Kuka)
		return pose.J2 - pose.J3;
	else if (robotType == RobotType.Abb)
		return pose.J2 + pose.J3;
	// ...
}
```

**After** (Strategy Pattern):
```csharp
// Interface
public interface IRobotConfiguration
{
	string Name { get; }
	double CalculateJ23Angle(RobotPose pose);
	double NormalizeAngle180(double angle);
}

// Implementations
public class FanucConfiguration : IRobotConfiguration
{
	public double CalculateJ23Angle(RobotPose pose) => pose.J2 + pose.J3;
}

public class KukaConfiguration : IRobotConfiguration
{
	public double CalculateJ23Angle(RobotPose pose) => pose.J2 - pose.J3;
}
```

**Location Change**:
- **From**: `TempCompCalculations.cs` (static method with enum)
- **To**: `ISRA.Calculations/TempComp/RobotConfiguration/` (strategy classes)

---

### Pattern 2: Inline Logic → Validator Classes

**Example: J2-3 Angle Coverage Check**

**Before** (inline in OnAnalyze):
```csharp
var j23 = TempCompCalculations.CheckJ23AngleCoverage(bodyPoses, tempCompPoses, robotType);
bool j23MaxOK = j23.CountMax >= 2;
bool j23MinOK = j23.CountMin >= 2;
```

**After** (Validator):
```csharp
public class J23AngleCoverageValidator : TempCompValidator
{
	protected override ValidationResult ValidateCore(TempCompValidationInput input)
	{
		// Calculate coverage
		// Return structured ValidationResult
	}
}
```

**Location Change**:
- **From**: `TempCompAddon/Startup.cs` OnAnalyze() (inline)
- **To**: `ISRA.Calculations/TempComp/Validators/J23AngleCoverageValidator.cs`

---

### Pattern 3: Mixed UI/Logic → MVP Pattern

**Example: Analyze Workflow**

**Before** (TempCompForm):
```csharp
private void OnAnalyze(object sender, EventArgs e)
{
	// 1. Validate inputs (20 lines)
	// 2. Read poses (30 lines)
	// 3. Run validations (50 lines)
	// 4. Calculate nearest TC (40 lines)
	// 5. Format validation results (50 lines)
	// 6. Format nearest TC results (60 lines)
	// 7. Format raw data (50 lines)
	// Total: ~290 lines
}
```

**After** (MVP):
```csharp
// Form (View)
private void OnAnalyze(object sender, EventArgs e)
{
	_presenter.Analyze(); // 1 line
}

// Presenter
public void Analyze()
{
	// Orchestrate services
	var input = CreateInput();
	var report = _analyzer.Analyze(input);
	_view.DisplayValidationResults(report);
	// ...
}

// Formatter
public void FormatValidationResults(AnalysisReport report, ListView listView)
{
	// Format and display
}
```

**Location Changes**:
- **UI Logic**: `TempCompAddon/Startup.cs` (unchanged location, refactored role)
- **Business Logic**: → `TempCompAddon/Presentation/TempCompPresenter.cs`
- **Formatting**: → `TempCompAddon/Presentation/TempCompReportFormatter.cs`

---

### Pattern 4: Static Wrapper → OOP + Compatibility Layer

**Example: Star Component**

**Before**:
```csharp
public static class star_515_0139
{
	public static TxVector GetLedWorldPosition(ITxLocatableObject starObj)
	{
		// Static implementation
	}
}
```

**After**:
```csharp
// Interface
public interface IStar
{
	EmitterData[] GetEmitters();
	TxVector GetLedWorldPosition(ITxLocatableObject starObj);
}

// OOP Implementation
public class Star515_0139 : IStar
{
	public TxVector GetLedWorldPosition(ITxLocatableObject starObj)
	{
		// Instance implementation
	}
}

// Backward Compatibility Wrapper
public static class star_515_0139
{
	private static readonly Star515_0139 _instance = new Star515_0139();

	public static TxVector GetLedWorldPosition(ITxLocatableObject starObj)
		=> _instance.GetLedWorldPosition(starObj);
}
```

**Location Changes**:
- **From**: `ISRA.Components/AccuSite/Stars/star_515_0139.cs` (static class)
- **To**:
  - `ISRA.Components/AccuSite/Stars/IStar.cs` (interface)
  - `ISRA.Components/AccuSite/Stars/Star515_0139.cs` (OOP class)
  - `ISRA.Components/AccuSite/Stars/star_515_0139.cs` (compat wrapper)

---

## 📊 Code Movement Summary

### TempCompAddon Code Relocations

| Original Location | New Location | Component Type |
|-------------------|--------------|----------------|
| `Startup.cs` OnAnalyze (input validation) | `TempCompPresenter.cs` Analyze() | Business Logic |
| `Startup.cs` OnAnalyze (pose reading) | `PoseReader.cs` service | Data Access |
| `Startup.cs` OnAnalyze (validation logic) | `J23AngleCoverageValidator.cs`, etc. | Validators |
| `Startup.cs` OnAnalyze (distance calc) | `DistanceCalculator.cs` service | Business Logic |
| `Startup.cs` OnAnalyze (result formatting) | `TempCompReportFormatter.cs` | Presentation |
| `Startup.cs` AddValidationRow() | `TempCompReportFormatter.cs` FormatValidationResults() | Presentation |
| `Startup.cs` DiffColor() | `ColorPalette.cs` GetDifferenceColor() | Utility |
| `TempCompCalculations.cs` ReadPosesFromProgram() | `PoseReader.cs` ReadPosesFromProgram() | Data Access |
| `TempCompCalculations.cs` robot type enum logic | `IRobotConfiguration` strategies | Strategy Pattern |
| Anonymous result types | Domain models in `Domain/Results/` | Domain Layer |

### LedVisibilityAddon Code Relocations

| Original Location | New Location | Component Type |
|-------------------|--------------|----------------|
| `Startup.cs` OnAnalyze (full workflow) | `LedVisibilityPresenter.cs` Analyze() | Business Logic |
| `Startup.cs` OnAnalyze (star analysis) | `LedVisibilityPresenter.cs` (star loop) | Business Logic |
| `Startup.cs` OnAnalyze (result formatting) | `LedVisibilityReportFormatter.cs` | Presentation |
| `Startup.cs` OnListDoubleClick (collapse logic) | `LedVisibilityReportFormatter.cs` ToggleCollapsibleRow() | Presentation |
| `Startup.cs` CollapsibleTag class | `LedVisibilityReportFormatter.cs` CollapsibleTag | Presentation |
| `star_515_0139` static class | `IStar` interface + `Star515_0139` class | OOP Layer |
| `tracker_920_0005` static class | `ITracker` interface + `Tracker920_0005` class | OOP Layer |
| Anonymous star result types | `StarAnalysisResult` domain model | Domain Layer |
| Anonymous triangle result types | `TriangleAnalysisResult` domain model | Domain Layer |

### ISRA.Calculations Code Additions

| Component | Location | Type |
|-----------|----------|------|
| RobotPose | `TempComp/Domain/RobotPose.cs` | Domain Model |
| TempCompValidationInput | `TempComp/Domain/TempCompValidationInput.cs` | DTO |
| J23CoverageResult, etc. | `TempComp/Domain/Results/*.cs` | Domain Models |
| IRobotConfiguration | `TempComp/RobotConfiguration/IRobotConfiguration.cs` | Interface |
| FanucConfiguration, etc. | `TempComp/RobotConfiguration/*Configuration.cs` | Strategies |
| TempCompValidator | `TempComp/Validators/TempCompValidator.cs` | Base Class |
| J23AngleCoverageValidator, etc. | `TempComp/Validators/*.cs` | Validators |
| PoseReader | `TempComp/Services/PoseReader.cs` | Service |
| DistanceCalculator | `TempComp/Services/DistanceCalculator.cs` | Service |
| TempCompAnalyzer | `TempComp/Services/TempCompAnalyzer.cs` | Orchestrator |
| VisibilityAnalysisInput | `AccuSite/Domain/VisibilityAnalysisInput.cs` | DTO |
| TriangleResult, etc. | `AccuSite/Domain/Results/*.cs` | Domain Models |
| AccuSiteValidator | `AccuSite/Validators/AccuSiteValidator.cs` | Base Class |
| VisibilityAnalyzer | `AccuSite/Services/VisibilityAnalyzer.cs` | Orchestrator |

---

## 🎯 Architecture Layers Comparison

### Before: Two-Layer Monolithic

```
┌─────────────────────────────────────────┐
│         Presentation Layer              │
│  (UI Forms + Business Logic + Display)  │
│                                         │
│  • TempCompForm (~900 lines)           │
│  • VisibilityForm (~830 lines)         │
│  • All logic mixed together            │
└─────────────────────────────────────────┘
					↓
┌─────────────────────────────────────────┐
│         Data/Calculation Layer          │
│        (Static utility methods)         │
│                                         │
│  • TempCompCalculations (static)        │
│  • GeometryCalculations (static)        │
│  • CollisionCheck (static)              │
└─────────────────────────────────────────┘
```

### After: Five-Layer Clean Architecture

```
┌─────────────────────────────────────────┐
│         Presentation Layer              │
│              (UI Only)                  │
│                                         │
│  • TempCompForm : ITempCompView         │
│  • VisibilityForm : ILedVisibilityView  │
│  • Pure UI control management          │
└─────────────────────────────────────────┘
					↓
┌─────────────────────────────────────────┐
│          Presenter Layer                │
│       (Business Orchestration)          │
│                                         │
│  • TempCompPresenter                    │
│  • LedVisibilityPresenter               │
│  • Workflow coordination                │
└─────────────────────────────────────────┘
					↓
┌─────────────────────────────────────────┐
│         Service Layer                   │
│       (Business Logic)                  │
│                                         │
│  • PoseReader                           │
│  • DistanceCalculator                   │
│  • TempCompAnalyzer                     │
│  • VisibilityAnalyzer                   │
└─────────────────────────────────────────┘
					↓
┌─────────────────────────────────────────┐
│         Domain Layer                    │
│    (Models, Validators, Strategies)     │
│                                         │
│  • RobotPose, ValidationResult          │
│  • TempCompValidator hierarchy          │
│  • IRobotConfiguration strategies       │
└─────────────────────────────────────────┘
					↓
┌─────────────────────────────────────────┐
│    Infrastructure/Component Layer       │
│      (Data Access, Components)          │
│                                         │
│  • IStar, ITracker interfaces           │
│  • Component implementations            │
│  • Process Simulate API wrappers        │
└─────────────────────────────────────────┘
```

---

## 📈 Metrics: Code Distribution Changes

### TempCompAddon

**Before**:
```
Startup.cs: 900 lines
  ├── UI Code: ~250 lines (28%)
  ├── Business Logic: ~450 lines (50%)
  └── Display Logic: ~200 lines (22%)
```

**After**:
```
Startup.cs: 600 lines
  └── UI Code: 600 lines (100%)

TempCompPresenter.cs: 150 lines
  └── Business Logic: 150 lines (100%)

TempCompReportFormatter.cs: 220 lines
  └── Display Logic: 220 lines (100%)

Services (PoseReader, etc.): ~400 lines
  └── Business Logic: 400 lines (100%)
```

**Result**: Each component has single, clear responsibility

### LedVisibilityAddon

**Before**:
```
Startup.cs: 830 lines
  ├── UI Code: ~230 lines (28%)
  ├── Business Logic: ~400 lines (48%)
  └── Display Logic: ~200 lines (24%)
```

**After**:
```
Startup.cs: 600 lines
  └── UI Code: 600 lines (100%)

LedVisibilityPresenter.cs: 180 lines
  └── Business Logic: 180 lines (100%)

LedVisibilityReportFormatter.cs: 280 lines
  └── Display Logic: 280 lines (100%)
```

---

## 🏆 Key Transformation Achievements

### 1. Separation of Concerns
- **Before**: UI, logic, and display mixed in single files
- **After**: Clear layers with distinct responsibilities

### 2. Testability
- **Before**: Cannot test without UI
- **After**: Presenters, services, validators all testable in isolation

### 3. Maintainability
- **Before**: 900-line procedural methods
- **After**: 50-150 line focused classes

### 4. Extensibility
- **Before**: Modify large methods for changes
- **After**: Add new validators/strategies without changing existing code

### 5. Code Reuse
- **Before**: Duplicate code across addons
- **After**: Shared infrastructure in ISRA.Core

---

## 📤 Excel Export Features (Phase 5)

### TempComp Excel Export

**New Service:** `TempCompAddon/Services/TempCompExcelExporter.cs`

**Features:**
- **3 Excel Worksheets:**
  1. **Validation Results**: Pass/fail summary for all 6 validators with color coding
  2. **Nearest TC**: Body-to-TC comparison (J2-3, J4, J5, J6, Max Diff) with threshold highlighting
  3. **Raw Data**: Complete pose data with J1-J6 values and path names

**Architecture:**
```
TempCompPresenter.cs
├── Export() - Validates results exist
└── PrepareExportData() - Builds TempCompExportData DTO
    └── TempCompExportData.cs (DTO)
        ├── ValidationReport
        ├── NearestTcResults
        ├── BodyPoses (with PathName)
        ├── TempCompPoses (with PathName)
        ├── RobotConfiguration
        ├── MaxAngleThreshold
        └── Statistics
            └── TempCompExcelExporter.Export(data)
                ├── CreateValidationSheet()
                ├── CreateNearestTcSheet()
                └── CreateRawDataSheet()
```

**MVP Pattern:**
- Presenter owns export orchestration
- Form delegates via `ShowExportDialog(TempCompExportData)`
- Export button positioned above Help/About (matches LedVisibility)

### LedVisibility Export Refactor

**Refactored to MVP:**
- Moved export logic from `OnExport()` in Startup.cs to `LedVisibilityPresenter.Export()`
- Created `ExportData.cs` DTO
- Updated `ILedVisibilityView` with `HasResults` and `ShowExportDialog(ExportData)`
- Maintains existing Excel format (single sheet with star visibility + optional triangle)

### Path Name Tracking

**Domain Enhancement:** `ISRA.Calculations/TempComp/Domain/RobotPose.cs`
- Added `PathName` property to track parent WeldOperation

**Service Update:** `ISRA.Calculations/TempComp/Services/PoseReader.cs`
- `ReadPosesFromProgram()` now captures and populates `PathName`

**UI Enhancement:**
- Raw Data tab displays **"Body Path"** and **"TC Path"** columns
- Excel Raw Data sheet includes **"Path"** column for traceability

**Benefits:**
- Easy identification of which WeldOperation each pose belongs to
- Improved debugging and issue tracing
- Matches Process Simulate hierarchical structure

---

## 🗺️ Quick Reference: "Where Did X Go?"

| I'm looking for... | It moved to... |
|-------------------|----------------|
| TempComp OnAnalyze validation logic | `TempCompPresenter.cs` + validators |
| TempComp pose reading | `PoseReader.cs` service |
| TempComp ListView formatting | `TempCompReportFormatter.cs` |
| Robot-specific angle calculations | `IRobotConfiguration` strategy classes |
| TempComp distance calculations | `DistanceCalculator.cs` service |
| **TempComp export logic** | **`TempCompExcelExporter.cs` service** |
| **TempComp export data preparation** | **`TempCompPresenter.PrepareExportData()`** |
| LedVisibility OnAnalyze workflow | `LedVisibilityPresenter.cs` |
| **LedVisibility export logic** | **`LedVisibilityPresenter.Export()`** |
| LedVisibility collapsible row logic | `LedVisibilityReportFormatter.cs` |
| Star component methods | `IStar` interface + `Star515_0139` class |
| Tracker component methods | `ITracker` interface + `Tracker920_0005` class |
| Color coding logic | `ColorPalette.cs` in ISRA.Core |
| Validation base classes | `IValidator<T>` in ISRA.Core |
| **Path/WeldOperation tracking** | **`RobotPose.PathName` property** |

---

**This map shows the complete transformation from legacy procedural code to modern OOP architecture with comprehensive export capabilities!** 🚀
