$ErrorActionPreference = "Stop"

Write-Host "Installing w2ds..." -ForegroundColor Cyan

$repoUrl = "https://github.com/d4vidn/win2day-statistics/releases/download/v1.0.0/w2ds.exe"
$targetDir = "$env:LOCALAPPDATA\w2ds"
$exePath = "$targetDir\w2ds.exe"

if (!(Test-Path $targetDir)) {
    New-Item -ItemType Directory -Path $targetDir | Out-Null
    Write-Host "Created directory: $targetDir" -ForegroundColor Green
}

try {
    Write-Host "Downloading w2ds.exe..." -ForegroundColor Yellow
    Invoke-WebRequest -Uri $repoUrl -OutFile $exePath -UseBasicParsing
    Write-Host "Downloaded successfully" -ForegroundColor Green
} catch {
    Write-Host "ERROR: Download failed - $_" -ForegroundColor Red
    return
}

$userPath = [Environment]::GetEnvironmentVariable("Path", [EnvironmentVariableTarget]::User)
if ($userPath -notlike "*$targetDir*") {
    try {
        [Environment]::SetEnvironmentVariable("Path", "$userPath;$targetDir", [EnvironmentVariableTarget]::User)
        Write-Host "Added to PATH" -ForegroundColor Green
    } catch {
        Write-Host "ERROR: Failed to update PATH - $_" -ForegroundColor Red
        return
    }
}

Write-Host ""
Write-Host "Installation successful!" -ForegroundColor Green
Write-Host "Restart your terminal and run: w2ds --help" -ForegroundColor Cyan
