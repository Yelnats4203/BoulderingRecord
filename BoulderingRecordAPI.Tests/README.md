# BoulderingRecordAPI.Tests

`BoulderingRecordAPI` 專案的 xUnit 測試專案。

## 常用指令

```bash
# 執行測試（於方案根目錄執行）
dotnet test
```

## 程式碼慣例

- **不使用 `var`**：區域變數一律宣告明確型別，禁止使用 `var`（包含 `out var`、tuple 解構等寫法），以利閱讀時清楚掌握型別資訊。

## API 測試案例清單

> **維護規則**：新增或修改任何 API 測試（Controller、Filter 等）時，必須同步在本節補上對應的測試案例敘述，保持與程式碼一致。

### AuthController（`Controllers/AuthControllerTests.cs`）

| 測試案例 | 說明 |
| --- | --- |
| `Login_ValidCredentials_ReturnsOkWithToken_AndStoresInCache` | 帳密正確時，登入回應 200 並回傳 token，且該 token 會存入 active token 快取。 |
| `Login_InvalidPassword_ReturnsUnauthorized` | 密碼錯誤時，登入回應 401。 |
| `Login_UnknownAcc_ReturnsUnauthorized` | 帳號不存在時，登入回應 401。 |
| `Login_SameAccTwice_OldTokenInvalidated_NewTokenActive` | 同一帳號重複登入時，快取中的 active token 會被新 token 取代，舊 token 失效（防止重複登入）。 |
| `Logout_RemovesActiveToken` | 登出後，該帳號的 active token 會從快取中移除。 |
| `Refresh_ReturnsNewToken_OldTokenNoLongerActive` | 呼叫換發 token 後，回應新 token 並成為快取中的 active token，舊 token 不再有效。 |

### SendsController（`Controllers/SendsControllerTests.cs`）

| 測試案例 | 說明 |
| --- | --- |
| `UploadAuthorization_Authenticated_ReturnsAuthorizationForUser` | 已登入使用者請求上傳授權時，回應 200，並回傳含該使用者 ID 的 public ID 簽章授權資訊。 |
| `UploadAuthorization_NotAuthenticated_ReturnsUnauthorized` | 未登入使用者請求上傳授權時，回應 401。 |
| `Upload_AuthenticatedUser_ReturnsCreatedWithBackendAssignedFields` | 已登入使用者於影片直傳 Cloudinary 完成後建立紀錄時，回應 201，且岩館名稱、難度、備註等欄位正確帶入，並由後端指派上傳者 ID 與上傳時間。 |
| `Upload_NoAuthenticatedUser_ReturnsUnauthorized` | 未登入使用者嘗試建立紀錄時，回應 401。 |
| `Upload_ResourceNotUploaded_ReturnsBadRequest` | 對應的影片尚未實際上傳至 Cloudinary（`ResourceExistsAsync` 回傳 false）時，回應 400，避免偽造未上傳的紀錄。 |
| `GetAll_ReturnsAllSends` | 查詢所有紀錄時，回應 200 並回傳全部紀錄清單。 |
| `GetById_ExistingId_ReturnsSend` | 以存在的 ID 查詢單筆紀錄時，回應 200 並回傳對應紀錄。 |
| `GetById_UnknownId_ReturnsNotFound` | 以不存在的 ID 查詢單筆紀錄時，回應 404。 |
| `GetMine_NotAuthenticated_ReturnsUnauthorized` | 未登入使用者查詢自己的影片紀錄清單時，回應 401。 |
| `GetMine_NoFilter_ReturnsOnlyOwnSends` | 未帶任何篩選條件查詢時，僅回傳目前登入使用者自己的紀錄，不含其他使用者的紀錄，且回應含 Cloudinary 簽章縮圖網址。 |
| `GetMine_GymNameFilter_ReturnsPartialMatchOnly` | 以岩館名稱關鍵字篩選時，採部分比對（模糊搜尋），僅回傳名稱包含關鍵字的紀錄。 |
| `GetMine_UploadedAtRangeFilter_ReturnsSendsWithinRange` | 以上傳時間區間篩選時，僅回傳落在區間內的紀錄。 |
| `GetMine_DifficultyRangeFilter_ReturnsSendsWithinRange` | 以難度區間篩選時，僅回傳難度落在區間內的紀錄。 |
| `GetMine_NoMatchingSends_ReturnsEmpty` | 篩選條件無符合紀錄時，回應 200 並回傳空陣列。 |
| `GetVideo_UnknownId_ReturnsNotFound` | 以不存在的 ID 讀取影片時，回應 404。 |
| `GetVideo_PrivateSend_NotOwner_ReturnsNotFound` | 紀錄可見度為「不公開」且非上傳者本人讀取時，回應 404。 |
| `GetVideo_PrivateSend_Owner_ReturnsRedirectToSignedUrl` | 紀錄可見度為「不公開」但由上傳者本人讀取時，回應 302 並導向 Cloudinary 簽章播放網址。 |
| `GetVideo_PublicOrShareableSend_NotOwner_ReturnsRedirectToSignedUrl` | 紀錄可見度為「公開」或「可被分享」時，非上傳者本人也能取得導向播放網址的 302 回應。 |

### SessionsController（`Controllers/SessionsControllerTests.cs`）

| 測試案例 | 說明 |
| --- | --- |
| `Create_Authenticated_ReturnsCreatedWithBackendAssignedUserId` | 已登入使用者建立活動紀錄時，回應 201，且岩館名稱、各級數完攀/未完攀次數正確帶入，並由後端指派所屬使用者 ID。 |
| `Create_NotAuthenticated_ReturnsUnauthorized` | 未登入使用者嘗試建立活動紀錄時，回應 401。 |
| `GetAll_ReturnsOnlyCurrentUserSessions` | 查詢活動紀錄清單時，僅回傳目前登入使用者自己的紀錄，不包含其他使用者的紀錄。 |
| `GetById_Owner_ReturnsSession` | 本人查詢自己擁有的活動紀錄時，回應 200 並回傳對應紀錄。 |
| `GetById_NotOwner_ReturnsNotFound` | 查詢非本人擁有的活動紀錄時，回應 404。 |
| `GetById_UnknownId_ReturnsNotFound` | 以不存在的 ID 查詢活動紀錄時，回應 404。 |
| `Update_Owner_UpdatesFieldsAndGradeCounts` | 本人更新自己擁有的活動紀錄時，日期、岩館名稱與各級數統計皆會被覆蓋為新值。 |
| `Update_NotOwner_ReturnsNotFound` | 嘗試更新非本人擁有的活動紀錄時，回應 404。 |
| `Delete_Owner_ReturnsNoContentAndRemoves` | 本人刪除自己擁有的活動紀錄時，回應 204，且該紀錄之後查無資料。 |
| `Delete_NotOwner_ReturnsNotFound` | 嘗試刪除非本人擁有的活動紀錄時，回應 404。 |

### TokenAuthorizationFilter（`Filters/TokenAuthorizationFilterTests.cs`）

| 測試案例 | 說明 |
| --- | --- |
| `OnAuthorizationAsync_NoAuthorizationHeader_ReturnsUnauthorized` | 請求未帶 `Authorization` 標頭時，過濾器回應 401。 |
| `OnAuthorizationAsync_ExpiredToken_ReturnsUnauthorized` | 帶入已過期的 token 時，回應 401。 |
| `OnAuthorizationAsync_TamperedToken_ReturnsUnauthorized` | 帶入被竄改（簽章不符）的 token 時，回應 401。 |
| `OnAuthorizationAsync_ValidTokenNotMatchingCache_ReturnsUnauthorized` | token 本身有效，但與快取中的 active token 不一致（已被新登入取代）時，回應 401 且不設定 `HttpContext.User`。 |
| `OnAuthorizationAsync_ValidTokenMatchingCache_SetsHttpContextUser` | token 有效且與快取中的 active token 一致時，放行並將使用者資訊設定到 `HttpContext.User`。 |
