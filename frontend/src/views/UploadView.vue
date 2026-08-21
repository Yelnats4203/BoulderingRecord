<script setup lang="ts">
import { ref } from 'vue'
import { createSend, getUploadAuthorization, uploadVideoToCloudinary } from '../api/sends'
import { VideoCompressionError } from '../utils/videoCompression'
import { compressVideoWebCodecs } from '../utils/videoCompressionWebCodecs'
import LoadingSpinner from '../components/LoadingSpinner.vue'

function todayDateOnly(): string {
  const now = new Date()
  const year = String(now.getFullYear())
  const month = String(now.getMonth() + 1).padStart(2, '0')
  const day = String(now.getDate()).padStart(2, '0')
  return `${year}-${month}-${day}`
}

const videoFile = ref<File | null>(null)
const uploadedAt = ref<string>(todayDateOnly())
const gymName = ref<string>('')
const difficulty = ref<string>('')
const note = ref<string>('')
const isCompressing = ref<boolean>(false)
const compressionProgress = ref<number>(0)
const isUploading = ref<boolean>(false)
const uploadErrorMessage = ref<string>('')
const uploadSuccessMessage = ref<string>('')

function handleFileChange(event: Event): void {
  const input = event.target as HTMLInputElement
  videoFile.value = input.files && input.files.length > 0 ? input.files[0] : null
}

function sanitizeDifficulty(): void {
  if (difficulty.value === '') {
    return
  }
  const parsed: number = Math.round(Number(difficulty.value))
  if (Number.isNaN(parsed)) {
    difficulty.value = ''
    return
  }
  difficulty.value = String(Math.min(10, Math.max(0, parsed)))
}

async function handleUpload(): Promise<void> {
  if (!videoFile.value) {
    uploadErrorMessage.value = '請選擇要上傳的影片檔案。'
    return
  }

  uploadErrorMessage.value = ''
  uploadSuccessMessage.value = ''
  isCompressing.value = true
  compressionProgress.value = 0

  let compressedFile: File
  try {
    compressedFile = await compressVideoWebCodecs(videoFile.value, (ratio) => {
      compressionProgress.value = ratio
    })
  } catch (error) {
    if (error instanceof VideoCompressionError && error.code === 'OUTPUT_TOO_LARGE') {
      uploadErrorMessage.value = '影片壓縮後仍超過 25MB，請改用較短或較低畫質的影片再試一次。'
    } else {
      uploadErrorMessage.value = '影片壓縮失敗，請確認影片格式後再試一次。'
    }
    isCompressing.value = false
    return
  }
  isCompressing.value = false

  isUploading.value = true
  try {
    const auth = await getUploadAuthorization()
    await uploadVideoToCloudinary(compressedFile, auth)
    await createSend({
      sendId: auth.sendId,
      gymName: gymName.value,
      difficulty: difficulty.value,
      note: note.value,
      uploadedAt: uploadedAt.value,
    })
    uploadSuccessMessage.value = '上傳成功。'
    videoFile.value = null
    uploadedAt.value = todayDateOnly()
    gymName.value = ''
    difficulty.value = ''
    note.value = ''
    const fileInput = document.getElementById('video') as HTMLInputElement | null
    if (fileInput) {
      fileInput.value = ''
    }
  } catch {
    uploadErrorMessage.value = '上傳失敗，請確認影片格式後再試一次。'
  } finally {
    isUploading.value = false
  }
}
</script>

<template>
  <div class="page upload-page">
    <form class="card upload-form" @submit.prevent="handleUpload">
      <h2>上傳影片</h2>

      <p v-if="uploadErrorMessage" class="error-text">{{ uploadErrorMessage }}</p>
      <p v-if="uploadSuccessMessage" class="success-text">{{ uploadSuccessMessage }}</p>

      <div class="form-field">
        <label for="video">影片檔案</label>
        <input id="video" type="file" accept="video/mp4,video/quicktime,.mp4,.mov" @change="handleFileChange" required />
      </div>

      <div class="form-field">
        <label for="uploadedAt">日期</label>
        <input id="uploadedAt" v-model="uploadedAt" type="date" />
      </div>

      <div class="form-field">
        <label for="gymName">岩館名稱（選填）</label>
        <input id="gymName" v-model="gymName" type="text" />
      </div>

      <div class="form-field">
        <label for="difficulty">難度（選填）</label>
        <div class="difficulty-input-wrapper">
          <span class="difficulty-prefix">V</span>
          <input
            id="difficulty"
            v-model="difficulty"
            type="number"
            list="difficulty-options"
            min="0"
            max="10"
            step="1"
            @blur="sanitizeDifficulty"
          />
        </div>
        <datalist id="difficulty-options">
          <option v-for="n in 11" :key="n" :value="n - 1">V{{ n - 1 }}</option>
        </datalist>
      </div>

      <div class="form-field">
        <label for="note">備註（選填）</label>
        <textarea id="note" v-model="note" placeholder="可輸入Crux等等"></textarea>
      </div>

      <button
        class="btn-primary"
        :class="{ 'btn-loading': isCompressing || isUploading }"
        type="submit"
        :disabled="isCompressing || isUploading"
      >
        <LoadingSpinner v-if="isCompressing || isUploading" :size="16" />
        <span v-if="isCompressing">壓縮中{{ compressionProgress > 0 ? `（${Math.round(compressionProgress * 100)}%）` : '...' }}</span>
        <span v-else-if="isUploading">上傳中...</span>
        <span v-else>上傳</span>
      </button>
    </form>
  </div>
</template>

<style scoped>
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

.difficulty-input-wrapper {
  position: relative;
}

.difficulty-input-wrapper input {
  padding-left: 28px;
}

.difficulty-prefix {
  position: absolute;
  top: 50%;
  left: 12px;
  transform: translateY(-50%);
  color: var(--color-text-muted);
  font-weight: 600;
  pointer-events: none;
}
</style>
