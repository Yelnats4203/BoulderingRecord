<script setup lang="ts">
import { ref, watch } from 'vue'
import { deleteSend, getSendVideo, updateSend } from '../api/sends'
import { useVideoPlaybackCacheStore } from '../stores/videoPlaybackCache'
import type { VideoRecordResponse } from '../types/sends'
import ConfirmDialog from './ConfirmDialog.vue'

const props = defineProps<{
  record: VideoRecordResponse
}>()

const emit = defineEmits<{
  close: []
  updated: [record: VideoRecordResponse]
  deleted: [id: string]
}>()

const videoPlaybackCache = useVideoPlaybackCacheStore()

const playbackUrl = ref<string>('')
const isVideoLoading = ref<boolean>(false)
const videoErrorMessage = ref<string>('')

watch(
  () => props.record.id,
  async (id) => {
    videoErrorMessage.value = ''

    const cachedUrl = videoPlaybackCache.get(id)
    if (cachedUrl) {
      playbackUrl.value = cachedUrl
      return
    }

    playbackUrl.value = ''
    isVideoLoading.value = true
    try {
      const video = await getSendVideo(id)
      playbackUrl.value = video.playbackUrl
      videoPlaybackCache.set(id, video.playbackUrl)
    } catch {
      videoErrorMessage.value = '影片載入失敗，請稍後再試。'
    } finally {
      isVideoLoading.value = false
    }
  },
  { immediate: true },
)

function formatDate(value: string): string {
  return new Date(value).toLocaleString()
}

function toDateTimeLocalValue(value: string): string {
  const date = new Date(value)
  const pad = (n: number): string => String(n).padStart(2, '0')
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}`
}

const isEditing = ref<boolean>(false)
const isSaving = ref<boolean>(false)
const errorMessage = ref<string>('')
const uploadedAt = ref<string>(toDateTimeLocalValue(props.record.uploadedAt))
const gymName = ref<string>(props.record.gymName ?? '')
const difficulty = ref<string>(props.record.difficulty === null ? '' : String(props.record.difficulty))
const note = ref<string>(props.record.note ?? '')

function startEditing(): void {
  uploadedAt.value = toDateTimeLocalValue(props.record.uploadedAt)
  gymName.value = props.record.gymName ?? ''
  difficulty.value = props.record.difficulty === null ? '' : String(props.record.difficulty)
  note.value = props.record.note ?? ''
  errorMessage.value = ''
  isEditing.value = true
}

function cancelEditing(): void {
  isEditing.value = false
}

async function handleSave(): Promise<void> {
  errorMessage.value = ''
  isSaving.value = true
  try {
    const updated = await updateSend(props.record.id, {
      uploadedAt: new Date(uploadedAt.value).toISOString(),
      gymName: gymName.value,
      difficulty: difficulty.value,
      note: note.value,
    })
    emit('updated', {
      ...props.record,
      gymName: updated.gymName,
      uploadedAt: updated.uploadedAt,
      difficulty: updated.difficulty,
      note: updated.note,
    })
    isEditing.value = false
  } catch {
    errorMessage.value = '更新失敗，請稍後再試。'
  } finally {
    isSaving.value = false
  }
}

const isConfirmingDelete = ref<boolean>(false)
const isDeleting = ref<boolean>(false)

async function handleConfirmDelete(): Promise<void> {
  isDeleting.value = true
  try {
    await deleteSend(props.record.id)
    isConfirmingDelete.value = false
    emit('deleted', props.record.id)
  } catch {
    errorMessage.value = '刪除失敗，請稍後再試。'
    isConfirmingDelete.value = false
  } finally {
    isDeleting.value = false
  }
}
</script>

<template>
  <div class="modal-overlay" @click.self="emit('close')">
    <div class="card modal-content">
      <div class="modal-header">
        <h2>影片紀錄詳細</h2>
        <button class="btn-secondary" type="button" @click="emit('close')">關閉</button>
      </div>

      <video
        v-if="playbackUrl"
        class="video-player"
        controls
        :poster="record.thumbnailUrl"
        :src="playbackUrl"
      ></video>
      <img v-else class="video-thumbnail-large" :src="record.thumbnailUrl" alt="影片縮圖" />
      <p v-if="isVideoLoading" class="hint-text">影片載入中...</p>
      <p v-if="videoErrorMessage" class="error-text">{{ videoErrorMessage }}</p>

      <p v-if="errorMessage" class="error-text">{{ errorMessage }}</p>

      <form v-if="isEditing" class="edit-form" @submit.prevent="handleSave">
        <div class="form-field">
          <label for="edit-uploaded-at">上傳時間</label>
          <input id="edit-uploaded-at" v-model="uploadedAt" type="datetime-local" required />
        </div>
        <div class="form-field">
          <label for="edit-gym-name">岩館（選填）</label>
          <input id="edit-gym-name" v-model="gymName" type="text" />
        </div>
        <div class="form-field">
          <label for="edit-difficulty">難度（選填）</label>
          <input id="edit-difficulty" v-model="difficulty" type="number" />
        </div>
        <div class="form-field">
          <label for="edit-note">備註（選填）</label>
          <textarea id="edit-note" v-model="note"></textarea>
        </div>
        <div class="edit-actions">
          <button class="btn-secondary" type="button" :disabled="isSaving" @click="cancelEditing">取消</button>
          <button class="btn-primary" type="submit" :disabled="isSaving">{{ isSaving ? '儲存中...' : '儲存' }}</button>
        </div>
      </form>

      <template v-else>
        <div class="video-row"><span class="video-label">岩館</span><span>{{ record.gymName ?? '-' }}</span></div>
        <div class="video-row"><span class="video-label">難度</span><span>{{ record.difficulty ?? '-' }}</span></div>
        <div class="video-row"><span class="video-label">上傳時間</span><span>{{ formatDate(record.uploadedAt) }}</span></div>
        <div class="video-row"><span class="video-label">備註</span><span>{{ record.note ?? '-' }}</span></div>

        <div class="detail-actions">
          <button class="btn-secondary" type="button" @click="startEditing">編輯</button>
          <button class="btn-danger" type="button" @click="isConfirmingDelete = true">刪除</button>
        </div>
      </template>
    </div>

    <ConfirmDialog
      v-if="isConfirmingDelete"
      title="刪除影片紀錄"
      message="刪除後將無法復原，且會一併刪除 Cloudinary 上的影片，確定要刪除嗎？"
      :confirm-disabled="isDeleting"
      @confirm="handleConfirmDelete"
      @cancel="isConfirmingDelete = false"
    />
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

.video-player {
  width: 100%;
  max-height: 360px;
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

.detail-actions {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  margin-top: 16px;
}

.edit-form .form-field:last-of-type {
  margin-bottom: 0;
}

.edit-actions {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  margin-top: 8px;
}
</style>
