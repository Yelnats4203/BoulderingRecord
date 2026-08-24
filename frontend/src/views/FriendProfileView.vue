<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRoute } from 'vue-router'
import { getFriendVideos } from '../api/friends'
import type { VideoRecordResponse } from '../types/sends'
import VideoRecordList from '../components/VideoRecordList.vue'
import VideoRecordDetailModal from '../components/VideoRecordDetailModal.vue'

const route = useRoute()
const userId = route.params.userId as string
const friendUsername = typeof route.query.username === 'string' ? route.query.username : ''

const records = ref<VideoRecordResponse[]>([])
const isLoading = ref<boolean>(false)
const errorMessage = ref<string>('')
const selectedRecord = ref<VideoRecordResponse | null>(null)

async function fetchRecords(): Promise<void> {
  isLoading.value = true
  errorMessage.value = ''
  try {
    records.value = await getFriendVideos(userId)
  } catch {
    errorMessage.value = '讀取好友影片失敗，請稍後再試。'
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
      <h2>{{ friendUsername || '好友' }} 的影片</h2>
    </div>

    <p v-if="errorMessage" class="error-text">{{ errorMessage }}</p>
    <p v-else-if="isLoading" class="hint-text">載入中...</p>
    <VideoRecordList v-else :records="records" @select="handleSelect" />

    <VideoRecordDetailModal v-if="selectedRecord" :record="selectedRecord" :readonly="true" @close="handleCloseDetail" />
  </div>
</template>
