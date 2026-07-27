<script setup lang="ts">
import type { RecordResponse } from '../types/records'

defineProps<{
  records: RecordResponse[]
}>()

function formatDate(value: string): string {
  return new Date(value).toLocaleString()
}

function shortId(value: string): string {
  return value.slice(0, 8)
}
</script>

<template>
  <div v-if="records.length === 0" class="card empty-state">目前尚無任何紀錄。</div>

  <template v-else>
    <div class="record-cards">
      <div v-for="record in records" :key="record.id" class="card record-card">
        <div class="record-row"><span class="record-label">岩館</span><span>{{ record.gymName ?? '-' }}</span></div>
        <div class="record-row"><span class="record-label">難度</span><span>{{ record.difficulty ?? '-' }}</span></div>
        <div class="record-row"><span class="record-label">備註</span><span>{{ record.note ?? '-' }}</span></div>
        <div class="record-row"><span class="record-label">上傳時間</span><span>{{ formatDate(record.uploadedAt) }}</span></div>
        <div class="record-row"><span class="record-label">上傳者</span><span :title="record.uploaderId">{{ shortId(record.uploaderId) }}</span></div>
      </div>
    </div>

    <table class="record-table">
      <thead>
        <tr>
          <th>岩館</th>
          <th>難度</th>
          <th>備註</th>
          <th>上傳時間</th>
          <th>上傳者</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="record in records" :key="record.id">
          <td>{{ record.gymName ?? '-' }}</td>
          <td>{{ record.difficulty ?? '-' }}</td>
          <td>{{ record.note ?? '-' }}</td>
          <td>{{ formatDate(record.uploadedAt) }}</td>
          <td :title="record.uploaderId">{{ shortId(record.uploaderId) }}</td>
        </tr>
      </tbody>
    </table>
  </template>
</template>

<style scoped>
.empty-state {
  text-align: center;
  color: var(--color-text-muted);
}

.record-cards {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.record-card {
  padding: 16px;
}

.record-row {
  display: flex;
  justify-content: space-between;
  gap: 12px;
  padding: 4px 0;
  border-bottom: 1px solid var(--color-border);
}

.record-row:last-child {
  border-bottom: none;
}

.record-label {
  font-weight: 600;
  color: var(--color-text-muted);
  flex-shrink: 0;
}

.record-table {
  display: none;
  width: 100%;
  border-collapse: collapse;
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius);
  overflow: hidden;
}

.record-table th,
.record-table td {
  text-align: left;
  padding: 12px 16px;
  border-bottom: 1px solid var(--color-border);
}

.record-table tr:last-child td {
  border-bottom: none;
}

@media (min-width: 768px) {
  .record-cards {
    display: none;
  }

  .record-table {
    display: table;
  }
}
</style>
