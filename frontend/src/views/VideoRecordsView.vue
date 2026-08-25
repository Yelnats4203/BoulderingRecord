<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { getMySends } from '../api/sends'
import type { VideoRecordFilter, VideoRecordResponse } from '../types/sends'
import VideoFilterForm from '../components/VideoFilterForm.vue'
import VideoRecordList from '../components/VideoRecordList.vue'
import VideoRecordDetailModal from '../components/VideoRecordDetailModal.vue'
import { useToastStore } from '../stores/toast'

const toastStore = useToastStore()

const records = ref<VideoRecordResponse[]>([])
const isLoading = ref<boolean>(false)
const selectedRecord = ref<VideoRecordResponse | null>(null)

async function fetchRecords(filter: Partial<VideoRecordFilter> = {}): Promise<void> {
  isLoading.value = true
  try {
    records.value = await getMySends(filter)
  } catch {
    toastStore.showToast('讀取影片紀錄清單失敗，請稍後再試。', 'error')
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
    <div class="page-header">
      <h2>影片紀錄清單</h2>
      <RouterLink class="btn-primary upload-link" :to="{ name: 'upload' }">上傳影片</RouterLink>
    </div>

    <VideoFilterForm :is-loading="isLoading" @filter="fetchRecords" />

    <p v-if="isLoading" class="hint-text">載入中...</p>
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
.page-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
}

.page-header h2 {
  margin: 0;
}

.upload-link {
  width: auto;
  display: inline-block;
  text-decoration: none;
  white-space: nowrap;
}
</style>
