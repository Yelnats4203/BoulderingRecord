# BoulderingRecord Frontend

攀岩紀錄管理系統的前端專案，以 **Vue 3 + TypeScript + Vite** 建置，使用 `<script setup>` SFC 語法。

## 技術棧

- **Vue 3**（`<script setup>`）+ **Vue Router** + **Pinia**
- **TypeScript**
- **Vite** 建置工具
- **Chart.js** / **vue-chartjs** — 儀表板圖表
- **Axios** — API 請求
- 影片壓縮：`@ffmpeg/ffmpeg`（單執行緒 fallback）與瀏覽器原生 **WebCodecs API**（主要方案，詳見下方「影片壓縮」）
- `mp4-muxer` / `mp4box` — WebCodecs 壓縮後封裝 MP4

## 專案結構

```
frontend/
├── src/
│   ├── api/            # 後端 API 呼叫（auth、sends、sessions、users）
│   ├── assets/         # 靜態資源、全域樣式
│   ├── components/     # 共用元件（圖表、篩選表單、影片清單、對話框等）
│   ├── layouts/        # 版面配置（AppLayout：側邊選單 + 內容區）
│   ├── router/         # 路由設定與登入/權限守衛
│   ├── stores/         # Pinia store（auth、videoPlaybackCache）
│   ├── types/          # TypeScript 型別定義
│   ├── utils/          # 工具函式（影片壓縮、統計計算）
│   ├── views/          # 頁面元件
│   ├── App.vue
│   └── main.ts
├── public/              # 靜態資源（含 ffmpeg 核心檔）
├── index.html
├── vite.config.ts
└── vercel.json          # 部署平台為 Vercel，SPA rewrite 設定
```

## 常用指令

```bash
# 安裝套件
npm install

# 開發伺服器（預設連線 http://localhost:5016 的後端 API）
npm run dev

# 型別檢查 + 正式建置
npm run build

# 預覽正式建置產物
npm run preview
```

## 環境變數

- `.env.development` — 本機開發用，`VITE_API_BASE_URL=http://localhost:5016`
- `.env.production` — 正式環境用，`VITE_API_BASE_URL` 由部署平台（Vercel）注入，本機檔案內為空字串

## 路由與頁面

路由設定於 `src/router/index.ts`，透過 `router.beforeEach` 做登入狀態（`requiresAuth`）與編輯權限（`requiresEditPermission`）的導航守衛，未登入導向 `/login`，無權限則導回 `/dashboard`。

主要頁面（皆包在 `AppLayout` 版面內）：

- `/dashboard` — 儀表板（統計圖表）
- `/sessions/create` — 建立攀岩紀錄場次
- `/upload` — 上傳攀岩影片
- `/videos` — 影片紀錄清單
- `/change-password` — 修改密碼
- `/users`、`/user-list` — 使用者管理（需編輯權限）

## 影片壓縮

上傳影片會先在瀏覽器端壓縮再上傳：

- **正式壓縮方案**：`src/utils/videoCompressionWebCodecs.ts`，使用瀏覽器原生 **WebCodecs API** 解碼/編碼，效能較佳。
- **保留的 fallback**：`src/utils/videoCompression.ts`，使用 `@ffmpeg/ffmpeg`（WASM，單執行緒）；多執行緒方案已測試失敗並移除。

技術細節與踩雷紀錄詳見專案記憶（ffmpeg.wasm 需用 ESM 核心檔、`toBlobURL`、`exec()` 需檢查 exit code 等）。

## 部署

前端部署於 **Vercel**，`vercel.json` 設定所有路徑 rewrite 至 `index.html` 以支援 SPA 前端路由。

## 版控安全檢查

本專案為公開 Repo，前端不會直接存放 API Keys（Cloudinary 等機密皆由後端管理），但送出版控前仍應依方案根目錄 `CLAUDE.md` 的「版控安全檢查」規範，確認未意外提交任何機密資訊。前端測試用登入帳號密碼存放於方案根目錄的 `TestAccount.txt`（已列入 `.gitignore`）。
