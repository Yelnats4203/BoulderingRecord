$json = [Console]::In.ReadToEnd() | ConvertFrom-Json
$line = "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] $($json.tool_input.command)"
Add-Content -Path "D:\BoulderingRecord\.claude\logs\bash-commands.log" -Value $line
