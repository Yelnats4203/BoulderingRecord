$json = [Console]::In.ReadToEnd() | ConvertFrom-Json
$prompt = $json.prompt

# 系統自動注入的訊息（背景 task 完成通知、local slash command 產生的內容）不算使用者親自輸入的 prompt，略過不記錄
if ($prompt -match '^\s*<(task-notification|local-command-caveat)>') {
    exit 0
}

$line = "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] $prompt"
$logFile = "D:\BoulderingRecord\.claude\logs\user-prompts-$(Get-Date -Format 'yyyy-MM-dd').log"
Add-Content -Path $logFile -Value $line -Encoding utf8
