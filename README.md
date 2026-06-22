# ISRA_PS_AddOns

Process Simulate Add-Ons developed by ISRA Vision / CAD & Simulation Team.

## 🎯 Quick Start

### For End Users (No Visual Studio Required)
📥 **[Download Installer](../../releases)** → Extract → Run `INSTALL.bat` as Administrator

👉 See [Deployment Guide](docs/DEPLOYMENT_GUIDE.md) for detailed installation instructions.

### For Developers
Clone this repository and open `ISRA_PS_AddOns.slnx` in Visual Studio.

---

## Add-Ons

| Add-On | Description | Status |
|---|---|---|
| **TempComp Validator** | Validates Temp Comp measurement path coverage with Excel export | ✅ v1.0 |
| **LED Visibility Analyzer** | Validates AccuSite star placement vs tracker FOV with Excel export | ✅ v1.0 |

## ✨ Features

### TempComp Validator
- ✅ 6 comprehensive validation checks
- ✅ Excel export with 3 sheets (Validation, Nearest TC, Raw Data)
- ✅ Path tracking - see which WeldOperation each pose belongs to
- ✅ Color-coded results
- ✅ Statistics and analysis

### LED Visibility Analyzer
- ✅ Star visibility validation vs tracker FOV
- ✅ Excel export with visibility data
- ✅ Triangle calculations for complex scenarios
- ✅ Collapsible detail rows

## 📚 Documentation

### End Users
- **[Deployment Guide](docs/DEPLOYMENT_GUIDE.md)** - Installation for end users
- **[Installation Guide](Installer/INSTALLATION_GUIDE.md)** - Detailed installation steps

### Feature Documentation
- [Star Visibility Analyzer](docs/README_StarVisibilityAnalyzer.md)
- [Temp Comp Validator](docs/README_TempCompValidator.md)

### Developers
- **[Refactoring Map](REFACTORING_MAP.md)** - Architecture overview and migration guide
- **[Refactoring Summary](REFACTORING_SUMMARY.md)** - Detailed refactoring documentation

## 🏗️ Solution Structure

```
ISRA_PS_AddOns/
├── TempCompAddon/          # Temp Comp Validator UI & Presentation
├── LedVisibilityAddon/     # LED Visibility Analyzer UI & Presentation
├── ISRA.Core/              # Shared infrastructure (MVP, validation, utilities)
├── ISRA.Components/        # AccuSite component definitions (Stars, Trackers)
├── ISRA.Calculations/      # Business logic (geometry, TempComp analysis)
└── Installer/              # Installation scripts and documentation
```

## 🔧 Requirements

### End Users
- Siemens Process Simulate 2206 / 2408 / 2502
- .NET Framework 4.8 (usually pre-installed)
- Windows Administrator privileges (for installation only)

### Developers
- Visual Studio 2019+ or Visual Studio 2022+
- .NET Framework 4.8 SDK
- Process Simulate installation for testing