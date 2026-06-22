# 📦 ISRA Add-ons Distribution - Private Repository

Since this repository is **private**, the installer is distributed via network share or direct file transfer.

## 📥 For End Users

### Download Location
Contact your administrator for the installer package location:
- Network share: `\\[your-server]\shared\ISRA_Tools\ISRA_AddOns_Installer_v1.0.zip`
- Or request the file via email/Teams

### Installation Steps
1. Copy `ISRA_AddOns_Installer_v1.0.zip` to your local machine (e.g., `C:\Temp\`)
2. Extract the ZIP file
3. Run `INSTALL.bat` as Administrator
4. Follow instructions in `INSTALLATION_GUIDE.md`

## 📋 For Administrators

### Setting Up Network Share

1. Copy the installer to your shared location:
   ```
   Source: C:\Users\a00598789\source\repos\ISRA_PS_AddOns\ISRA_AddOns_Installer_v1.0.zip
   Destination: \\your-server\shared\ISRA_Tools\
   ```

2. Set appropriate permissions (Read access for all users)

3. Share the path with your team:
   ```
   \\your-server\shared\ISRA_Tools\ISRA_AddOns_Installer_v1.0.zip
   ```

### Email Distribution

Attach `ISRA_AddOns_Installer_v1.0.zip` (10.94 MB) to email or upload to:
- Microsoft Teams shared files
- SharePoint document library
- OneDrive with sharing link
- Company intranet

## 📧 Sample Distribution Email

```
Subject: 🎉 ISRA Process Simulate Add-ons v1.0 Available

Hi Team,

The ISRA Process Simulate Add-ons v1.0 are now available!

📥 Download from:
[Network path or attachment]

📦 What's included:
• TempComp Validator - Excel export with path tracking
• LED Visibility Analyzer - FOV validation

🚀 Installation:
1. Extract ISRA_AddOns_Installer_v1.0.zip
2. Run INSTALL.bat as Administrator
3. Follow INSTALLATION_GUIDE.md

📚 Complete documentation is included in the package.

Questions? Contact [your name/team]
```

## 🔄 Version Updates

When releasing updates:
1. Run `Installer\Build-Simple.ps1` to rebuild package
2. Rename to `ISRA_AddOns_Installer_vX.X.zip` (increment version)
3. Copy to network share
4. Notify users of the update

## 🔒 Access Control

Since the repository is private:
- ✅ Source code remains internal
- ✅ Only authorized personnel have GitHub access
- ✅ End users receive installer packages only
- ✅ Documentation travels with the installer

---

**Current Version:** v1.0  
**Package Size:** 10.94 MB  
**Package Location:** `C:\Users\a00598789\source\repos\ISRA_PS_AddOns\ISRA_AddOns_Installer_v1.0.zip`
