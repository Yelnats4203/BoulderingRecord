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
| `Login_ValidCredentials_ReturnsOkWithToken_AndStoresInCache` | 帳密正確時，登入回應 200 並回傳 token 與使用者名稱（`Username`），且該 token 會存入 active token 快取。 |
| `Login_InvalidPassword_ReturnsUnauthorized` | 密碼錯誤時，登入回應 401。 |
| `Login_UnknownAcc_ReturnsUnauthorized` | 帳號不存在時，登入回應 401。 |
| `Login_SameAccTwice_OldTokenInvalidated_NewTokenActive` | 同一帳號重複登入時，快取中的 active token 會被新 token 取代，舊 token 失效（防止重複登入）。 |
| `Logout_RemovesActiveToken` | 登出後，該帳號的 active token 會從快取中移除。 |
| `Refresh_ReturnsNewToken_OldTokenNoLongerActive` | 呼叫換發 token 後，回應新 token 並成為快取中的 active token，舊 token 不再有效。 |

### FriendsController（`Controllers/FriendsControllerTests.cs`）

| 測試案例 | 說明 |
| --- | --- |
| `GetFriends_NotAuthenticated_ReturnsUnauthorized` | 未登入使用者查詢好友清單時，回應 401。 |
| `GetFriends_ReturnsAcceptedRelationsRegardlessOfDirection` | 查詢好友清單時，回傳所有已成立（Accepted）的好友關係，不論目前使用者是邀請發起人或接收人。 |
| `GetPendingRequests_NotAuthenticated_ReturnsUnauthorized` | 未登入使用者查詢收到的邀請清單時，回應 401。 |
| `GetPendingRequests_OnlyReturnsPendingReceivedByCurrentUser` | 查詢收到的邀請清單時，僅回傳自己收到且尚未回應（Pending）的邀請，不含自己送出的邀請或已成立的好友關係。 |
| `SendRequest_NotAuthenticated_ReturnsUnauthorized` | 未登入使用者送出好友邀請時，回應 401。 |
| `SendRequest_ToSelf_ReturnsBadRequest` | 對自己送出好友邀請時，回應 400。 |
| `SendRequest_AddresseeNotFound_ReturnsNotFound` | 邀請對象的使用者不存在時，回應 404。 |
| `SendRequest_AlreadyPendingBetweenUsers_ReturnsBadRequest` | 雙方之間已存在邀請中（Pending）的好友邀請時，回應 400。 |
| `SendRequest_AlreadyFriends_ReturnsBadRequest` | 雙方已是好友（Accepted）時，回應 400。 |
| `SendRequest_Valid_CreatesPendingRequestAndReturnsCreated` | 成功送出邀請時，回應 201 並附帶邀請對象資訊，且資料庫中會建立一筆 Pending 狀態、發起人為目前使用者的邀請紀錄。 |
| `Accept_NotAuthenticated_ReturnsUnauthorized` | 未登入使用者接受邀請時，回應 401。 |
| `Accept_UnknownId_ReturnsNotFound` | 以不存在的邀請 ID 接受邀請時，回應 404。 |
| `Accept_CurrentUserNotAddressee_ReturnsNotFound` | 目前使用者非該邀請的收件人時，回應 404。 |
| `Accept_AlreadyAccepted_ReturnsNotFound` | 該邀請已被接受過（非 Pending 狀態）時，回應 404。 |
| `Accept_PendingAsAddressee_UpdatesStatusAndReturnsOk` | 收件人接受待處理邀請時，回應 200 並附帶好友資訊，且該邀請狀態會更新為 Accepted。 |
| `Delete_NotAuthenticated_ReturnsUnauthorized` | 未登入使用者刪除好友關係／邀請時，回應 401。 |
| `Delete_UnknownId_ReturnsNotFound` | 以不存在的 ID 刪除時，回應 404。 |
| `Delete_CurrentUserNotPartOfRelation_ReturnsNotFound` | 目前使用者不是該邀請/好友關係雙方之一時，回應 404。 |
| `Delete_AsRequester_RemovesRequestAndReturnsNoContent` | 以邀請發起人身分刪除時，回應 204，且該筆資料自資料庫移除。 |
| `Delete_AsAddressee_RemovesRequestAndReturnsNoContent` | 以邀請接收人身分刪除時，回應 204，且該筆資料自資料庫移除。 |
| `GetFriendVideos_NotAuthenticated_ReturnsUnauthorized` | 未登入使用者查詢好友影片時，回應 401。 |
| `GetFriendVideos_NoRelation_ReturnsNotFound` | 與查詢對象之間沒有任何好友關係時，回應 404。 |
| `GetFriendVideos_PendingNotAccepted_ReturnsNotFound` | 與查詢對象之間僅有 Pending 邀請、尚未成為好友時，回應 404。 |
| `GetFriendVideos_Accepted_ReturnsOnlyPublicSends` | 與查詢對象已是好友時，僅回傳該好友可見度為「公開」的影片紀錄，不含私人影片。 |
| `GetRecentVideos_NotAuthenticated_ReturnsUnauthorized` | 未登入使用者查詢好友動態時，回應 401。 |
| `GetRecentVideos_MergesPublicSendsAcrossFriendsOrderedByUploadedAtDescending` | 查詢好友動態時，合併所有好友的公開影片、依上傳時間降冪排序，且不包含非好友使用者的影片。 |

### HealthController（`Controllers/HealthControllerTests.cs`）

| 測試案例 | 說明 |
| --- | --- |
| `Get_ReturnsOk` | 呼叫健康檢查端點（無需登入）時，回應 200，供 UptimeRobot 等外部監控服務定時呼叫確認伺服器存活。 |

### SendsController（`Controllers/SendsControllerTests.cs`）

| 測試案例 | 說明 |
| --- | --- |
| `UploadAuthorization_Authenticated_ReturnsAuthorizationForUser` | 已登入使用者請求上傳授權時，回應 200，並回傳含該使用者 ID 的 public ID 簽章授權資訊。 |
| `UploadAuthorization_NotAuthenticated_ReturnsUnauthorized` | 未登入使用者請求上傳授權時，回應 401。 |
| `Upload_AuthenticatedUser_ReturnsCreatedWithBackendAssignedFields` | 已登入使用者於影片直傳 Cloudinary 完成後建立紀錄時，回應 201，且岩館名稱、難度、備註等欄位正確帶入，並由後端指派上傳者 ID 與上傳時間。 |
| `Upload_NoAuthenticatedUser_ReturnsUnauthorized` | 未登入使用者嘗試建立紀錄時，回應 401。 |
| `Upload_ResourceNotUploaded_ReturnsBadRequest` | 對應的影片尚未實際上傳至 Cloudinary（`ResourceExistsAsync` 回傳 false）時，回應 400，避免偽造未上傳的紀錄。 |
| `Upload_AttemptsProvided_ReturnsCreatedWithAttempts` | 建立紀錄時帶入正整數嘗試次數，回應中的嘗試次數正確帶入。 |
| `Upload_AttemptsNonPositive_ReturnsBadRequest` | 建立紀錄時嘗試次數帶入 0 或負數，回應 400。 |
| `GetAll_ReturnsAllSends` | 查詢所有紀錄時，回應 200 並回傳全部紀錄清單。 |
| `GetById_ExistingId_ReturnsSend` | 以存在的 ID 查詢單筆紀錄時，回應 200 並回傳對應紀錄。 |
| `GetById_UnknownId_ReturnsNotFound` | 以不存在的 ID 查詢單筆紀錄時，回應 404。 |
| `GetMine_NotAuthenticated_ReturnsUnauthorized` | 未登入使用者查詢自己的影片紀錄清單時，回應 401。 |
| `GetMine_NoFilter_ReturnsOnlyOwnSends` | 未帶任何篩選條件查詢時，僅回傳目前登入使用者自己的紀錄，不含其他使用者的紀錄，且回應含 Cloudinary 簽章縮圖網址。 |
| `GetMine_GymNameFilter_ReturnsPartialMatchOnly` | 以岩館名稱關鍵字篩選時，採部分比對（模糊搜尋），僅回傳名稱包含關鍵字的紀錄。 |
| `GetMine_UploadedAtRangeFilter_ReturnsSendsWithinRange` | 以上傳時間區間篩選時，僅回傳落在區間內的紀錄。 |
| `GetMine_DifficultyRangeFilter_ReturnsSendsWithinRange` | 以難度區間篩選時，僅回傳難度落在區間內的紀錄。 |
| `GetMine_NoMatchingSends_ReturnsEmpty` | 篩選條件無符合紀錄時，回應 200 並回傳空陣列。 |
| `Update_Owner_UpdatesFields` | 本人編輯自己擁有的紀錄時，上傳時間、岩館、難度、嘗試次數、備註皆會被覆蓋為新值。 |
| `Update_UploadedAtDefault_ReturnsBadRequest` | 上傳時間為預設值（未填）時，回應 400。 |
| `Update_AttemptsNonPositive_ReturnsBadRequest` | 編輯紀錄時嘗試次數帶入 0 或負數，回應 400。 |
| `Update_NotOwner_ReturnsNotFound` | 嘗試編輯非本人擁有的紀錄時，回應 404。 |
| `Update_NotAuthenticated_ReturnsUnauthorized` | 未登入使用者嘗試編輯紀錄時，回應 401。 |
| `Delete_Owner_DeletesRecordAndCloudinaryResource` | 本人刪除自己擁有的紀錄時，回應 204，同時刪除 Cloudinary 上對應的影片資源，且該紀錄之後查無資料。 |
| `Delete_NotOwner_ReturnsNotFound` | 嘗試刪除非本人擁有的紀錄時，回應 404。 |
| `Delete_NotAuthenticated_ReturnsUnauthorized` | 未登入使用者嘗試刪除紀錄時，回應 401。 |
| `GetVideo_UnknownId_ReturnsNotFound` | 以不存在的 ID 讀取影片時，回應 404。 |
| `GetVideo_PrivateSend_NotOwner_ReturnsNotFound` | 紀錄可見度為「不公開」且非上傳者本人讀取時，回應 404。 |
| `GetVideo_PrivateSend_Owner_ReturnsSignedUrl` | 紀錄可見度為「不公開」但由上傳者本人讀取時，回應 200 並附帶 Cloudinary 簽章播放網址 JSON。 |
| `GetVideo_PublicOrShareableSend_NotOwner_ReturnsSignedUrl` | 紀錄可見度為「公開」或「可被分享」時，非上傳者本人也能取得包含播放網址的 200 回應。 |

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
| `Update_Owner_ChangingGradeCountSize_AddsAndRemovesRecords` | 更新時若各級數統計筆數增加或減少，僅新增/刪除差額筆數，就地更新既有筆數，結果的筆數與內容皆正確。 |
| `Update_ConcurrentModification_ReturnsConflict` | 更新時若資料庫層拋出 `DbUpdateConcurrencyException`（如同一紀錄遭併發修改），回應 409 而非未處理例外。 |
| `Update_NotOwner_ReturnsNotFound` | 嘗試更新非本人擁有的活動紀錄時，回應 404。 |
| `Delete_Owner_ReturnsNoContentAndRemoves` | 本人刪除自己擁有的活動紀錄時，回應 204，且該紀錄之後查無資料。 |
| `Delete_NotOwner_ReturnsNotFound` | 嘗試刪除非本人擁有的活動紀錄時，回應 404。 |

### UsersController（`Controllers/UsersControllerTests.cs`）

| 測試案例 | 說明 |
| --- | --- |
| `Create_NewAcc_ReturnsCreatedAndPersistsHashedPassword` | 建立新使用者時，回應 201，且密碼以雜湊後的值存入資料庫（非明文）。 |
| `Create_IsDemoAccTrue_PersistsDemoAccountFlag` | 建立使用者時勾選「測試帳號」，該旗標會正確存入資料庫。 |
| `Create_DuplicateAcc_ReturnsBadRequest` | 帳號已存在時，回應 400。 |
| `Create_MissingRequiredField_ReturnsBadRequest` | 使用者名稱、帳號或密碼任一為空時，回應 400。 |
| `Create_WeakPassword_ReturnsBadRequest` | 密碼不符合強度規則（缺大小寫英文、數字或特殊符號等）時，回應 400。 |
| `ResetPassword_ExistingAcc_ReturnsNoContentAndPersistsHashedPassword` | 具編輯權限的使用者重設指定帳號密碼時，回應 204，且新密碼以雜湊後的值存入資料庫（非明文）。 |
| `ResetPassword_UnknownAcc_ReturnsNotFound` | 欲重設密碼的帳號不存在時，回應 404。 |
| `ResetPassword_WeakPassword_ReturnsBadRequest` | 新密碼不符合強度規則時，回應 400。 |
| `ResetPassword_MissingRequiredField_ReturnsBadRequest` | 帳號或新密碼任一為空時，回應 400。 |
| `GetAll_ReturnsAllUsersWithoutPassword` | 查詢所有使用者時，回應 200 並回傳全部使用者（含編輯權限旗標），不含密碼欄位。 |
| `Search_CurrentUserWithoutEditPermission_ExcludesAdminCandidates` | 目前登入者不具編輯權限時，搜尋結果會排除具編輯權限（管理員）的候選使用者，但仍包含一般使用者。 |
| `Search_CurrentUserWithEditPermission_IncludesAllCandidates` | 目前登入者本身具編輯權限時，搜尋結果不受過濾，同時包含一般使用者與其他管理員。 |
| `Search_ExcludesCurrentUserFromResults` | 搜尋結果不包含使用者自己。 |

### GymsController（`Controllers/GymsControllerTests.cs`）

| 測試案例 | 說明 |
| --- | --- |
| `GetNames_NotAuthenticated_ReturnsUnauthorized` | 未登入使用者查詢岩館名稱清單時，回應 401。 |
| `GetNames_HasGymNames_ReturnsDistinctSortedNames` | 已登入使用者查詢岩館名稱清單時，回傳去除重複值與空白／null 值後、依字串排序的清單。 |
| `GetNames_NoGymNames_ReturnsEmptyList` | 資料庫中沒有任何岩館名稱時，回傳空陣列。 |

### TokenAuthorizationFilter（`Filters/TokenAuthorizationFilterTests.cs`）

| 測試案例 | 說明 |
| --- | --- |
| `OnAuthorizationAsync_NoAuthorizationHeader_ReturnsUnauthorizedWithSessionExpiredReason` | 請求未帶 `Authorization` 標頭時，回應 401 並附上 `UnauthorizedErrorResponse { Reason = SessionExpired }`。 |
| `OnAuthorizationAsync_ExpiredToken_ReturnsUnauthorizedWithSessionExpiredReason` | 帶入已過期的 token 時，回應 401 並附上 `UnauthorizedErrorResponse { Reason = SessionExpired }`。 |
| `OnAuthorizationAsync_TamperedToken_ReturnsUnauthorizedWithSessionExpiredReason` | 帶入被竄改（簽章不符）的 token 時，回應 401 並附上 `UnauthorizedErrorResponse { Reason = SessionExpired }`。 |
| `OnAuthorizationAsync_ValidTokenNotMatchingCache_ReturnsUnauthorizedWithDuplicateLoginReason` | token 本身有效，但與快取中的 active token 不一致（已被新登入取代）時，回應 401 並附上 `UnauthorizedErrorResponse { Reason = DuplicateLogin }`，且不設定 `HttpContext.User`。 |
| `OnAuthorizationAsync_ValidTokenMatchingCache_SetsHttpContextUser` | token 有效且與快取中的 active token 一致時，放行並將使用者資訊設定到 `HttpContext.User`。 |
