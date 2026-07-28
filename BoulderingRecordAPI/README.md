# BoulderingRecordAPI

以 **.NET 10**（`net10.0`）為目標、採用 controller-based（`Microsoft.NET.Sdk.Web`）架構的攀岩紀錄 ASP.NET Core Web API。

## 專案架構

```
Controllers/    # API 進入點（AuthController、RecordsController）
Filters/        # TokenAuthorizeAttribute / TokenAuthorizationFilter：自訂 token 驗證
Services/       # 商業邏輯服務（TokenService、LocalVideoStorageService、MemoryActiveTokenStore）
Repositories/   # 資料存取介面與實作（IUserRepository、IRecordRepository）
Entities/       # 資料庫實體（User、Record）
Data/           # DbContext 與 provider（SQLite／SQL Server）切換設定
Migrations/     # 依 provider 分開存放的 EF Core migration（Sqlite/、SqlServer/）
Models/         # 各 API 的 Request／Response DTO（Auth/、Records/）
Options/         # 可設定選項（JwtSettings、VideoStorageOptions）
```

### 分層說明

- **Controller** 只負責接收請求、驗證使用者身分（透過 `[TokenAuthorize]`）、呼叫 repository／service，並轉換成 Response DTO。
- **Repository** 以介面（`IUserRepository`、`IRecordRepository`）為主，不直接綁定特定 EF Core provider，商業邏輯與 provider 無關。
- **Data** 層依設定值 `Database:Provider` 動態切換 `BoulderingRecordSqliteDbContext` 或 `BoulderingRecordSqlServerDbContext`，兩者皆繼承共用的 `BoulderingRecordDbContext`。

## 資料持久化架構

資料存取層為抽象化的 repository 層，讓同一套程式碼可以搭配兩種不同的 EF Core provider 運作：

- **SQLite** — 測試環境使用
- **MSSQL（SQL Server）** — 正式部署環境使用

透過設定值 `Database:Provider`（`Sqlite` 或 `SqlServer`）決定使用哪個 provider，對應的連線字串取自 `ConnectionStrings:<Provider>`。切換 provider 不需更動 repository 或商業邏輯程式碼；各 provider 的 migration 分別存放在 `Migrations/Sqlite/`、`Migrations/SqlServer/`。

## 身分驗證機制

- 採用 **JWT**，登入成功後由 `TokenService` 產生 token，並存入 `IActiveTokenStore`（目前實作為 `MemoryActiveTokenStore`，以 `IMemoryCache` 為底層）。
- **同一帳號僅允許一組有效 token**：重新登入或換發 token 時，快取中的舊 token 會被新 token 取代，舊 token 隨即失效（防止重複登入）。
- 需要驗證的端點加上 `[TokenAuthorize]`，交由 `TokenAuthorizationFilter` 驗證 `Authorization: Bearer <token>` 標頭，並檢查 token 是否與快取中的 active token 一致。
- 相關設定：`Jwt:Key`、`Jwt:Issuer`、`Jwt:Audience`、`Jwt:AccessTokenExpiresMinutes`。

## API 端點

### AuthController（`/api/auth`）

| Method | 路徑 | 驗證 | 說明 |
| --- | --- | --- | --- |
| POST | `/api/auth/login` | 不需 | 以帳號密碼登入，回傳 JWT token 與到期時間，並存入 active token 快取。 |
| POST | `/api/auth/logout` | 需要 | 登出，將該帳號的 active token 從快取移除。 |
| POST | `/api/auth/refresh` | 需要 | 換發新 token，取代快取中原有的 active token。 |

### RecordsController（`/api/records`）

| Method | 路徑 | 驗證 | 說明 |
| --- | --- | --- | --- |
| POST | `/api/records` | 需要 | 上傳攀岩紀錄影片（`multipart/form-data`：影片檔、岩館名稱、難度、備註），影片存放於本機儲存，上傳者與上傳時間由後端指派。 |
| GET | `/api/records` | 不需 | 取得所有攀岩紀錄清單。 |
| GET | `/api/records/{id}` | 不需 | 依 ID 取得單筆攀岩紀錄，不存在則回傳 404。 |

影片實際存放邏輯由 `IVideoStorageService` 抽象化，目前實作 `LocalVideoStorageService` 會將檔案存到設定值 `VideoStorage:Directory`（預設 `userUpload`）下依 `userId` 分開的子資料夾，即 `{VideoStorage:Directory}/{userId}/{recordId}{副檔名}`。

## 常用指令

```bash
# 建置
dotnet build

# 執行（於方案根目錄執行）
dotnet run --project BoulderingRecordAPI
```

測試專案為 `BoulderingRecordAPI.Tests`（xUnit），所有 API 測試案例的敘述維護於該專案的 README，詳見 [`BoulderingRecordAPI.Tests/README.md`](../BoulderingRecordAPI.Tests/README.md)。

## 程式碼慣例

- **不使用 `var`**：區域變數一律宣告明確型別，禁止使用 `var`（包含 `out var`、tuple 解構等寫法），以利閱讀時清楚掌握型別資訊。
- **Entity 與 API 端點須加上 summary 註解**：`Entities/` 下的實體類別（含其屬性）與各 Controller 的 API 端點方法，一律以 `/// <summary>` XML 文件註解說明其用途，以利閱讀與產生 API 文件。
