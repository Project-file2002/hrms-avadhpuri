param(
  [string]$Url = "http://localhost:5096"
)

$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$logDir = Join-Path $PSScriptRoot "logs"
New-Item -ItemType Directory -Path $logDir -Force | Out-Null

$logFile   = Join-Path $logDir "backend-$timestamp.log"
$errorFile = Join-Path $logDir "backend-$timestamp.err"

Write-Host "Starting HRMS API backend..." -ForegroundColor Green
Write-Host "  URL:       $Url"
Write-Host "  Log:       $logFile"
Write-Host "  Error log: $errorFile"

$env:ASPNETCORE_URLS = $Url

dotnet run --project "$PSScriptRoot/HRMS.API.csproj" 2>&1 | ForEach-Object {
  $line = "$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss.fff') | $_"
  if ($_.Exception -or $_.ToString() -match 'fail|error|Exception|Error') {
    $line | Out-File -FilePath $errorFile -Append -Encoding utf8
  }
  $line | Out-File -FilePath $logFile -Append -Encoding utf8
  Write-Host $_
}
