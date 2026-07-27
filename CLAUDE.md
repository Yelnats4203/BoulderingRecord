# CLAUDE.md

本檔案為 Claude Code（claude.ai/code）在此儲存庫中工作時的指引。

## 專案概覽

BoulderingRecord 是一個攀岩紀錄管理系統，以 **.NET 10**（`net10.0`）為目標建置。方案結構：

- 方案檔：`BoulderingRecord.slnx`
- 子專案：
  - `BoulderingRecordAPI/` — Web API 主專案
  - `BoulderingRecordAPI.Tests/` — xUnit 測試專案

各子專案的技術細節、常用指令與資料持久化架構等說明，請參閱其專案目錄下的 README.md；本檔案僅記錄跨子專案共用的方案層級指引。

## 常用指令

- 建置整個方案：`dotnet build`
- 執行測試：`dotnet test`
- 測試框架統一採用 **xUnit**（已定案）——不要改用 NUnit 或 MSTest。

## 程式碼慣例

- **不使用 `var`**：區域變數一律宣告明確型別，禁止使用 `var`（包含 `out var`、tuple 解構等寫法），以利閱讀時清楚掌握型別資訊。此慣例適用於方案內所有子專案。
