# Creating a GitHub Release

This guide shows how to create a GitHub Release so colleagues can easily download the installer.

## 📤 Steps to Create a Release

### 1. Go to GitHub Releases

1. Open your browser and go to:
   ```
   https://github.com/barnakollar-prog/ISRA_PS_AddOns/releases
   ```

2. Click **"Draft a new release"** button

### 2. Create a Tag

1. Click **"Choose a tag"**
2. Type: `v1.0.0`
3. Click **"Create new tag: v1.0.0 on publish"**

### 3. Fill in Release Information

**Release title:**
```
ISRA Process Simulate Add-Ons v1.0.0
```

**Description:**
```markdown
## 🎉 First Official Release - v1.0.0

Complete OOP refactoring with Excel export capabilities for both TempComp and LED Visibility add-ons.

### 📦 Installation

**For end users (no Visual Studio required):**
1. Download `ISRA_AddOns_Installer_v1.0.zip` below
2. Extract the ZIP file
3. Right-click `INSTALL.bat` and select "Run as Administrator"
4. See [Deployment Guide](docs/DEPLOYMENT_GUIDE.md) for detailed instructions

### ✨ What's New

#### TempComp Validator
- ✅ Complete MVP architecture refactor
- ✅ Excel export with 3 sheets (Validation Results, Nearest TC, Raw Data)
- ✅ Path tracking - see which WeldOperation each pose belongs to
- ✅ Date-based filenames for easy organization
- ✅ Color-coded validation results
- ✅ Comprehensive pose statistics

#### LED Visibility Analyzer
- ✅ MVP architecture refactor  
- ✅ Excel export with star visibility data
- ✅ Improved presenter/view separation
- ✅ Enhanced maintainability

### 🔧 System Requirements

- **Process Simulate**: 2206 / 2408 / 2502
- **.NET Framework**: 4.8 (usually pre-installed)
- **Windows**: with Administrator privileges

### 📚 Documentation

- [Deployment Guide](docs/DEPLOYMENT_GUIDE.md) - For end users
- [Installation Guide](Installer/INSTALLATION_GUIDE.md) - Detailed instructions
- [Refactoring Map](REFACTORING_MAP.md) - Architecture overview
- [README](README.md) - Project overview

### 🏗️ Technical Details

**Architecture:**
- MVP pattern with clean separation of concerns
- Strategy pattern for robot configurations
- Shared infrastructure in ISRA.Core
- Domain logic in ISRA.Calculations

**Commits included:**
- Excel export features with MVP pattern and path tracking
- REFACTORING_MAP.md documentation
- Installer package for end-user distribution
- Deployment guide and README updates

### 📝 What's Included

**Add-Ons:**
- TempCompAddon.dll
- LedVisibilityAddon.dll

**Dependencies:**
- ISRA.Core.dll
- ISRA.Components.dll
- ISRA.Calculations.dll
- EPPlusFree.dll (for Excel export)

**Installation Files:**
- INSTALL.bat (double-click installer)
- UNINSTALL.bat (removal tool)
- PowerShell scripts with auto-detection
- Comprehensive documentation

### ⚠️ Important Notes

- Installation requires Administrator privileges
- Automatically detects Process Simulate installation
- Supports multiple Process Simulate versions simultaneously
- Clean uninstallation available

### 🐛 Known Issues

None reported. Please create an issue if you encounter problems.

### 🙏 Credits

Developed by ISRA Vision / CAD & Simulation Team
```

### 4. Upload the Installer ZIP

1. Scroll down to **"Attach binaries"**
2. Click or drag the file:
   ```
   C:\Users\a00598789\source\repos\ISRA_PS_AddOns\ISRA_AddOns_Installer_v1.0.zip
   ```
3. Wait for upload to complete

### 5. Set as Latest Release

1. Check ✅ **"Set as the latest release"**
2. Leave **"Set as a pre-release"** unchecked (this is a stable release)

### 6. Publish

1. Click **"Publish release"**
2. Done! 🎉

---

## 📥 After Publishing

### Share the Download Link

Send colleagues this link:
```
https://github.com/barnakollar-prog/ISRA_PS_AddOns/releases/latest/download/ISRA_AddOns_Installer_v1.0.zip
```

This is a **direct download link** that always points to the latest version.

### Share the Release Page

Or send the release page:
```
https://github.com/barnakollar-prog/ISRA_PS_AddOns/releases
```

---

## 🔄 Creating Future Releases

For future updates:

1. **Update version numbers:**
   - Tag: `v1.1.0`, `v1.2.0`, etc.
   - ZIP filename: `ISRA_AddOns_Installer_v1.1.zip`

2. **Rebuild the package:**
   ```powershell
   .\Installer\Build-Simple.ps1
   Compress-Archive -Path ".\Release_Package\*" -DestinationPath ".\ISRA_AddOns_Installer_v1.1.zip"
   ```

3. **Create new release** on GitHub with updated version

---

## ✅ Verification

After publishing, verify:

1. ✅ Release appears on the releases page
2. ✅ ZIP file is downloadable
3. ✅ Direct download link works
4. ✅ Documentation links work in the release description

---

## 📞 Need Help?

If you have issues creating the release:
- GitHub Releases Documentation: https://docs.github.com/en/repositories/releasing-projects-on-github
- Contact your team's GitHub administrator

---

**Once published, colleagues can install the add-ons in under 2 minutes!** 🚀
