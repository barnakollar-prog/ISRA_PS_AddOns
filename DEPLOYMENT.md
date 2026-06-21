# Deployment Guide for ISRA PS AddOns

## Overview
This document describes how to deploy the refactored ISRA Process Simulate AddOns after building.

## Build Configuration
- **Target Framework**: .NET Framework 4.8
- **Platform**: AnyCPU or x64
- **Configuration**: Debug or Release

## Deployment Steps

### 1. Build the Solution
```powershell
# Option A: Build from Visual Studio
Build > Build Solution (Ctrl+Shift+B)

# Option B: Build from command line
msbuild ISRA_PS_AddOns.sln /p:Configuration=Release /p:Platform=AnyCPU
```

### 2. Locate Build Outputs

#### Debug Build (Default)
- **TempCompAddon.dll**: Auto-deployed to `C:\Program Files\Tecnomatix_2206.0\eMPower\DotNetCommands\`
- **LedVisibilityAddon.dll**: Built to `LedVisibilityAddon\bin\Debug\`
- **ISRA.Core.dll**: Built to `ISRA.Core\bin\Debug\`
- **ISRA.Calculations.dll**: Built to `ISRA.Calculations\bin\Debug\`
- **ISRA.Components.dll**: Built to `ISRA.Components\bin\Debug\`

#### Release Build
- **TempCompAddon.dll**: Built to `TempCompAddon\bin\Release\`
- **LedVisibilityAddon.dll**: Built to `LedVisibilityAddon\bin\Release\`
- **ISRA.Core.dll**: Built to `ISRA.Core\bin\Release\`
- **ISRA.Calculations.dll**: Built to `ISRA.Calculations\bin\Release\`
- **ISRA.Components.dll**: Built to `ISRA.Components\bin\Release\`

### 3. Copy DLLs to Process Simulate

**Target Directory**: `C:\Program Files\Tecnomatix_2206.0\eMPower\DotNetCommands\`

**Required Files**:
1. `TempCompAddon.dll` (if updated)
2. `LedVisibilityAddon.dll` (if updated)
3. `ISRA.Core.dll` (new dependency)
4. `ISRA.Calculations.dll` (if updated)
5. `ISRA.Components.dll` (if updated)
6. Supporting XML/bitmap files (if updated)

```powershell
# Copy all required DLLs (Release build example)
Copy-Item "TempCompAddon\bin\Release\TempCompAddon.dll" "C:\Program Files\Tecnomatix_2206.0\eMPower\DotNetCommands\" -Force
Copy-Item "LedVisibilityAddon\bin\Release\LedVisibilityAddon.dll" "C:\Program Files\Tecnomatix_2206.0\eMPower\DotNetCommands\" -Force
Copy-Item "ISRA.Core\bin\Release\ISRA.Core.dll" "C:\Program Files\Tecnomatix_2206.0\eMPower\DotNetCommands\" -Force
Copy-Item "ISRA.Calculations\bin\Release\ISRA.Calculations.dll" "C:\Program Files\Tecnomatix_2206.0\eMPower\DotNetCommands\" -Force
Copy-Item "ISRA.Components\bin\Release\ISRA.Components.dll" "C:\Program Files\Tecnomatix_2206.0\eMPower\DotNetCommands\" -Force
```

### 4. Register AddOns with Process Simulate

**Command Registration Tool**: `C:\Program Files\Tecnomatix_2206.0\eMPower\CommandReg.exe`

```powershell
# Navigate to Process Simulate directory
cd "C:\Program Files\Tecnomatix_2206.0\eMPower\"

# Register TempCompAddon
.\CommandReg.exe /register /path:"DotNetCommands\TempCompAddon.dll"

# Register LedVisibilityAddon
.\CommandReg.exe /register /path:"DotNetCommands\LedVisibilityAddon.dll"
```

### 5. Verify Deployment

1. Launch Process Simulate 2206
2. Check that both addon buttons appear in the ribbon
3. Click each addon button to ensure the forms open without errors
4. Test basic functionality:
   - **TempCompAddon**: Select robot, pick paths, run analysis
   - **LedVisibilityAddon**: Select tracker/stars, run visibility check

## Troubleshooting

### Issue: "Could not load file or assembly 'ISRA.Core'"
**Solution**: Ensure `ISRA.Core.dll` is copied to the `DotNetCommands` folder.

### Issue: Addon button doesn't appear
**Solution**: 
1. Check that the DLL is in `DotNetCommands`
2. Re-register using `CommandReg.exe`
3. Restart Process Simulate

### Issue: "Method not found" or "Type not found" errors
**Solution**: 
1. Ensure all ISRA library DLLs are up to date
2. Rebuild the entire solution
3. Copy all DLLs again
4. Re-register addons

### Issue: Old version behavior persists
**Solution**:
1. Close Process Simulate completely
2. Delete old addon DLLs from `DotNetCommands`
3. Copy new DLLs
4. Re-register
5. Restart Process Simulate

## Architecture Changes (OOP Refactoring)

### New Dependencies
The refactored addons now depend on:
- **ISRA.Core**: Shared infrastructure (validators, analyzers, presenters, UI base classes)
- **ISRA.Calculations**: Domain logic and services
- **ISRA.Components**: Component interfaces and data

### Benefits
- **MVP Pattern**: Clear separation between UI (View) and business logic (Presenter)
- **Testability**: Presenters and services can be unit tested
- **Maintainability**: Single responsibility principle throughout
- **Extensibility**: Easy to add new validators, robot types, or analysis criteria

### Backward Compatibility
- All original functionality is preserved
- No changes to user workflows
- Legacy static methods remain available via compatibility wrappers

## Build Verification

```powershell
# Check DLL timestamps
Get-ChildItem "C:\Program Files\Tecnomatix_2206.0\eMPower\DotNetCommands\" -Filter "*.dll" | 
	Where-Object { $_.Name -match "TempComp|LedVisibility|ISRA" } | 
	Select-Object Name, LastWriteTime, Length | 
	Format-Table -AutoSize
```

Expected output should show recent timestamps for all ISRA DLLs after deployment.

## Notes
- **TempCompAddon** has auto-deployment configured in Debug mode (OutputPath points directly to DotNetCommands)
- **LedVisibilityAddon** requires manual copy from bin\Release or bin\Debug
- Always re-register after updating DLLs
- Process Simulate must be closed during DLL updates
