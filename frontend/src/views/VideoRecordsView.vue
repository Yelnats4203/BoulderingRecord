<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { getMySends } from '../api/sends'
import type { VideoRecordFilter, VideoRecordResponse } from '../types/sends'
import VideoFilterForm from '../components/VideoFilterForm.vue'
import VideoRecordList from '../components/VideoRecordList.vue'
import VideoRecordDetailModal from '../components/VideoRecordDetailModal.vue'

const records = ref<VideoRecordResponse[]>([])
const isLoading = ref<boolean>(false)
const errorMessage = ref<string>('')
const selectedRecord = ref<VideoRecordResponse | null>(null)

async function fetchRecords(filter: Partial<VideoRecordFilter> = {}): Promise<void> {
  isLoading.value = true
  errorMessage.value = ''
  try {
    records.value = await getMySends(filter)
  } catch {
    errorMessage.value = '讀取影片紀錄清單失敗，請稍後再試。'
  } finally {
    isLoading.value = false
  }
}

function handleSelect(record: VideoRecordResponse): void {
  selectedRecord.value = record
}

function handleCloseDetail(): void {
  selectedRecord.value = null
}

function handleUpdated(record: VideoRecordResponse): void {
  const index = records.value.findIndex((r) => r.id === record.id)
  if (index !== -1) {
    records.value[index] = record
  }
  selectedRecord.value = record
}

function handleDeleted(id: string): void {
  records.value = records.value.filter((r) => r.id !== id)
  selectedRecord.value = null
}

onMounted(() => {
  void fetchRecords()
})
</script>

<template>
  <div class="page video-records-page">
    <h2>影片紀錄清單</h2>

    <VideoFilterForm :is-loading="isLoading" @filter="fetchRecords" />

    <p v-if="errorMessage" class="error-text">{{ errorMessage }}</p>
    <p v-else-if="isLoading" class="hint-text">載入中...</p>
    <VideoRecordList v-else :records="records" @select="handleSelect" />

    <VideoRecordDetailModal
      v-if="selectedRecord"
      :record="selectedRecord"
      @close="handleCloseDetail"
      @updated="handleUpdated"
      @deleted="handleDeleted"
    />
  </div>
</template>

<style scoped>
.video-records-page h2 {
  margin-top: 0;
}
</style>
