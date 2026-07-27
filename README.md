# BoulderingRecord

攀岩紀錄管理系統，以 **.NET 10**（`net10.0`）為目標建置的方案。

## 方案結構

- 方案檔：`BoulderingRecord.slnx`
- 子專案：
  - [`BoulderingRecordAPI/`](BoulderingRecordAPI/README.md) — Web API 主專案
  - [`BoulderingRecordAPI.Tests/`](BoulderingRecordAPI.Tests/README.md) — xUnit 測試專案

各子專案的技術細節、常用指令與資料持久化架構等說明，請參閱其專案目錄下的 README.md。

## 常用指令

```bash
# 建置整個方案
dotnet build

# 測試
dotnet test
```

## 程式碼慣例

- **不使用 `var`**：區域變數一律宣告明確型別，禁止使用 `var`（包含 `out var`、tuple 解構等寫法），以利閱讀時清楚掌握型別資訊。此慣例適用於方案內所有子專案。

## Claude Code Hooks

本專案在 `.claude/settings.json` 中設定了以下 hooks：

- **UserPromptSubmit → 記錄使用者輸入**（`.claude/hooks/log-user-prompt.ps1`）
  每次使用者送出提示（prompt）時觸發，將時間戳記與提示內容附加寫入 `.claude/logs/user-prompts.log`。

- **PostToolUse（`Write`、`Edit`）→ 自動格式化 C# 檔案**（`.claude/hooks/format-cs-file.ps1`）
  每次 Claude 透過 `Write` 或 `Edit` 工具異動檔案後觸發；若被異動的檔案副檔名為 `.cs`，會對該檔案執行 `dotnet format BoulderingRecordAPI.csproj --include <relativePath>`，自動套用專案的格式化規則。
