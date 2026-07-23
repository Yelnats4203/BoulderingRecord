# BoulderingRecord

以 **.NET 10**（`net10.0`）為目標、採用 controller-based（`Microsoft.NET.Sdk.Web`）架構的攀岩紀錄 ASP.NET Core Web API 專案。

## 方案結構

- 方案檔：`BoulderingRecord.slnx`
- 專案：
  - `BoulderingRecordAPI/` — Web API 主專案
  - `BoulderingRecordAPI.Tests/` — xUnit 測試專案

## 目前狀態

專案仍處於早期鷹架階段，`WeatherForecastController.cs` 與 `WeatherForecast.cs` 是 `dotnet new webapi` 範本留下的預設檔案，待實際攀岩領域功能（攀岩路線、攀爬紀錄、場次等）開發出來後應予以移除。

## 常用指令

```bash
# 建置
dotnet build

# 執行
dotnet run --project BoulderingRecordAPI

# 測試
dotnet test
```

## 資料持久化架構

資料存取層規劃為抽象化的 repository 層（以介面為主，不直接綁定特定 EF Core provider），讓同一套程式碼可以搭配兩種不同的 EF Core provider 運作：

- **SQLite** — 測試環境使用
- **MSSQL（SQL Server）** — 正式部署環境使用

新增實體（entity）或資料存取邏輯時，provider 相關設定會隔離在 `DbContext` 設定或 provider 切換邏輯中，切換 SQLite ↔ MSSQL 不需更動 repository 或商業邏輯程式碼。

## Claude Code Hooks

本專案在 `.claude/settings.json` 中設定了以下 hooks：

- **UserPromptSubmit → 記錄使用者輸入**（`.claude/hooks/log-user-prompt.ps1`）
  每次使用者送出提示（prompt）時觸發，將時間戳記與提示內容附加寫入 `.claude/logs/user-prompts.log`。

- **PostToolUse（`Write`、`Edit`）→ 自動格式化 C# 檔案**（`.claude/hooks/format-cs-file.ps1`）
  每次 Claude 透過 `Write` 或 `Edit` 工具異動檔案後觸發；若被異動的檔案副檔名為 `.cs`，會對該檔案執行 `dotnet format BoulderingRecordAPI.csproj --include <relativePath>`，自動套用專案的格式化規則。
