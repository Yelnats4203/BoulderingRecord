<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRoute } from 'vue-router'
import { getFriendVideos } from '../api/friends'
import type { VideoRecordResponse } from '../types/sends'
import VideoRecordList from '../components/VideoRecordList.vue'
import VideoRecordDetailModal from '../components/VideoRecordDetailModal.vue'
import { useToastStore } from '../stores/toast'

const route = useRoute()
const toastStore = useToastStore()
const userId = route.params.userId as string
const friendUsername = typeof route.query.username === 'string' ? route.query.username : ''

const records = ref<VideoRecordResponse[]>([])
const isLoading = ref<boolean>(false)
const selectedRecord = ref<VideoRecordResponse | null>(null)

async function fetchRecords(): Promise<void> {
  isLoading.value = true
  try {
    records.value = await getFriendVideos(userId)
  } catch {
    toastStore.showToast('讀取好友影片失敗，請稍後再試。', 'error')
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

onMounted(() => {
  void fetchRecords()
})
</script>

<template>
  <div class="page friend-profile-page">
    <div class="page-header">
      <h2>{{ friendUsername || '好友' }} 的完攀影片</h2>
    </div>

    <p v-if="isLoading" class="hint-text">載入中...</p>
    <VideoRecordList v-else :records="records" @select="handleSelect" />

    <VideoRecordDetailModal v-if="selectedRecord" :record="selectedRecord" :readonly="true" @close="handleCloseDetail" />
  </div>
</template>
