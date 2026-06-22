# Installer Directory

This directory contains scripts to build and package the ISRA Process Simulate Add-Ons installer.

## For Developers (Building the Installer)

### Quick Build

Run this PowerShell script to build and package everything:

```powershell
.\Build-Package.ps1
```

This will:
1. Build the solution in Release mode
2. Collect all DLLs and dependencies
3. Create a `Release_Package` folder with installer files
4. Optionally create a ZIP file for distribution

### Output

The script creates:
```
Release_Package/
├── Binaries/
│   ├── TempCompAddon.dll
│   ├── LedVisibilityAddon.dll
│   ├── ISRA.Core.dll
│   ├── ISRA.Components.dll
│   ├── ISRA.Calculations.dll
│   └── EPPlusFree.dll
├── INSTALL.bat
├── UNINSTALL.bat
├── Install-ISRA-Addons.ps1
├── INSTALLATION_GUIDE.md
├── README.md
└── VERSION.txt
```

### Distribution

After running `Build-Package.ps1`:

1. **Test the installer** on a clean Process Simulate installation
2. **Create ZIP** for distribution (script will offer to do this)
3. **Share** `ISRA_AddOns_Installer_v1.0.zip` with colleagues

## For End Users (Installing Add-Ons)

If you received the installer package:

1. **Extract** the ZIP file to a folder
2. **Right-click** `INSTALL.bat` and select **"Run as Administrator"**
3. Follow the on-screen instructions
4. See `INSTALLATION_GUIDE.md` for detailed instructions

## Files in This Directory

| File | Purpose |
|------|---------|
| `Build-Package.ps1` | **Developer tool** - Builds solution and creates installer package |
| `Install-ISRA-Addons.ps1` | **Core installer** - PowerShell script that does the actual installation |
| `INSTALL.bat` | **User-friendly launcher** - Double-click to install (runs PowerShell script) |
| `UNINSTALL.bat` | **User-friendly uninstaller** - Double-click to uninstall |
| `INSTALLATION_GUIDE.md` | **End-user documentation** - Detailed installation instructions |
| `README.md` | This file - Developer and user reference |

## Installation Features

✅ **Automatic detection** of Process Simulate installation  
✅ **Admin privilege checking**  
✅ **File copying** to Tecnomatix bin folder  
✅ **Command registration** with Process Simulate  
✅ **Clean uninstallation** support  
✅ **Error handling** and user-friendly messages  

## Supported Process Simulate Versions

- Process Simulate 2206
- Process Simulate 2408
- Process Simulate 2502

## Manual Installation Path Override

If automatic detection fails, users can specify the path:

```powershell
.\Install-ISRA-Addons.ps1 -TecnomatixPath "C:\Program Files\Tecnomatix\ProcessSimulate_2502\bin"
```

## Requirements

- Windows with Administrator privileges
- .NET Framework 4.8
- Siemens Process Simulate (one of the supported versions)

## Troubleshooting

See `INSTALLATION_GUIDE.md` for detailed troubleshooting steps.

## Version History

**v1.0** (Current)
- Initial release
- TempComp Validator with Excel export
- LED Visibility Analyzer with Excel export
- MVP architecture
- Path tracking in exports
