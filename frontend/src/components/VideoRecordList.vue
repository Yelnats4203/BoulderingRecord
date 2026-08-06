<script setup lang="ts">
import type { VideoRecordResponse } from '../types/sends'

defineProps<{
  records: VideoRecordResponse[]
}>()

const emit = defineEmits<{
  select: [record: VideoRecordResponse]
}>()

function formatDate(value: string): string {
  return new Date(value).toLocaleString()
}
</script>

<template>
  <div v-if="records.length === 0" class="card empty-state">查無符合條件的影片紀錄。</div>

  <template v-else>
    <div class="video-cards">
      <button
        v-for="record in records"
        :key="record.id"
        type="button"
        class="card video-card"
        @click="emit('select', record)"
      >
        <img class="video-thumbnail" :src="record.thumbnailUrl" alt="影片縮圖" />
        <div class="video-card-info">
          <div class="video-row"><span class="video-label">岩館</span><span>{{ record.gymName ?? '-' }}</span></div>
          <div class="video-row"><span class="video-label">難度</span><span>{{ record.difficulty ?? '-' }}</span></div>
          <div class="video-row"><span class="video-label">上傳時間</span><span>{{ formatDate(record.uploadedAt) }}</span></div>
        </div>
      </button>
    </div>

    <table class="video-table">
      <thead>
        <tr>
          <th>縮圖</th>
          <th>岩館</th>
          <th>難度</th>
          <th>上傳時間</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="record in records" :key="record.id" class="video-table-row" @click="emit('select', record)">
          <td><img class="video-thumbnail-small" :src="record.thumbnailUrl" alt="影片縮圖" /></td>
          <td>{{ record.gymName ?? '-' }}</td>
          <td>{{ record.difficulty ?? '-' }}</td>
          <td>{{ formatDate(record.uploadedAt) }}</td>
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

.video-cards {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.video-card {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 16px;
  text-align: left;
  cursor: pointer;
}

.video-thumbnail {
  width: 96px;
  height: 96px;
  object-fit: cover;
  border-radius: var(--radius);
  flex-shrink: 0;
  background: var(--color-bg);
}

.video-card-info {
  flex: 1;
  min-width: 0;
}

.video-row {
  display: flex;
  justify-content: space-between;
  gap: 12px;
  padding: 2px 0;
}

.video-label {
  font-weight: 600;
  color: var(--color-text-muted);
  flex-shrink: 0;
}

.video-table {
  display: none;
  width: 100%;
  border-collapse: collapse;
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius);
  overflow: hidden;
}

.video-table th,
.video-table td {
  text-align: left;
  padding: 12px 16px;
  border-bottom: 1px solid var(--color-border);
}

.video-table tr:last-child td {
  border-bottom: none;
}

.video-table-row {
  cursor: pointer;
}

.video-table-row:hover {
  background: var(--color-bg);
}

.video-thumbnail-small {
  width: 64px;
  height: 64px;
  object-fit: cover;
  border-radius: var(--radius);
  background: var(--color-bg);
}

@media (min-width: 768px) {
  .video-cards {
    display: none;
  }

  .video-table {
    display: table;
  }
}
</style>
