$ErrorActionPreference = "Stop"

$repoUrl = "https://github.com/d4vidn/win2day-statistics/releases/latest/download/w2ds.exe"
$targetDir = "C:\Program Files\w2ds"
$exeName = "w2ds.exe"

if (!(Test-Path $targetDir)) { New-Item -ItemType Directory -Path $targetDir | Out-Null }

try {
    Invoke-WebRequest -Uri $repoUrl -OutFile "$targetDir\$exeName" -UseBasicParsing
} catch {
    Write-Host "Download failed"
    exit 1
}

$envPath = [Environment]::GetEnvironmentVariable("Path", [EnvironmentVariableTarget]::Machine)
if ($envPath -notmatch [Regex]::Escape($targetDir)) {
    try {
        [Environment]::SetEnvironmentVariable("Path", "$envPath;$targetDir", [EnvironmentVariableTarget]::Machine)
    } catch {
        Write-Host "Failed to update PATH"
        exit 1
    }
}

Write-Host "Installation successful"
exit 0

Read-Host "Press Enter to exit"
