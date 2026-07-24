# BoulderingRecordAPI

以 **.NET 10**（`net10.0`）為目標、採用 controller-based（`Microsoft.NET.Sdk.Web`）架構的 ASP.NET Core Web API 專案。

## 目前狀態

專案仍處於早期鷹架階段，尚未加入實際的攀岩領域功能（攀岩路線、攀爬紀錄、場次等）。

## 常用指令

```bash
# 建置
dotnet build

# 執行（於方案根目錄執行）
dotnet run --project BoulderingRecordAPI
```

測試：目前尚未建立測試專案。之後新增測試專案時採用 xUnit。

## 程式碼慣例

- **不使用 `var`**：區域變數一律宣告明確型別，禁止使用 `var`（包含 `out var`、tuple 解構等寫法），以利閱讀時清楚掌握型別資訊。

## 資料持久化架構

資料存取層規劃為抽象化的 repository 層（以介面為主，不直接綁定特定 EF Core provider），讓同一套程式碼可以搭配兩種不同的 EF Core provider 運作：

- **SQLite** — 測試環境使用
- **MSSQL（SQL Server）** — 正式部署環境使用

新增實體（entity）或資料存取邏輯時，provider 相關設定會隔離在 `DbContext` 設定或 provider 切換邏輯中，切換 SQLite ↔ MSSQL 不需更動 repository 或商業邏輯程式碼。
