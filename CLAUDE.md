# CLAUDE.md

本檔案為 Claude Code（claude.ai/code）在此儲存庫中工作時的指引。

## 專案概覽

BoulderingRecord 是一個攀岩紀錄管理系統，後端以 **.NET 10**（`net10.0`）為目標建置，前端為 **Vue 3 + TypeScript + Vite** 專案。方案結構：

- 方案檔：`BoulderingRecord.slnx`
- 後端子專案：
  - `BoulderingRecordAPI/` — Web API 主專案
  - `BoulderingRecordAPI.Tests/` — xUnit 測試專案
- 前端專案：
  - `frontend/` — Vue 3 + TypeScript + Vite SPA，部署於 Vercel

各子專案的技術細節、常用指令與資料持久化架構等說明，請參閱其專案目錄下的 README.md；本檔案僅記錄跨子專案共用的指引。

## 常用指令

- 建置整個後端方案：`dotnet build`
- 執行後端測試：`dotnet test`
- 測試框架統一採用 **xUnit**（已定案）——不要改用 NUnit 或 MSTest。
- 前端開發伺服器：`cd frontend && npm run dev`
- 前端建置（含型別檢查）：`cd frontend && npm run build`

## 程式碼慣例

- **不使用 `var`**（後端）：C# 區域變數一律宣告明確型別，禁止使用 `var`（包含 `out var`、tuple 解構等寫法），以利閱讀時清楚掌握型別資訊。此慣例適用於方案內所有 .NET 子專案，不適用於前端 TypeScript 程式碼。
- **資料庫欄位不使用 DB 端預設值，一律由程式端明確賦值**：`OnModelCreating` 設定 Entity 屬性時，不使用 `HasDefaultValue(...)`／`HasDefaultValueSql(...)`，所有欄位的值都必須在程式碼中明確指定（例如建立實體時於建構式或物件初始化設定），資料庫本身不負責產生任何預設值。尤其是 Guid 主鍵，務必明確設定 `.ValueGeneratedNever()`——若讓 SQL Server 對 Guid 主鍵套用預設的 `ValueGeneratedOnAdd()`（例如不小心留下 `DEFAULT NEWID()`），會導致 EF Core 在把新物件加進已追蹤的擁有者集合（如 `OwnsMany`）時，把「主鍵已有值」誤判為「資料庫裡已存在該筆紀錄」，因而產生 UPDATE 而非 INSERT 語句，導致 `DbUpdateConcurrencyException`。詳見 `BoulderingRecordAPI/Data/BoulderingRecordDbContext.cs` 中 `SessionGradeRecord.Id` 的設定方式。

## API 測試規範（必要步驟）

- 新增任何 API 端點（Controller Action）時，**必須**在 `BoulderingRecordAPI.Tests` 補上對應的測試案例，涵蓋正常路徑與常見錯誤路徑（如未登入、資源不存在、權限不符等）。
- 新增或修改 API 測試後，**必須**同步在 `BoulderingRecordAPI.Tests/README.md` 的「API 測試案例清單」補上或更新對應的測試案例敘述，保持文件與程式碼一致。

## 版控安全檢查（必要步驟）

本專案為**公開 Repo**，每次執行版控（`git add` / `git commit` 前）**必須**檢查是否有因測試需求而寫入的 API Keys、密鑰、Token 等機密資訊：

- 檢查設定檔（如 `appsettings.json`、`appsettings.Development.json`）、程式碼、測試專案中是否存在實際的 API Keys 或機密值。
- 若發現有寫入實際的 API Keys，**版控前必須將其清空為空字串**（如 `""`），不得將實際金鑰提交進儲存庫。
- 部署時的實際 API Keys 由使用者手動填入，不透過版控管理，因此送出前清空是安全且不影響部署流程的做法。

## 本機測試機密存放位置

- 方案根目錄的 `CloudinaryKeys.txt` 存放 Cloudinary 的 `API Key`、`API Secret` 等本機測試用機密，已列入 `.gitignore`，不會進版控。
- 需要在本機測試環境（如 `appsettings.Development.json` 或執行期環境變數）填入 Cloudinary 憑證、但目前設定值為空時，先查找此檔案取得對應的值再填入，不要要求使用者重新提供。
- 方案根目錄的 `TestAccount.txt` 存放前端測試用登入帳號密碼，已列入 `.gitignore`，不會進版控。日後需要用瀏覽器自動化工具（如 Chrome 擴充套件）實測前端登入後的功能（上傳、儀表板等）時，先查找此檔案取得帳密再登入測試，不要要求使用者重新提供。
- 測試完成、送出版控前，仍須依上述「版控安全檢查」規範，確認 `appsettings.json` 等設定檔中的機密值已清空。
