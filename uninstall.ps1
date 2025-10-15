$ErrorActionPreference = "Stop"

Write-Host "Uninstalling w2ds..." -ForegroundColor Cyan

$targetDir = "$env:LOCALAPPDATA\w2ds"

if (Test-Path $targetDir) {
    Remove-Item -Path $targetDir -Recurse -Force
    Write-Host "Removed: $targetDir" -ForegroundColor Green
}

$userPath = [Environment]::GetEnvironmentVariable("Path", [EnvironmentVariableTarget]::User)
if ($userPath -like "*$targetDir*") {
    $newPath = $userPath -replace [regex]::Escape(";$targetDir"), ""
    [Environment]::SetEnvironmentVariable("Path", $newPath, [EnvironmentVariableTarget]::User)
    Write-Host "Removed from PATH" -ForegroundColor Green
}

Write-Host ""
Write-Host "Uninstall complete" -ForegroundColor Green
