# ISRA Process Simulate Add-Ons - Installation Guide

## 📦 Package Contents

This installer package includes:
- **TempComp Validator** - Validates Temp Comp measurement path coverage
- **LED Visibility Analyzer** - Validates AccuSite star placement vs tracker FOV

## 🔧 System Requirements

- **Siemens Process Simulate** 2206 / 2408 / 2502
- **.NET Framework 4.8** (usually already installed with Process Simulate)
- **Windows** with Administrator privileges

## 📥 Installation Steps

### Automatic Installation (Recommended)

1. **Extract** the installer package to a folder (e.g., `C:\Temp\ISRA_Addons`)

2. **Right-click** on `INSTALL.bat` and select **"Run as Administrator"**

3. The installer will:
   - Automatically detect your Process Simulate installation
   - Copy all required DLL files
   - Register both add-ons with Process Simulate

4. **Open Process Simulate** after installation

5. **Add toolbar buttons:**
   - Go to: `Tools → Customize...`
   - Find "ISRA Temp Comp Validator" and "ISRA LED Visibility Analyzer"
   - Drag them to your toolbar

### Manual Installation (If Automatic Fails)

If the automatic installer cannot find your Tecnomatix installation:

1. **Right-click** on PowerShell and select **"Run as Administrator"**

2. Navigate to the installer folder:
   ```powershell
   cd "C:\Path\To\Installer\Folder"
   ```

3. Run with custom path:
   ```powershell
   .\Install-ISRA-Addons.ps1 -TecnomatixPath "C:\Program Files\Tecnomatix\ProcessSimulate_XXXX\bin"
   ```
   (Replace `XXXX` with your version: 2206, 2408, or 2502)

## 🧪 Verify Installation

After installation, verify the add-ons are working:

1. Open Process Simulate
2. Open any study with robot programs
3. Click the **TempComp Validator** button from your toolbar
4. Click the **LED Visibility Analyzer** button from your toolbar

Both windows should open without errors.

## 📤 Export Features

### TempComp Validator
- Click "Analyze" to validate robot poses
- Click "Export" to generate Excel report with 3 sheets:
  - **Validation Results** - Pass/fail summary
  - **Nearest TC** - Body-to-TC comparison
  - **Raw Data** - Complete pose data with path names

### LED Visibility Analyzer
- Click "Analyze" to check star visibility
- Click "Export" to generate Excel report with star positions and visibility results

## 🗑️ Uninstallation

To remove the add-ons:

1. **Right-click** on `UNINSTALL.bat` and select **"Run as Administrator"**

2. The uninstaller will:
   - Unregister both add-ons
   - Remove all DLL files from Process Simulate bin folder

## ⚠️ Troubleshooting

### "Administrator privileges required" error
- Right-click the batch file and select "Run as Administrator"

### "Tecnomatix installation not found" error
- Use manual installation with `-TecnomatixPath` parameter
- Ensure Process Simulate is installed

### "Add-ons don't appear in Customize menu"
- Restart Process Simulate after installation
- Check if installation completed without errors

### "Load failed" error when clicking add-on button
- Verify all DLL files are in the Process Simulate bin folder:
  - TempCompAddon.dll
  - LedVisibilityAddon.dll
  - ISRA.Core.dll
  - ISRA.Components.dll
  - ISRA.Calculations.dll
  - EPPlusFree.dll

### Excel export doesn't work
- Ensure EPPlusFree.dll is in the bin folder
- Check that you have write permissions to the export location

## 📂 Installation Location

By default, files are installed to:
```
C:\Program Files\Tecnomatix\ProcessSimulate_XXXX\bin\
```

## 🔄 Updating to New Version

To update to a newer version:

1. Run `UNINSTALL.bat` first (optional but recommended)
2. Extract the new installer package
3. Run `INSTALL.bat` from the new package

## 📞 Support

For issues or questions, contact:
- ISRA Vision / CAD & Simulation Team
- GitHub: https://github.com/barnakollar-prog/ISRA_PS_AddOns

## 📄 Version Information

**Version:** 1.0  
**Date:** 2025  
**Compatible with:**
- Process Simulate 2206
- Process Simulate 2408
- Process Simulate 2502

## 🔐 Security Note

The installer requires Administrator privileges to copy files to the Program Files directory and register commands with Process Simulate. This is normal and required for proper installation.
