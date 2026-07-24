# BoulderingRecordAPI.Tests

`BoulderingRecordAPI` 專案的 xUnit 測試專案。

## 常用指令

```bash
# 執行測試（於方案根目錄執行）
dotnet test
```

## 程式碼慣例

- **不使用 `var`**：區域變數一律宣告明確型別，禁止使用 `var`（包含 `out var`、tuple 解構等寫法），以利閱讀時清楚掌握型別資訊。
