# CLAUDE.md

本檔案為 Claude Code（claude.ai/code）在此儲存庫中工作時的指引。

## 專案概覽

這是一個以 **.NET 10**（`net10.0`）為目標、採用 controller-based（`Microsoft.NET.Sdk.Web`）架構的 ASP.NET Core Web API 專案。單一專案的方案結構：

- 方案檔：`BoulderingRecord.slnx`
- 專案：`BoulderingRecordAPI/BoulderingRecordAPI.csproj`

目前專案仍處於早期鷹架階段——`WeatherForecastController.cs` 與 `WeatherForecast.cs` 是 `dotnet new webapi` 範本留下的預設檔案。等實際的攀岩領域功能（攀岩路線、攀爬紀錄、場次等）開發出來後，應移除這些範本檔案。

## 常用指令

- 建置：`dotnet build`
- 執行：`dotnet run --project BoulderingRecordAPI`
- 測試：目前尚未建立測試專案。之後新增測試專案時請使用 **xUnit**（已定案）——不要改用 NUnit 或 MSTest。

## 資料持久化架構

資料存取層規劃為**抽象化的 repository 層**（以介面為主，不直接綁定特定 EF Core provider），讓同一套程式碼可以搭配兩種不同的 EF Core provider 運作：

- **SQLite** — 測試環境使用
- **MSSQL（SQL Server）** — 正式部署環境使用

新增實體（entity）或資料存取邏輯時，請將 provider 相關設定隔離開來（例如放在 `DbContext` 設定或是 provider 切換邏輯中），確保切換 SQLite ↔ MSSQL 時不需要更動 repository 或商業邏輯程式碼。
