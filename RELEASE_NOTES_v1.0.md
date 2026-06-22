# ISRA Process Simulate Add-ons v1.0 - Release Notes

## 🎉 What's New

First stable release of ISRA Process Simulate add-ons with modern MVP architecture and complete documentation.

## 📦 Included Add-ons

### TempComp Validator v1.0
Temperature Compensation measurement path validation with Excel export.

**Features:**
- ✅ 6 comprehensive validation checks
- ✅ Excel export with 3 sheets:
  - Validation Results (Pass/Fail summary)
  - Nearest TC (Body-to-TC comparison)
  - Raw Data (Complete pose data with path names)
- ✅ Path tracking - see which WeldOperation each pose belongs to
- ✅ Color-coded results
- ✅ Statistics and analysis

### LED Visibility Analyzer v1.0
AccuSite star placement validation against tracker Field of View.

**Features:**
- ✅ Star visibility validation vs tracker FOV
- ✅ Excel export with visibility data
- ✅ Triangle calculations for complex scenarios
- ✅ Collapsible detail rows
- ✅ Color-coded results

## 🏗️ Architecture Improvements

This release includes a complete architectural refactor:

- **MVP Pattern**: Presenter-driven architecture for better testability
- **Shared Core**: `ISRA.Core` library with reusable components
- **Strategy Pattern**: Robot-specific configuration strategies
- **Shared DTOs**: Consistent data models across add-ons
- **Export Pipeline**: Standardized Excel export with EPPlus

See [REFACTORING_MAP.md](REFACTORING_MAP.md) for complete details.

## 📥 Installation

### Requirements
- Siemens Tecnomatix Process Simulate
- .NET Framework 4.8 (usually pre-installed with Windows)

### Quick Start
1. Download `ISRA_AddOns_Installer_v1.0.zip` from [Releases](https://github.com/barnakollar-prog/ISRA_PS_AddOns/releases/latest)
2. Extract the ZIP file
3. Run `INSTALL.bat` as Administrator
4. Follow the registration steps in `INSTALLATION_GUIDE.md`

👉 **Detailed instructions:** [Deployment Guide](docs/DEPLOYMENT_GUIDE.md)

## 📚 Documentation

### End Users
- [Deployment Guide](docs/DEPLOYMENT_GUIDE.md) - Installation for users without Visual Studio
- [Installation Guide](Installer/INSTALLATION_GUIDE.md) - Detailed installation and registration steps

### Feature Documentation
- [Star Visibility Analyzer](docs/README_StarVisibilityAnalyzer.md)
- [Temp Comp Validator](docs/README_TempCompValidator.md)

### Developers
- [Refactoring Map](REFACTORING_MAP.md) - Architecture overview and migration guide

## 🔧 Technical Details

### Built With
- .NET Framework 4.8
- Siemens Tecnomatix Process Simulate API
- EPPlus (Excel export)
- Emgu.CV (Computer Vision)

### Installer Contents
- TempComp Add-on DLL + icons
- LED Visibility Add-on DLL + icons
- Shared libraries (ISRA.Core, ISRA.Calculations, ISRA.Components)
- Dependencies (EPPlus, Emgu.CV, etc.)
- Installation scripts and documentation

## 🚀 What's Next

Future enhancements planned:
- Unit test coverage
- Additional robot configurations
- Enhanced validation rules
- Performance optimizations

## 📝 Changelog

### Added
- MVP architecture with presenters and view contracts
- Excel export with path tracking for TempComp
- Shared ISRA.Core library
- Robot configuration strategy pattern
- Complete installation package
- Comprehensive documentation

### Changed
- Refactored both add-ons to MVP pattern
- Moved shared logic to ISRA.Core
- Standardized export pipeline
- Improved error handling

### Fixed
- Export path tracking now shows WeldOperation names
- Installer uses correct Tecnomatix folder structure
- Registration process aligned with Process Simulate workflow

---

**Full Release Package:** [ISRA_AddOns_Installer_v1.0.zip](https://github.com/barnakollar-prog/ISRA_PS_AddOns/releases/latest)

**Repository:** https://github.com/barnakollar-prog/ISRA_PS_AddOns
