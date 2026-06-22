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
		TargetSubfolder = "DotNetCommands\TempComp"
	},
	@{
		Name = "LED Visibility Analyzer"
		DllName = "LedVisibilityAddon.dll"
		CommandName = "LedVisibilityAnalyzer"
		Description = "ISRA LED Visibility Analyzer"
		ImageName = "LedVisibilityIcon.png"
		TargetSubfolder = "DotNetCommands\Accusite"
	}
)

# Shared DLLs that both addons need (copied to both folders)
$SharedDlls = @(
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
		"C:\Program Files\Tecnomatix_2502.0\eMPower",
		"C:\Program Files\Tecnomatix_2408.0\eMPower",
		"C:\Program Files\Tecnomatix_2206.0\eMPower",
		"C:\Program Files\Siemens\Tecnomatix_2502.0\eMPower",
		"C:\Program Files\Siemens\Tecnomatix_2408.0\eMPower",
		"C:\Program Files\Siemens\Tecnomatix_2206.0\eMPower"
	)

	foreach ($path in $CommonPaths) {
		$binPath = Join-Path $path "bin\TxApplication.exe"
		if (Test-Path $binPath) {
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
	param(
		[string]$SourcePath,
		[string]$EmpowerPath
	)

	Write-ColorText "`nCopying add-on files..." Yellow

	if (-not (Test-Path $SourcePath)) {
		throw "Source path not found: $SourcePath"
	}

	# Copy each add-on to its specific subfolder
	foreach ($addon in $AddOns) {
		$targetFolder = Join-Path $EmpowerPath $addon.TargetSubfolder

		# Create target folder if it doesn't exist
		if (-not (Test-Path $targetFolder)) {
			New-Item -ItemType Directory -Path $targetFolder -Force | Out-Null
			Write-ColorText "  Created folder: $($addon.TargetSubfolder)" Cyan
		}

		# Copy addon DLL
		$addonSource = Join-Path $SourcePath $addon.DllName
		$addonDest = Join-Path $targetFolder $addon.DllName

		if (-not (Test-Path $addonSource)) {
			Write-ColorText "  ✗ Missing: $($addon.DllName)" Red
			throw "Required file not found: $addonSource"
		}

		Copy-Item -Path $addonSource -Destination $addonDest -Force
		Write-ColorText "  ✓ Copied: $($addon.DllName) → $($addon.TargetSubfolder)" Green

		# Copy shared DLLs to this addon's folder
		foreach ($sharedDll in $SharedDlls) {
			$sharedSource = Join-Path $SourcePath $sharedDll
			$sharedDest = Join-Path $targetFolder $sharedDll

			if (Test-Path $sharedSource) {
				Copy-Item -Path $sharedSource -Destination $sharedDest -Force
				Write-ColorText "  ✓ Copied: $sharedDll → $($addon.TargetSubfolder)" Green
			} else {
				Write-ColorText "  ⚠ Missing shared DLL: $sharedDll" Yellow
			}
		}

		# Copy icon if exists
		if ($addon.ImageName) {
			$iconSource = Join-Path $SourcePath $addon.ImageName
			$iconDest = Join-Path $targetFolder $addon.ImageName

			if (Test-Path $iconSource) {
				Copy-Item -Path $iconSource -Destination $iconDest -Force
				Write-ColorText "  ✓ Copied: $($addon.ImageName) → $($addon.TargetSubfolder)" Green
			}
		}

		Write-Host ""
	}
}

function Register-Addon {
	param(
		[string]$EmpowerPath,
		[hashtable]$Addon
	)

	# commandreg.exe is in the eMPower folder
	$commandRegPath = Join-Path $EmpowerPath "commandreg.exe"

	if (-not (Test-Path $commandRegPath)) {
		Write-ColorText "  ⚠ commandreg.exe not found at: $commandRegPath" Yellow
		Write-ColorText "  ℹ Manual registration required - see post-install instructions" Cyan
		return $false
	}

	$dllPath = Join-Path $EmpowerPath "$($Addon.TargetSubfolder)\$($Addon.DllName)"

	Write-ColorText "`nRegistering: $($Addon.Name)..." Yellow
	Write-ColorText "  DLL: $dllPath" Gray
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
		$imagePath = Join-Path $EmpowerPath "$($Addon.TargetSubfolder)\$($Addon.ImageName)"
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
		[string]$EmpowerPath,
		[hashtable]$Addon
	)

	# commandreg.exe is in the eMPower folder
	$commandRegPath = Join-Path $EmpowerPath "commandreg.exe"

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
	param([string]$EmpowerPath)

	Write-ColorText "`nRemoving add-on files..." Yellow

	foreach ($addon in $AddOns) {
		$targetFolder = Join-Path $EmpowerPath $addon.TargetSubfolder

		if (Test-Path $targetFolder) {
			# Remove all files from addon folder
			Remove-Item -Path "$targetFolder\*" -Force -Recurse -ErrorAction SilentlyContinue
			Write-ColorText "  ✓ Removed files from: $($addon.TargetSubfolder)" Green
		}
	}
}
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
		Write-ColorText '  .\Install-ISRA-Addons.ps1 -TecnomatixPath "C:\Program Files\Tecnomatix_XXXX\eMPower"' White
		Write-ColorText '  (Replace XXXX with 2206.0, 2408.0, or 2502.0)' Gray
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

Write-ColorText "eMPower Path: $TecnomatixPath" Cyan

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
		Unregister-Addon -EmpowerPath $TecnomatixPath -Addon $addon
	}

	Remove-AddonFiles -EmpowerPath $TecnomatixPath

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
	# Copy files to addon-specific subfolders
	Copy-AddonFiles -SourcePath $DllSourcePath -EmpowerPath $TecnomatixPath

	# Attempt to register add-ons (may require manual steps)
	$registrationResults = @()
	foreach ($addon in $AddOns) {
		$result = Register-Addon -EmpowerPath $TecnomatixPath -Addon $addon
		$registrationResults += @{ Name = $addon.Name; Success = $result }
	}

	Write-Header "Installation Complete!"

	Write-ColorText "✓ DLL files copied successfully to addon subfolders" Green
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
		Write-ColorText "  $TecnomatixPath" Gray
		Write-Host ""
		Write-ColorText "Step 3: Right-click commandreg.exe → Run as Administrator" White
		Write-Host ""
		Write-ColorText "Step 4: For each add-on:" White
		foreach ($addon in $AddOns) {
			$addonPath = Join-Path $TecnomatixPath "$($addon.TargetSubfolder)\$($addon.DllName)"
			Write-ColorText "  • $($addon.Name):" Cyan
			Write-ColorText "    Browse to: $addonPath" Gray
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

	Write-ColorText "Installation locations:" Gray
	foreach ($addon in $AddOns) {
		$addonPath = Join-Path $TecnomatixPath $addon.TargetSubfolder
		Write-ColorText "  • $($addon.Name): $addonPath" DarkGray
	}

} catch {
	Write-ColorText "`nERROR: Installation failed" Red
	Write-ColorText $_.Exception.Message Red
	Read-Host "`nPress Enter to exit"
	exit 1
}

Read-Host "`nPress Enter to exit"
