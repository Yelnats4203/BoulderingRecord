<script setup lang="ts">
import type { SendResponse } from '../types/sends'

defineProps<{
  sends: SendResponse[]
}>()

function formatDate(value: string): string {
  return new Date(value).toLocaleString()
}

function shortId(value: string): string {
  return value.slice(0, 8)
}
</script>

<template>
  <div v-if="sends.length === 0" class="card empty-state">目前尚無任何紀錄。</div>

  <template v-else>
    <div class="send-cards">
      <div v-for="send in sends" :key="send.id" class="card send-card">
        <div class="send-row"><span class="send-label">岩館</span><span>{{ send.gymName ?? '-' }}</span></div>
        <div class="send-row"><span class="send-label">難度</span><span>{{ send.difficulty ?? '-' }}</span></div>
        <div class="send-row"><span class="send-label">備註</span><span>{{ send.note ?? '-' }}</span></div>
        <div class="send-row"><span class="send-label">上傳時間</span><span>{{ formatDate(send.uploadedAt) }}</span></div>
        <div class="send-row"><span class="send-label">上傳者</span><span :title="send.uploaderId">{{ shortId(send.uploaderId) }}</span></div>
      </div>
    </div>

    <table class="send-table">
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
        <tr v-for="send in sends" :key="send.id">
          <td>{{ send.gymName ?? '-' }}</td>
          <td>{{ send.difficulty ?? '-' }}</td>
          <td>{{ send.note ?? '-' }}</td>
          <td>{{ formatDate(send.uploadedAt) }}</td>
          <td :title="send.uploaderId">{{ shortId(send.uploaderId) }}</td>
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

.send-cards {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.send-card {
  padding: 16px;
}

.send-row {
  display: flex;
  justify-content: space-between;
  gap: 12px;
  padding: 4px 0;
  border-bottom: 1px solid var(--color-border);
}

.send-row:last-child {
  border-bottom: none;
}

.send-label {
  font-weight: 600;
  color: var(--color-text-muted);
  flex-shrink: 0;
}

.send-table {
  display: none;
  width: 100%;
  border-collapse: collapse;
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius);
  overflow: hidden;
}

.send-table th,
.send-table td {
  text-align: left;
  padding: 12px 16px;
  border-bottom: 1px solid var(--color-border);
}

.send-table tr:last-child td {
  border-bottom: none;
}

@media (min-width: 768px) {
  .send-cards {
    display: none;
  }

  .send-table {
    display: table;
  }
}
</style>
