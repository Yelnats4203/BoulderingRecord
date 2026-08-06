<script setup lang="ts">
import type { VideoRecordResponse } from '../types/sends'

defineProps<{
  record: VideoRecordResponse
}>()

const emit = defineEmits<{
  close: []
}>()

function formatDate(value: string): string {
  return new Date(value).toLocaleString()
}
</script>

<template>
  <div class="modal-overlay" @click.self="emit('close')">
    <div class="card modal-content">
      <div class="modal-header">
        <h2>影片紀錄詳細</h2>
        <button class="btn-secondary" type="button" @click="emit('close')">關閉</button>
      </div>

      <img class="video-thumbnail-large" :src="record.thumbnailUrl" alt="影片縮圖" />

      <div class="video-row"><span class="video-label">岩館</span><span>{{ record.gymName ?? '-' }}</span></div>
      <div class="video-row"><span class="video-label">難度</span><span>{{ record.difficulty ?? '-' }}</span></div>
      <div class="video-row"><span class="video-label">上傳時間</span><span>{{ formatDate(record.uploadedAt) }}</span></div>
      <div class="video-row"><span class="video-label">備註</span><span>{{ record.note ?? '-' }}</span></div>
    </div>
  </div>
</template>

<style scoped>
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 16px;
  z-index: 100;
}

.modal-content {
  width: 100%;
  max-width: 480px;
  max-height: calc(100vh - 32px);
  overflow-y: auto;
}

.modal-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 16px;
}

.modal-header h2 {
  margin: 0;
  font-size: 1.1rem;
}

.video-thumbnail-large {
  width: 100%;
  max-height: 260px;
  object-fit: cover;
  border-radius: var(--radius);
  background: var(--color-bg);
  margin-bottom: 16px;
}

.video-row {
  display: flex;
  justify-content: space-between;
  gap: 12px;
  padding: 8px 0;
  border-bottom: 1px solid var(--color-border);
}

.video-row:last-child {
  border-bottom: none;
}

.video-label {
  font-weight: 600;
  color: var(--color-text-muted);
  flex-shrink: 0;
}
</style>
