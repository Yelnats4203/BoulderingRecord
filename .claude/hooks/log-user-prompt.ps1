$json = [Console]::In.ReadToEnd() | ConvertFrom-Json
$line = "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] $($json.prompt)"
Add-Content -Path "D:\BoulderingRecord\.claude\logs\user-prompts.log" -Value $line
