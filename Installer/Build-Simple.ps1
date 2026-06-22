# Build and package ISRA Add-ons for distribution
Write-Host "Building package..." -ForegroundColor Cyan

$SolutionDir = Get-Location
$PackageDir = Join-Path $SolutionDir "Release_Package"
$BinariesDir = Join-Path $PackageDir "Binaries"

# Clean and create package directory
if (Test-Path $PackageDir) {
    Remove-Item -Path $PackageDir -Recurse -Force
}
New-Item -ItemType Directory -Path $BinariesDir -Force | Out-Null

Write-Host "Copying DLLs..." -ForegroundColor Yellow

# Copy addon DLLs
Copy-Item "TempCompAddon\bin\Release\*.dll" -Destination $BinariesDir -Force
Copy-Item "LedVisibilityAddon\bin\Release\*.dll" -Destination $BinariesDir -Force

# Copy shared library DLLs
Copy-Item "ISRA.Core\bin\Release\*.dll" -Destination $BinariesDir -Force
Copy-Item "ISRA.Components\bin\Release\*.dll" -Destination $BinariesDir -Force
Copy-Item "ISRA.Calculations\bin\Release\*.dll" -Destination $BinariesDir -Force

# Copy EPPlus
Copy-Item "packages\EPPlusFree.4.5.3.8\lib\net40\EPPlusFree.dll" -Destination $BinariesDir -Force -ErrorAction SilentlyContinue

Write-Host "Copying icon files..." -ForegroundColor Yellow

# Copy icon files
Copy-Item "TempCompAddon\temp_comp_add_on_16x16.bmp" -Destination $BinariesDir -Force
Copy-Item "TempCompAddon\temp_comp_add_on_32x32.png" -Destination $BinariesDir -Force
Copy-Item "LedVisibilityAddon\star_515_0139_icon_16x16.bmp" -Destination $BinariesDir -Force
Copy-Item "LedVisibilityAddon\star_515_0139_icon_32x32.png" -Destination $BinariesDir -Force

Write-Host "Copying installer files..." -ForegroundColor Yellow

# Copy installer scripts and documentation
Copy-Item "Installer\Install-ISRA-Addons.ps1" -Destination $PackageDir -Force
Copy-Item "Installer\INSTALL.bat" -Destination $PackageDir -Force
Copy-Item "Installer\UNINSTALL.bat" -Destination $PackageDir -Force
Copy-Item "Installer\INSTALLATION_GUIDE.md" -Destination $PackageDir -Force
Copy-Item "Installer\README.md" -Destination $PackageDir -Force

Write-Host "Done! Package created at: $PackageDir" -ForegroundColor Green
