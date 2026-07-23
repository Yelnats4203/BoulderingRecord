$json = [Console]::In.ReadToEnd() | ConvertFrom-Json
$prompt = $json.prompt

$logFile = "D:\BoulderingRecord\.claude\logs\user-prompts-$(Get-Date -Format 'yyyy-MM-dd').log"
$timestamp = Get-Date -Format 'yyyy-MM-dd HH:mm:ss'

# 系統自動注入的背景 task 完成通知，不算使用者輸入，略過不記錄
if ($prompt -match '^\s*<task-notification>') {
    exit 0
}

# local slash command（例如 /plan、/clear）：只記錄指令名稱與參數，不記錄整段包裝內容
if ($prompt -match '^\s*<local-command-caveat>') {
    if ($prompt -match '<command-name>(.*?)</command-name>') {
        $commandName = $Matches[1].Trim()
        $argsMatch = [regex]::Match($prompt, '<command-args>(.*?)</command-args>')
        $commandArgs = if ($argsMatch.Success) { $argsMatch.Groups[1].Value.Trim() } else { '' }
        $line = if ($commandArgs) { "[$timestamp] [指令] $commandName $commandArgs" } else { "[$timestamp] [指令] $commandName" }
        Add-Content -Path $logFile -Value $line -Encoding utf8
    }
    exit 0
}

$line = "[$timestamp] $prompt"
Add-Content -Path $logFile -Value $line -Encoding utf8
