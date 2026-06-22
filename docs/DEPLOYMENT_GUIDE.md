# 📦 ISRA Process Simulate Add-Ons - Deployment Guide

## For Colleagues Without Visual Studio

This guide is for team members who want to **use** the ISRA add-ons in Process Simulate without needing to build from source.

---

## 🎯 What You Get

Two powerful add-ons for Process Simulate:

### 1. TempComp Validator
- Validates Temp Comp measurement path coverage
- Exports to Excel with 3 sheets:
  - **Validation Results** - Pass/fail summary
  - **Nearest TC** - Body-to-TC comparison  
  - **Raw Data** - Complete pose data with path names

### 2. LED Visibility Analyzer
- Validates AccuSite star placement vs tracker FOV
- Exports visibility analysis to Excel
- Shows triangle calculations for complex scenarios

---

## 📥 Installation Steps

### Step 1: Get the Installer

**Option A: Download from GitHub Releases** (Recommended)
1. Go to: https://github.com/barnakollar-prog/ISRA_PS_AddOns/releases
2. Download `ISRA_AddOns_Installer_v1.0.zip`

**Option B: Get from Network Share**
1. Ask your administrator for the installer ZIP file
2. Copy it to your local machine (e.g., `C:\Temp\`)

### Step 2: Extract the ZIP

1. Right-click `ISRA_AddOns_Installer_v1.0.zip`
2. Select **"Extract All..."**
3. Extract to a folder (e.g., `C:\Temp\ISRA_Installer\`)

### Step 3: Run Installer

1. Open the extracted folder
2. **Right-click** `INSTALL.bat`
3. Select **"Run as Administrator"**
4. The installer will:
   - Detect your Process Simulate installation automatically
   - Copy all required DLL files

### Step 4: Register Add-Ons

⚠️ **Important**: Close Process Simulate before this step!

1. Open **File Explorer** and go to your eMPower folder:
   ```
   C:\Program Files\Tecnomatix_XXXX\eMPower\
   ```
   (XXXX = 2206.0, 2408.0, or 2502.0)

2. **Right-click** `commandreg.exe` → **"Run as Administrator"**

3. **Register TempComp Validator:**
   - Browse to: `DotNetCommands\TempComp\TempCompAddon.dll`
   - Click **"Create File"** (generates .xml registration)
   - Click **"Register"**

4. **Register LED Visibility Analyzer:**
   - Browse to: `DotNetCommands\Accusite\LedVisibilityAddon.dll`
   - Click **"Create File"** (generates .xml registration)
   - Click **"Register"**

### Step 5: Add to Toolbar

1. Open **Process Simulate**
2. Go to: **Tools → Customize...**
3. In the Commands tab, find:
   - **ISRA Temp Comp Validator**
   - **ISRA LED Visibility Analyzer**
4. **Drag** them to your toolbar
5. Click **Close**

---

## ✅ Verify Installation

### Test TempComp Validator
1. Open any study with robot programs
2. Click the **TempComp Validator** button
3. Select body and TC programs
4. Click **"Analyze"**
5. Click **"Export"** to generate Excel report

### Test LED Visibility Analyzer
1. Open any study with AccuSite components
2. Click the **LED Visibility Analyzer** button
3. Select star and tracker
4. Click **"Analyze"**
5. Click **"Export"** to generate Excel report

---

## 🔧 System Requirements

- **Process Simulate**: 2206 / 2408 / 2502
- **.NET Framework 4.8** (usually already installed)
- **Windows Administrator privileges** (for installation only)
- **Microsoft Excel** (optional, for viewing reports)

---

## 📂 Installation Location

Files are installed to addon-specific subfolders:
```
C:\Program Files\Tecnomatix_XXXX\eMPower\DotNetCommands\TempComp\
C:\Program Files\Tecnomatix_XXXX\eMPower\DotNetCommands\Accusite\
```

Where `XXXX` is your version (2206.0, 2408.0, or 2502.0).

---

## 🗑️ Uninstallation

To remove the add-ons:

1. Go to the installer folder
2. **Right-click** `UNINSTALL.bat`
3. Select **"Run as Administrator"**

All files will be removed cleanly.

---

## ⚠️ Troubleshooting

### "Administrator privileges required"
- **Solution**: Right-click the batch file and select "Run as Administrator"

### "Tecnomatix installation not found"
- **Solution**: Open PowerShell as Administrator and run:
  ```powershell
  cd "C:\Path\To\Installer\Folder"
  .\Install-ISRA-Addons.ps1 -TecnomatixPath "C:\Program Files\Tecnomatix\ProcessSimulate_XXXX\bin"
  ```
  Replace `XXXX` with your version number.

### Add-ons don't appear in Customize menu
- **Solution**: Restart Process Simulate and check again

### "Load failed" error when clicking add-on
- **Solution**: Verify all DLLs are in the Process Simulate bin folder:
  - TempCompAddon.dll
  - LedVisibilityAddon.dll
  - ISRA.Core.dll
  - ISRA.Components.dll
  - ISRA.Calculations.dll
  - EPPlusFree.dll

### Excel export doesn't work
- **Solution**: Ensure EPPlusFree.dll is in the bin folder

---

## 📞 Support

**Need help?**
- Contact: ISRA Vision / CAD & Simulation Team
- GitHub Issues: https://github.com/barnakollar-prog/ISRA_PS_AddOns/issues
- Email your team administrator

---

## 🆕 What's New in v1.0

- ✨ Complete OOP architecture refactor
- ✨ Excel export with multiple sheets
- ✨ Path tracking (see which WeldOperation each pose belongs to)
- ✨ Date-based filenames for easy organization
- ✨ Color-coded validation results
- ✨ MVP pattern for maintainability

---

## 📄 More Information

For detailed installation instructions, see:
- `INSTALLATION_GUIDE.md` (in the installer folder)
- `README.md` (in the installer folder)

---

**Installation takes less than 2 minutes! 🚀**
