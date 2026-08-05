<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { logout as logoutApi } from '../api/auth'
import { createSend, getAllSends, getUploadAuthorization, uploadVideoToCloudinary } from '../api/sends'
import { useAuthStore } from '../stores/auth'
import type { SendResponse } from '../types/sends'
import SendList from '../components/SendList.vue'

const authStore = useAuthStore()
const router = useRouter()

const sends = ref<SendResponse[]>([])
const isLoadingSends = ref<boolean>(false)
const listErrorMessage = ref<string>('')

const videoFile = ref<File | null>(null)
const gymName = ref<string>('')
const difficulty = ref<string>('')
const note = ref<string>('')
const isUploading = ref<boolean>(false)
const uploadErrorMessage = ref<string>('')
const uploadSuccessMessage = ref<string>('')

function handleFileChange(event: Event): void {
  const input = event.target as HTMLInputElement
  videoFile.value = input.files && input.files.length > 0 ? input.files[0] : null
}

async function fetchSends(): Promise<void> {
  isLoadingSends.value = true
  listErrorMessage.value = ''
  try {
    sends.value = await getAllSends()
  } catch {
    listErrorMessage.value = '讀取紀錄列表失敗，請稍後再試。'
  } finally {
    isLoadingSends.value = false
  }
}

async function handleUpload(): Promise<void> {
  if (!videoFile.value) {
    uploadErrorMessage.value = '請選擇要上傳的影片檔案。'
    return
  }

  uploadErrorMessage.value = ''
  uploadSuccessMessage.value = ''
  isUploading.value = true
  try {
    const auth = await getUploadAuthorization()
    await uploadVideoToCloudinary(videoFile.value, auth)
    await createSend({
      sendId: auth.sendId,
      gymName: gymName.value,
      difficulty: difficulty.value,
      note: note.value,
    })
    uploadSuccessMessage.value = '上傳成功。'
    videoFile.value = null
    gymName.value = ''
    difficulty.value = ''
    note.value = ''
    const fileInput = document.getElementById('video') as HTMLInputElement | null
    if (fileInput) {
      fileInput.value = ''
    }
    await fetchSends()
  } catch {
    uploadErrorMessage.value = '上傳失敗，請確認影片格式後再試一次。'
  } finally {
    isUploading.value = false
  }
}

async function handleLogout(): Promise<void> {
  try {
    await logoutApi()
  } finally {
    authStore.clearSession()
    await router.push({ name: 'login' })
  }
}

onMounted(() => {
  void fetchSends()
})
</script>

<template>
  <div class="page upload-page">
    <header class="page-header">
      <h1>攀岩紀錄</h1>
      <button class="btn-secondary" type="button" @click="handleLogout">登出</button>
    </header>

    <form class="card upload-form" @submit.prevent="handleUpload">
      <h2>上傳影片</h2>

      <p v-if="uploadErrorMessage" class="error-text">{{ uploadErrorMessage }}</p>
      <p v-if="uploadSuccessMessage" class="success-text">{{ uploadSuccessMessage }}</p>

      <div class="form-field">
        <label for="video">影片檔案</label>
        <input id="video" type="file" accept="video/mp4,video/quicktime,.mp4,.mov" @change="handleFileChange" required />
      </div>

      <div class="form-field">
        <label for="gymName">岩館名稱（選填）</label>
        <input id="gymName" v-model="gymName" type="text" />
      </div>

      <div class="form-field">
        <label for="difficulty">難度（選填）</label>
        <input id="difficulty" v-model="difficulty" type="number" />
      </div>

      <div class="form-field">
        <label for="note">備註（選填）</label>
        <textarea id="note" v-model="note"></textarea>
      </div>

      <button class="btn-primary" type="submit" :disabled="isUploading">
        {{ isUploading ? '上傳中...' : '上傳' }}
      </button>
    </form>

    <section class="send-section">
      <h2>所有使用者的紀錄</h2>
      <p v-if="listErrorMessage" class="error-text">{{ listErrorMessage }}</p>
      <p v-else-if="isLoadingSends" class="hint-text">載入中...</p>
      <SendList v-else :sends="sends" />
    </section>
  </div>
</template>

<style scoped>
.page-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 16px;
}

.page-header h1 {
  font-size: 1.5rem;
  margin: 0;
}

.upload-form {
  max-width: 480px;
  margin: 0 auto 24px;
}

.upload-form h2 {
  margin-top: 0;
}

.success-text {
  color: #16a34a;
  font-size: 0.9rem;
  margin: 0 0 12px;
}

.send-section h2 {
  font-size: 1.1rem;
}
</style>
