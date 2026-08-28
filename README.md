# BoulderingRecord

攀岩紀錄管理系統，後端以 **.NET 10**（`net10.0`）為目標建置，前端為 **Vue 3 + TypeScript + Vite** 專案。

## 專案結構

- 方案檔：`BoulderingRecord.slnx`
- 後端子專案：
  - [`BoulderingRecordAPI/`](BoulderingRecordAPI/README.md) — Web API 主專案
  - [`BoulderingRecordAPI.Tests/`](BoulderingRecordAPI.Tests/README.md) — xUnit 測試專案
- 前端專案：
  - [`frontend/`](frontend/README.md) — Vue 3 + TypeScript + Vite SPA

各子專案的技術細節、常用指令與資料持久化架構等說明，請參閱其專案目錄下的 README.md。

## 常用指令

```bash
# 建置整個後端方案
dotnet build

# 後端測試
dotnet test

# 前端開發伺服器
cd frontend && npm run dev

# 前端建置
cd frontend && npm run build
```

## 程式碼慣例

- **不使用 `var`**（後端）：C# 區域變數一律宣告明確型別，禁止使用 `var`（包含 `out var`、tuple 解構等寫法），以利閱讀時清楚掌握型別資訊。此慣例適用於方案內所有 .NET 子專案。

## 部署環境

- **後端 API**：Azure App Service（Linux，`DOTNETCORE|10.0`，Plan SKU F1 Free）
- **資料庫**：Azure SQL Server（serverless，會自動暫停/喚醒，首次連線可能因喚醒中而暫時失敗，重試即可）
- **前端**：Vercel（Vue 3 SPA，`vercel.json` 設定所有路徑 rewrite 至 `index.html`）
- **影片儲存**：Cloudinary（雲端物件儲存 + CDN），儲存路徑依環境（開發／正式）區分子資料夾
- 各項服務所需的連線字串、API Keys、JWT Key 等機密，皆透過各平台的環境設定（Azure App Service Application Settings、Vercel 環境變數）管理，不透過版控存放；本機測試用機密的存放位置與版控安全規範見 `CLAUDE.md`。

## Claude Code Hooks

本專案在 `.claude/settings.json` 中設定了以下 hooks：

- **UserPromptSubmit → 記錄使用者輸入**（`.claude/hooks/log-user-prompt.ps1`）
  每次使用者送出提示（prompt）時觸發，將時間戳記與提示內容附加寫入 `.claude/logs/user-prompts.log`。

- **PostToolUse（`Write`、`Edit`）→ 自動格式化 C# 檔案**（`.claude/hooks/format-cs-file.ps1`）
  每次 Claude 透過 `Write` 或 `Edit` 工具異動檔案後觸發；若被異動的檔案副檔名為 `.cs`，會對該檔案執行 `dotnet format BoulderingRecordAPI.csproj --include <relativePath>`，自動套用專案的格式化規則。
