# Script de test - Afișează informațiile serverului
Write-Host "=== Server Information ===" -ForegroundColor Green
Write-Host "Computer Name: $env:COMPUTERNAME"
Write-Host "Username: $env:USERNAME"
Write-Host "Current Date/Time: $(Get-Date)"
Write-Host "PowerShell Version: $($PSVersionTable.PSVersion)"
Write-Host ""
Write-Host "=== Disk Information ===" -ForegroundColor Green
Get-Volume | Select-Object DriveLetter, HealthStatus, @{Name="SizeGB"; Expression={[Math]::Round($_.Size/1GB, 2)}}, @{Name="FreeSpaceGB"; Expression={[Math]::Round($_.SizeRemaining/1GB, 2)}} | Format-Table -AutoSize
