# BoulderingRecordAPI

以 **.NET 10**（`net10.0`）為目標、採用 controller-based（`Microsoft.NET.Sdk.Web`）架構的攀岩紀錄 ASP.NET Core Web API。

## 專案架構

```
Controllers/    # API 進入點（AuthController、SendsController、SessionsController）
Filters/        # TokenAuthorizeAttribute / TokenAuthorizationFilter：自訂 token 驗證
Services/       # 商業邏輯服務（TokenService、CloudinaryVideoStorageService、MemoryActiveTokenStore）
Repositories/   # 資料存取介面與實作（IUserRepository、ISendRepository、ISessionRepository）
Entities/       # 資料庫實體（User、Send、Session、SessionGradeRecord）
Data/           # DbContext 與 provider（SQLite／SQL Server）切換設定
Migrations/     # 依 provider 分開存放的 EF Core migration（Sqlite/、SqlServer/）
Models/         # 各 API 的 Request／Response DTO（Auth/、Sends/、Sessions/）
Options/         # 可設定選項（JwtSettings、CloudinaryOptions）
```

### 分層說明

- **Controller** 只負責接收請求、驗證使用者身分（透過 `[TokenAuthorize]`）、呼叫 repository／service，並轉換成 Response DTO。
- **Repository** 以介面（`IUserRepository`、`ISendRepository`）為主，不直接綁定特定 EF Core provider，商業邏輯與 provider 無關。
- **Data** 層依設定值 `Database:Provider` 動態切換 `BoulderingRecordSqliteDbContext` 或 `BoulderingRecordSqlServerDbContext`，兩者皆繼承共用的 `BoulderingRecordDbContext`。

## 資料持久化架構

資料存取層為抽象化的 repository 層，讓同一套程式碼可以搭配兩種不同的 EF Core provider 運作：

- **SQLite** — 測試環境使用
- **MSSQL（SQL Server）** — 正式部署環境使用

透過設定值 `Database:Provider`（`Sqlite` 或 `SqlServer`）決定使用哪個 provider，對應的連線字串取自 `ConnectionStrings:<Provider>`。切換 provider 不需更動 repository 或商業邏輯程式碼；各 provider 的 migration 分別存放在 `Migrations/Sqlite/`、`Migrations/SqlServer/`。

## 身分驗證機制

- 採用 **JWT**，登入成功後由 `TokenService` 產生 token，並存入 `IActiveTokenStore`（目前實作為 `MemoryActiveTokenStore`，以 `IMemoryCache` 為底層）。
- **同一帳號僅允許一組有效 token**：重新登入或換發 token 時，快取中的舊 token 會被新 token 取代，舊 token 隨即失效（防止重複登入）。
- 需要驗證的端點加上 `[TokenAuthorize]`，交由 `TokenAuthorizationFilter` 驗證 `Authorization: Bearer <token>` 標頭，並檢查 token 是否與快取中的 active token 一致。驗證失敗時一律回應 401，並附上 body `{ "reason": "SessionExpired" | "DuplicateLogin" }`（`UnauthorizedErrorResponse`）供前端顯示對應提示：
  - `SessionExpired`：未帶 token、token 已過期或無效，須重新登入。
  - `DuplicateLogin`：token 本身簽章有效，但與快取中的 active token 不一致，代表已被其他裝置的新登入取代。
- 相關設定：`Jwt:Key`、`Jwt:Issuer`、`Jwt:Audience`、`Jwt:AccessTokenExpiresMinutes`。

## API 端點

### AuthController（`/auth`）

| Method | 路徑 | 驗證 | 說明 |
| --- | --- | --- | --- |
| POST | `/auth/login` | 不需 | 以帳號密碼登入，回傳 JWT token 與到期時間，並存入 active token 快取。 |
| POST | `/auth/logout` | 需要 | 登出，將該帳號的 active token 從快取移除。 |
| POST | `/auth/refresh` | 需要 | 換發新 token，取代快取中原有的 active token。 |

### SendsController（`/sends`）

| Method | 路徑 | 驗證 | 說明 |
| --- | --- | --- | --- |
| POST | `/sends/upload-authorization` | 需要 | 取得供前端直接上傳影片到 Cloudinary 的簽章授權（`sendId`、`publicId`、`folder`、`cloudName`、`apiKey`、`timestamp`、`signature`），純運算無副作用，不寫入資料庫。 |
| POST | `/sends` | 需要 | 影片已直接上傳至 Cloudinary 後，帶入 JSON body（`sendId`、岩館名稱、難度、備註、上傳日期）建立完攀紀錄，上傳者由後端指派；上傳日期若未提供則預設為今日。 |
| GET | `/sends` | 不需 | 取得所有完攀紀錄清單。 |
| GET | `/sends/mine` | 需要 | 依岩館名稱（模糊搜尋）、上傳日期區間、難度區間，取得目前登入使用者自己上傳的影片紀錄清單，含 Cloudinary 簽章縮圖網址。 |
| GET | `/sends/{id}` | 不需 | 依 ID 取得單筆完攀紀錄，不存在則回傳 404。 |
| PUT | `/sends/{id}` | 需要 | 編輯完攀紀錄的上傳日期（必填）、岩館、難度、備註；僅上傳者本人可編輯，非本人或不存在回傳 404。 |
| DELETE | `/sends/{id}` | 需要 | 刪除完攀紀錄，同時刪除 Cloudinary 上對應的影片資源；僅上傳者本人可刪除，非本人或不存在回傳 404。 |

影片儲存改採 **Cloudinary**（雲端物件儲存 + CDN），實際流程由 `IVideoStorageService` 抽象化，目前實作 `CloudinaryVideoStorageService`：

1. 前端呼叫 `POST /sends/upload-authorization`，後端產生 `sendId`、`public_id`（`sends/{userId}/{sendId}`）、`folder`（`Bouldering/{dev|proc}/{userId}`，依 `IHostEnvironment.IsDevelopment()` 判斷目前是開發環境還是部署環境，並讓影片在 Cloudinary Console 中依使用者歸類到對應資料夾）與時間戳記，並以 API Secret 簽章，回傳給前端。
2. 前端持簽章資訊直接將影片 `multipart/form-data` 上傳到 Cloudinary 的 Upload API（`https://api.cloudinary.com/v1_1/{cloudName}/video/upload`），影片位元組不經過本系統後端。上傳時指定 `type=authenticated`，確保未經簽章一律無法存取。
3. 前端呼叫 `POST /sends` 建立紀錄，後端會先以 Cloudinary Admin API 確認該 `public_id` 真的存在，才建立 `Send`（`VideoPublicId` 存放 Cloudinary 的 public ID）。

私人紀錄的存取控制透過 `GET /sends/{id}/video` 端點延續：驗證權限（不存在 → 404、私人且非本人 → 404）通過後，由後端即時產生一組 Cloudinary 簽章播放網址（以 `Cloudinary:ApiSecret` 做標準 delivery URL 簽章，非 Cloudinary Advanced 方案才有的 token-based authentication）並以 302 導向，瀏覽器接續由 Cloudinary CDN 直接播放（原生支援 Range 請求）。此簽章網址不會自動過期，但僅在每次請求通過權限檢查後才即時產生並回傳給授權使用者，非授權者無法取得。相關設定值為 `Cloudinary:CloudName`、`Cloudinary:ApiKey`、`Cloudinary:ApiSecret`。

### SessionsController（`/sessions`）

| Method | 路徑 | 驗證 | 說明 |
| --- | --- | --- | --- |
| POST | `/sessions` | 需要 | 建立抱石活動紀錄（日期、岩館名稱、各 V-Scale 級數的完攀／未完攀次數），所屬使用者由後端指派。 |
| GET | `/sessions` | 需要 | 取得目前登入使用者的所有活動紀錄清單。 |
| GET | `/sessions/{id}` | 需要 | 依 ID 取得單筆活動紀錄，不存在或非本人擁有則回傳 404。 |
| PUT | `/sessions/{id}` | 需要 | 更新活動紀錄的日期、岩館名稱與各級數統計，不存在或非本人擁有則回傳 404。 |
| DELETE | `/sessions/{id}` | 需要 | 刪除活動紀錄，成功回傳 204，不存在或非本人擁有則回傳 404。 |

Sessions 為個人活動統計紀錄，所有端點皆需登入，且僅能存取／操作自己的資料；各級數的完攀／未完攀次數統計以子紀錄（`SessionGradeRecord`）表示，難度採用 V-Scale（以整數儲存，例如 `3` 代表 V3）。

## 常用指令

```bash
# 建置
dotnet build

# 執行（於方案根目錄執行）
dotnet run --project BoulderingRecordAPI
```

## API 文件（Swagger UI）

- OpenAPI 文件由內建的 `Microsoft.AspNetCore.OpenApi`（`AddOpenApi()` / `MapOpenApi()`）產生，僅在 Development 環境啟用，路徑為 `/openapi/v1.json`。
- 搭配 `Swashbuckle.AspNetCore.SwaggerUI` 提供互動式網頁介面，指向上述 OpenAPI JSON，路徑為 `/swagger/index.html`（僅 Development 環境可用）。

測試專案為 `BoulderingRecordAPI.Tests`（xUnit），所有 API 測試案例的敘述維護於該專案的 README，詳見 [`BoulderingRecordAPI.Tests/README.md`](../BoulderingRecordAPI.Tests/README.md)。

## 程式碼慣例

- **不使用 `var`**：區域變數一律宣告明確型別，禁止使用 `var`（包含 `out var`、tuple 解構等寫法），以利閱讀時清楚掌握型別資訊。
- **Entity、Request／Response 與 API 端點須加上 summary 註解**：`Entities/` 下的實體類別（含其屬性）、`Models/` 下的 Request／Response DTO（含其參數／屬性），以及各 Controller 的 API 端點方法，一律以 `/// <summary>` XML 文件註解說明其用途，以利閱讀與產生 API 文件。
