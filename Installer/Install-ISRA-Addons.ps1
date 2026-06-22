# ISRA Process Simulate Add-Ons Installer
# Version: 1.0
# Installs TempComp Validator and LED Visibility Analyzer

param(
	[string]$TecnomatixPath = "",
	[switch]$UninstallOnly = $false
)

$ErrorActionPreference = "Stop"

# ============================================================================
# Configuration
# ============================================================================

$AddOns = @(
	@{
		Name = "TempComp Validator"
		DllName = "TempCompAddon.dll"
		CommandName = "TempCompAnalyzer"
		Description = "ISRA Temp Comp Validator"
		ImageName = "TempCompIcon.png"
	},
	@{
		Name = "LED Visibility Analyzer"
		DllName = "LedVisibilityAddon.dll"
		CommandName = "LedVisibilityAnalyzer"
		Description = "ISRA LED Visibility Analyzer"
		ImageName = "LedVisibilityIcon.png"
	}
)

$RequiredDlls = @(
	"TempCompAddon.dll",
	"LedVisibilityAddon.dll",
	"ISRA.Core.dll",
	"ISRA.Components.dll",
	"ISRA.Calculations.dll",
	"EPPlusFree.dll"
)

# ============================================================================
# Functions
# ============================================================================

function Write-ColorText {
	param([string]$Text, [ConsoleColor]$Color = [ConsoleColor]::White)
	Write-Host $Text -ForegroundColor $Color
}

function Write-Header {
	param([string]$Text)
	Write-Host ""
	Write-ColorText "================================================================" Cyan
	Write-ColorText " $Text" Cyan
	Write-ColorText "================================================================" Cyan
	Write-Host ""
}

function Find-TecnomatixPath {
	Write-ColorText "Searching for Tecnomatix installation..." Yellow

	$CommonPaths = @(
		"C:\Program Files\Tecnomatix\ProcessSimulate_2502\bin",
		"C:\Program Files\Tecnomatix\ProcessSimulate_2408\bin",
		"C:\Program Files\Tecnomatix\ProcessSimulate_2206\bin",
		"C:\Program Files\Siemens\Tecnomatix\ProcessSimulate_2502\bin",
		"C:\Program Files\Siemens\Tecnomatix\ProcessSimulate_2408\bin",
		"C:\Program Files\Siemens\Tecnomatix\ProcessSimulate_2206\bin"
	)

	foreach ($path in $CommonPaths) {
		if (Test-Path "$path\TxApplication.exe") {
			Write-ColorText "✓ Found: $path" Green
			return $path
		}
	}

	return $null
}

function Test-AdminPrivileges {
	$currentUser = [Security.Principal.WindowsIdentity]::GetCurrent()
	$principal = New-Object Security.Principal.WindowsPrincipal($currentUser)
	return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

function Copy-AddonFiles {
	param([string]$SourcePath, [string]$DestPath)

	Write-ColorText "`nCopying add-on files..." Yellow

	if (-not (Test-Path $SourcePath)) {
		throw "Source path not found: $SourcePath"
	}

	foreach ($dll in $RequiredDlls) {
		$source = Join-Path $SourcePath $dll
		$dest = Join-Path $DestPath $dll

		if (-not (Test-Path $source)) {
			Write-ColorText "✗ Missing: $dll" Red
			throw "Required file not found: $source"
		}

		Copy-Item -Path $source -Destination $dest -Force
		Write-ColorText "  ✓ Copied: $dll" Green
	}

	# Copy image files if they exist
	$imageFiles = Get-ChildItem -Path $SourcePath -Filter "*.png" -ErrorAction SilentlyContinue
	foreach ($img in $imageFiles) {
		Copy-Item -Path $img.FullName -Destination $DestPath -Force
		Write-ColorText "  ✓ Copied: $($img.Name)" Green
	}
}

function Register-Addon {
	param(
		[string]$TecnomatixBinPath,
		[hashtable]$Addon
	)

	# CommandReg.exe is in the parent eMPower folder, not in bin
	$empowerPath = Split-Path -Parent $TecnomatixBinPath
	$commandRegPath = Join-Path $empowerPath "commandreg.exe"

	if (-not (Test-Path $commandRegPath)) {
		Write-ColorText "  ⚠ commandreg.exe not found at: $commandRegPath" Yellow
		Write-ColorText "  ℹ Manual registration required - see post-install instructions" Cyan
		return $false
	}

	$dllPath = Join-Path $TecnomatixBinPath $Addon.DllName

	Write-ColorText "`nRegistering: $($Addon.Name)..." Yellow
	Write-ColorText "  Note: If registration doesn't complete automatically," Gray
	Write-ColorText "        you may need to register manually via commandreg.exe GUI" Gray

	# Note: commandreg.exe is typically a GUI tool, not a command-line tool
	# Attempting automated registration, but may require manual steps
	$arguments = @(
		"/register",
		"`"$dllPath`"",
		$Addon.CommandName,
		"`"$($Addon.Description)`""
	)

	if ($Addon.ImageName) {
		$imagePath = Join-Path $TecnomatixBinPath $Addon.ImageName
		if (Test-Path $imagePath) {
			$arguments += "`"$imagePath`""
		}
	}

	try {
		$process = Start-Process -FilePath $commandRegPath -ArgumentList $arguments -Wait -PassThru -NoNewWindow -ErrorAction SilentlyContinue

		if ($process.ExitCode -eq 0) {
			Write-ColorText "  ✓ Registered successfully" Green
			return $true
		} else {
			Write-ColorText "  ⚠ Registration may require manual steps (exit code: $($process.ExitCode))" Yellow
			return $false
		}
	} catch {
		Write-ColorText "  ⚠ Automated registration not available" Yellow
		return $false
	}
}

function Unregister-Addon {
	param(
		[string]$TecnomatixBinPath,
		[hashtable]$Addon
	)

	# CommandReg.exe is in the parent eMPower folder
	$empowerPath = Split-Path -Parent $TecnomatixBinPath
	$commandRegPath = Join-Path $empowerPath "commandreg.exe"

	if (-not (Test-Path $commandRegPath)) {
		Write-ColorText "  ⚠ commandreg.exe not found, skipping unregistration" Yellow
		return
	}

	Write-ColorText "`nUnregistering: $($Addon.Name)..." Yellow

	$arguments = @(
		"/unregister",
		$Addon.CommandName
	)

	try {
		$process = Start-Process -FilePath $commandRegPath -ArgumentList $arguments -Wait -PassThru -NoNewWindow -ErrorAction SilentlyContinue

		if ($process.ExitCode -eq 0) {
			Write-ColorText "  ✓ Unregistered successfully" Green
		} else {
			Write-ColorText "  ⚠ Unregistration completed with code: $($process.ExitCode)" Yellow
		}
	} catch {
		Write-ColorText "  ⚠ Manual unregistration may be required" Yellow
	}
}

function Remove-AddonFiles {
	param([string]$TecnomatixBinPath)

	Write-ColorText "`nRemoving add-on files..." Yellow

	foreach ($dll in $RequiredDlls) {
		$filePath = Join-Path $TecnomatixBinPath $dll
		if (Test-Path $filePath) {
			Remove-Item -Path $filePath -Force
			Write-ColorText "  ✓ Removed: $dll" Green
		}
	}

	# Remove image files
	foreach ($addon in $AddOns) {
		if ($addon.ImageName) {
			$imgPath = Join-Path $TecnomatixBinPath $addon.ImageName
			if (Test-Path $imgPath) {
				Remove-Item -Path $imgPath -Force
				Write-ColorText "  ✓ Removed: $($addon.ImageName)" Green
			}
		}
	}
}

# ============================================================================
# Main Installation Logic
# ============================================================================

Write-Header "ISRA Process Simulate Add-Ons Installer"

# Check admin privileges
if (-not (Test-AdminPrivileges)) {
	Write-ColorText "ERROR: This installer requires Administrator privileges." Red
	Write-ColorText "Please right-click and select 'Run as Administrator'" Yellow
	Read-Host "`nPress Enter to exit"
	exit 1
}

# Find or validate Tecnomatix path
if (-not $TecnomatixPath) {
	$TecnomatixPath = Find-TecnomatixPath

	if (-not $TecnomatixPath) {
		Write-ColorText "ERROR: Tecnomatix installation not found automatically." Red
		Write-ColorText "Please run the installer with -TecnomatixPath parameter:" Yellow
		Write-ColorText '  .\Install-ISRA-Addons.ps1 -TecnomatixPath "C:\Program Files\Tecnomatix\ProcessSimulate_XXXX\bin"' White
		Read-Host "`nPress Enter to exit"
		exit 1
	}
} else {
	if (-not (Test-Path $TecnomatixPath)) {
		Write-ColorText "ERROR: Specified Tecnomatix path not found: $TecnomatixPath" Red
		Read-Host "`nPress Enter to exit"
		exit 1
	}
}

Write-ColorText "Tecnomatix Path: $TecnomatixPath" Cyan

# Get current script directory (where DLLs should be)
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$DllSourcePath = Join-Path $ScriptDir "Binaries"

# Check if running from packaged installer
if (-not (Test-Path $DllSourcePath)) {
	$DllSourcePath = $ScriptDir
}

# ============================================================================
# Uninstall
# ============================================================================

if ($UninstallOnly) {
	Write-Header "Uninstalling ISRA Add-Ons"

	foreach ($addon in $AddOns) {
		Unregister-Addon -TecnomatixBinPath $TecnomatixPath -Addon $addon
	}

	Remove-AddonFiles -TecnomatixBinPath $TecnomatixPath

	Write-Header "Uninstallation Complete"
	Write-ColorText "The ISRA add-ons have been removed from Process Simulate." Green
	Read-Host "`nPress Enter to exit"
	exit 0
}

# ============================================================================
# Install
# ============================================================================

Write-Header "Installing ISRA Add-Ons"

try {
	# Copy files
	Copy-AddonFiles -SourcePath $DllSourcePath -DestPath $TecnomatixPath

	# Attempt to register add-ons (may require manual steps)
	$registrationResults = @()
	foreach ($addon in $AddOns) {
		$result = Register-Addon -TecnomatixBinPath $TecnomatixPath -Addon $addon
		$registrationResults += @{ Name = $addon.Name; Success = $result }
	}

	Write-Header "Installation Complete!"

	Write-ColorText "✓ DLL files copied successfully" Green
	Write-Host ""

	# Check if manual registration is needed
	$manualRegNeeded = $registrationResults | Where-Object { -not $_.Success }

	if ($manualRegNeeded) {
		Write-ColorText "⚠ Manual Registration Required" Yellow
		Write-Host ""
		Write-ColorText "The DLL files are installed, but you need to register the add-ons manually:" Cyan
		Write-Host ""
		Write-ColorText "Step 1: Ensure Process Simulate is CLOSED" White
		Write-Host ""
		Write-ColorText "Step 2: Navigate to eMPower folder:" White
		$empowerPath = Split-Path -Parent $TecnomatixPath
		Write-ColorText "  $empowerPath" Gray
		Write-Host ""
		Write-ColorText "Step 3: Right-click commandreg.exe → Run as Administrator" White
		Write-Host ""
		Write-ColorText "Step 4: For each add-on:" White
		foreach ($addon in $AddOns) {
			Write-ColorText "  • Browse to: $TecnomatixPath\$($addon.DllName)" Gray
			Write-ColorText "    Click 'Create File' to generate .xml registration" Gray
			Write-ColorText "    (For updates, select existing .xml from dropdown)" Gray
			Write-ColorText "    Click 'Register'" Gray
			Write-Host ""
		}
		Write-ColorText "Step 5: Open Process Simulate" White
		Write-ColorText "Step 6: Add buttons to toolbar (Tools → Customize...)" White
		Write-Host ""
	} else {
		Write-ColorText "✓ Add-ons registered successfully" Green
		Write-Host ""
		Write-ColorText "Next steps:" Cyan
		Write-ColorText "1. Open Siemens Process Simulate" White
		Write-ColorText "2. Go to: Tools → Customize..." White
		Write-ColorText "3. Find 'ISRA Temp Comp Validator' and 'ISRA LED Visibility Analyzer'" White
		Write-ColorText "4. Drag them to your toolbar" White
		Write-Host ""
	}

	Write-ColorText "Installation location: $TecnomatixPath" Gray

} catch {
	Write-ColorText "`nERROR: Installation failed" Red
	Write-ColorText $_.Exception.Message Red
	Read-Host "`nPress Enter to exit"
	exit 1
}

Read-Host "`nPress Enter to exit"
