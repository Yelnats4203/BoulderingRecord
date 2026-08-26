<script setup lang="ts">
import { ref } from 'vue'
import { createSend, getUploadAuthorization, getUploadEligibility, uploadVideoToCloudinary } from '../api/sends'
import { VideoCompressionError } from '../utils/videoCompression'
import { compressVideoWebCodecs } from '../utils/videoCompressionWebCodecs'
import { useToastStore } from '../stores/toast'
import LoadingSpinner from '../components/LoadingSpinner.vue'
import GymNameInput from '../components/GymNameInput.vue'

function todayDateOnly(): string {
  const now = new Date()
  const year = String(now.getFullYear())
  const month = String(now.getMonth() + 1).padStart(2, '0')
  const day = String(now.getDate()).padStart(2, '0')
  return `${year}-${month}-${day}`
}

const toastStore = useToastStore()

const videoFile = ref<File | null>(null)
const climbAt = ref<string>(todayDateOnly())
const gymName = ref<string>('')
const difficulty = ref<string>('')
const note = ref<string>('')
const isPublic = ref<boolean>(true)
const isCompressing = ref<boolean>(false)
const compressionProgress = ref<number>(0)
const isUploading = ref<boolean>(false)

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
    toastStore.showToast('請選擇要上傳的影片檔案。', 'error')
    return
  }

  try {
    const eligibility = await getUploadEligibility()
    if (!eligibility.isAllowed) {
      toastStore.showToast('測試帳號一日僅能上傳5筆。', 'error')
      return
    }
  } catch {
    toastStore.showToast('上傳資格確認失敗，請稍後再試。', 'error')
    return
  }

  isCompressing.value = true
  compressionProgress.value = 0

  let compressedFile: File
  try {
    compressedFile = await compressVideoWebCodecs(videoFile.value, (ratio) => {
      compressionProgress.value = ratio
    })
  } catch (error) {
    if (error instanceof VideoCompressionError && error.code === 'OUTPUT_TOO_LARGE') {
      toastStore.showToast('影片壓縮後仍超過 25MB，請改用較短或較低畫質的影片再試一次。', 'error')
    } else {
      toastStore.showToast('影片壓縮失敗，請確認影片格式後再試一次。', 'error')
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
      climbAt: climbAt.value,
      isPublic: isPublic.value,
    })
    toastStore.showToast('上傳成功。', 'success')
    videoFile.value = null
    climbAt.value = todayDateOnly()
    gymName.value = ''
    difficulty.value = ''
    note.value = ''
    isPublic.value = true
    const fileInput = document.getElementById('video') as HTMLInputElement | null
    if (fileInput) {
      fileInput.value = ''
    }
  } catch {
    toastStore.showToast('上傳失敗，請確認影片格式後再試一次。', 'error')
  } finally {
    isUploading.value = false
  }
}
</script>

<template>
  <div class="page upload-page">
    <form class="card upload-form" @submit.prevent="handleUpload">
      <h2>上傳完攀影片</h2>

      <div class="form-field">
        <label for="video">完攀影片檔案</label>
        <input id="video" type="file" accept="video/mp4,video/quicktime,.mp4,.mov" @change="handleFileChange" required />
      </div>

      <div class="form-field">
        <label for="climbAt">攀爬日期</label>
        <input id="climbAt" v-model="climbAt" type="date" />
      </div>

      <div class="form-field">
        <label for="gymName">岩館名稱（選填）</label>
        <GymNameInput id="gymName" v-model="gymName" />
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

      <div class="form-field form-field-checkbox">
        <label><input v-model="isPublic" type="checkbox" /> 公開影片（好友可見）</label>
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

.form-field-checkbox label {
  display: flex;
  align-items: center;
  gap: 8px;
  font-weight: 600;
}

.form-field-checkbox input {
  width: auto;
  min-height: auto;
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
