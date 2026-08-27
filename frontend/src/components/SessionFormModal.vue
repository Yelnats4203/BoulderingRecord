<script setup lang="ts">
import { ref } from 'vue'
import { createSession, deleteSession, updateSession } from '../api/sessions'
import { useToastStore } from '../stores/toast'
import type { SessionResponse } from '../types/sessions'
import ConfirmDialog from './ConfirmDialog.vue'
import LoadingSpinner from './LoadingSpinner.vue'
import GymNameInput from './GymNameInput.vue'

const props = defineProps<{
  session?: SessionResponse | null
}>()

const emit = defineEmits<{
  close: []
  saved: []
  deleted: []
}>()

interface GradeRow {
  grade: string
  completedCount: string
  uncompletedCount: string
}

function todayDateOnly(): string {
  const now = new Date()
  const year = String(now.getFullYear())
  const month = String(now.getMonth() + 1).padStart(2, '0')
  const day = String(now.getDate()).padStart(2, '0')
  return `${year}-${month}-${day}`
}

function createEmptyGradeRow(): GradeRow {
  return { grade: '', completedCount: '0', uncompletedCount: '0' }
}

function gradeRowsFromSession(session: SessionResponse): GradeRow[] {
  if (session.gradeCounts.length === 0) {
    return [createEmptyGradeRow()]
  }
  return session.gradeCounts.map((g) => ({
    grade: String(g.grade),
    completedCount: String(g.completedCount),
    uncompletedCount: String(g.uncompletedCount),
  }))
}

const isEditMode: boolean = !!props.session

const toastStore = useToastStore()

const date = ref<string>(props.session?.date ?? todayDateOnly())
const gymName = ref<string>(props.session?.gymName ?? '')
const gradeRows = ref<GradeRow[]>(props.session ? gradeRowsFromSession(props.session) : [createEmptyGradeRow()])
const isSubmitting = ref<boolean>(false)

function addGradeRow(): void {
  gradeRows.value.push(createEmptyGradeRow())
}

function removeGradeRow(index: number): void {
  gradeRows.value.splice(index, 1)
}

async function handleSubmit(): Promise<void> {
  if (isSubmitting.value) {
    return
  }

  const gradeCounts = gradeRows.value
    .filter((row) => row.grade !== '')
    .map((row) => ({
      grade: Number(row.grade),
      completedCount: Number(row.completedCount) || 0,
      uncompletedCount: Number(row.uncompletedCount) || 0,
    }))
    .filter((row) => row.completedCount !== 0 || row.uncompletedCount !== 0)

  if (gradeCounts.length === 0) {
    toastStore.showToast('請至少輸入一個難度的攀爬次數統計。', 'error')
    return
  }

  isSubmitting.value = true
  try {
    if (props.session) {
      await updateSession(props.session.id, { date: date.value, gymName: gymName.value, gradeCounts })
      toastStore.showToast('抱石紀錄更新成功。', 'success')
    } else {
      await createSession({ date: date.value, gymName: gymName.value, gradeCounts })
      toastStore.showToast('抱石紀錄建立成功。', 'success')
    }
    emit('saved')
  } catch {
    toastStore.showToast(props.session ? '更新失敗，請確認輸入內容後再試一次。' : '建立失敗，請確認輸入內容後再試一次。', 'error')
  } finally {
    isSubmitting.value = false
  }
}

const isConfirmingDelete = ref<boolean>(false)
const isDeleting = ref<boolean>(false)

async function handleConfirmDelete(): Promise<void> {
  if (!props.session) {
    return
  }
  isDeleting.value = true
  try {
    await deleteSession(props.session.id)
    toastStore.showToast('抱石紀錄刪除成功。', 'success')
    isConfirmingDelete.value = false
    emit('deleted')
  } catch {
    toastStore.showToast('刪除失敗，請稍後再試。', 'error')
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
        <h2>{{ isEditMode ? '編輯抱石紀錄' : '新增抱石紀錄' }}</h2>
        <button class="btn-secondary" type="button" @click="emit('close')">關閉</button>
      </div>

      <form class="session-form" @submit.prevent="handleSubmit">
        <fieldset class="session-fieldset" :disabled="isSubmitting">
          <div class="form-field">
            <label for="session-date">日期</label>
            <input id="session-date" v-model="date" type="date" required />
          </div>

          <div class="form-field">
            <label for="session-gym-name">岩館名稱</label>
            <GymNameInput id="session-gym-name" v-model="gymName" placeholder="選填" />
          </div>

          <div class="grade-rows">
            <div v-for="(row, index) in gradeRows" :key="index" class="grade-row">
              <div class="form-field">
                <label :for="`session-grade-${index}`">難度（V）</label>
                <input :id="`session-grade-${index}`" v-model="row.grade" type="number" min="0" required />
              </div>

              <div class="form-field">
                <label :for="`session-completed-${index}`">完攀數</label>
                <input :id="`session-completed-${index}`" v-model="row.completedCount" type="number" min="0" />
              </div>

              <div class="form-field">
                <label :for="`session-uncompleted-${index}`">未完攀數</label>
                <input :id="`session-uncompleted-${index}`" v-model="row.uncompletedCount" type="number" min="0" />
              </div>

              <button
                type="button"
                class="btn-secondary remove-grade-btn"
                :disabled="gradeRows.length === 1"
                @click="removeGradeRow(index)"
              >
                移除
              </button>
            </div>
          </div>

          <button type="button" class="btn-secondary add-grade-btn" @click="addGradeRow">新增難度</button>
        </fieldset>

        <div class="form-actions">
          <button
            v-if="isEditMode"
            class="btn-danger"
            type="button"
            :disabled="isSubmitting"
            @click="isConfirmingDelete = true"
          >
            刪除
          </button>
          <button
            class="btn-primary submit-btn"
            :class="{ 'btn-loading': isSubmitting }"
            type="submit"
            :disabled="isSubmitting"
          >
            <LoadingSpinner v-if="isSubmitting" :size="16" />
            <span>{{ isSubmitting ? '儲存中...' : isEditMode ? '儲存' : '建立紀錄' }}</span>
          </button>
        </div>
      </form>
    </div>

    <ConfirmDialog
      v-if="isConfirmingDelete"
      title="刪除抱石紀錄"
      message="刪除後將無法復原，確定要刪除嗎？"
      :confirm-disabled="isDeleting"
      :confirm-loading="isDeleting"
      confirm-loading-text="刪除中..."
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
  max-width: 640px;
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

.session-fieldset {
  border: 0;
  margin: 0;
  padding: 0;
  min-width: 0;
}

.grade-rows {
  display: flex;
  flex-direction: column;
  gap: 8px;
  margin-bottom: 16px;
}

.grade-row {
  display: grid;
  grid-template-columns: 1fr;
  gap: 8px;
  padding: 12px;
  border: 1px solid var(--color-border);
  border-radius: var(--radius);
}

.grade-row .form-field {
  margin-bottom: 0;
}

.remove-grade-btn {
  width: auto;
  justify-self: start;
}

.add-grade-btn {
  width: auto;
  margin-bottom: 24px;
}

.form-actions {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
}

.submit-btn {
  width: auto;
}

@media (min-width: 768px) {
  .grade-row {
    grid-template-columns: repeat(3, 1fr) auto;
    align-items: end;
  }
}
</style>
