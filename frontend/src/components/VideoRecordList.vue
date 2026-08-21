<script setup lang="ts">
import type { VideoRecordResponse } from '../types/sends'

defineProps<{
  records: VideoRecordResponse[]
}>()

const emit = defineEmits<{
  select: [record: VideoRecordResponse]
}>()

function formatDifficulty(value: number | null): string {
  return value === null ? '-' : `V${value}`
}

function formatDateOnly(value: string): string {
  return new Date(value).toLocaleDateString()
}
</script>

<template>
  <div v-if="records.length === 0" class="card empty-state">查無符合條件的影片紀錄。</div>

  <div v-else class="video-cards">
    <button
      v-for="record in records"
      :key="record.id"
      type="button"
      class="card video-card"
      @click="emit('select', record)"
    >
      <img class="video-thumbnail" :src="record.thumbnailUrl" alt="影片縮圖" />
      <div class="video-card-caption">
        {{ record.gymName ?? '-' }} - {{ formatDifficulty(record.difficulty) }} - {{ formatDateOnly(record.climbAt) }}
      </div>
    </button>
  </div>
</template>

<style scoped>
.empty-state {
  text-align: center;
  color: var(--color-text-muted);
}

.video-cards {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(160px, 1fr));
  gap: 16px;
}

.video-card {
  display: flex;
  flex-direction: column;
  aspect-ratio: 3 / 4;
  padding: 0;
  overflow: hidden;
  text-align: left;
  cursor: pointer;
}

.video-thumbnail {
  width: 100%;
  height: 80%;
  object-fit: cover;
  flex-shrink: 0;
  background: var(--color-bg);
}

.video-card-caption {
  height: 20%;
  display: flex;
  align-items: center;
  padding: 0 10px;
  font-size: 0.85rem;
  font-weight: 600;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
</style>
