<script setup lang="ts">
import { ref } from 'vue'
import { createSession } from '../api/sessions'
import LoadingSpinner from '../components/LoadingSpinner.vue'
import GymNameInput from '../components/GymNameInput.vue'

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

const date = ref<string>(todayDateOnly())
const gymName = ref<string>('')
const gradeRows = ref<GradeRow[]>([createEmptyGradeRow()])
const isSubmitting = ref<boolean>(false)
const errorMessage = ref<string>('')
const successMessage = ref<string>('')

function addGradeRow(): void {
  gradeRows.value.push(createEmptyGradeRow())
}

function removeGradeRow(index: number): void {
  gradeRows.value.splice(index, 1)
}

async function handleSubmit(): Promise<void> {
  errorMessage.value = ''
  successMessage.value = ''

  const gradeCounts = gradeRows.value
    .filter((row) => row.grade !== '')
    .map((row) => ({
      grade: Number(row.grade),
      completedCount: Number(row.completedCount) || 0,
      uncompletedCount: Number(row.uncompletedCount) || 0,
    }))

  if (gradeCounts.length === 0) {
    errorMessage.value = '請至少輸入一個難度的攀爬次數統計。'
    return
  }

  isSubmitting.value = true
  try {
    await createSession({ date: date.value, gymName: gymName.value, gradeCounts })
    successMessage.value = '抱石紀錄建立成功。'
    date.value = todayDateOnly()
    gymName.value = ''
    gradeRows.value = [createEmptyGradeRow()]
  } catch {
    errorMessage.value = '建立失敗，請確認輸入內容後再試一次。'
  } finally {
    isSubmitting.value = false
  }
}
</script>

<template>
  <div class="page create-session-page">
    <form class="card create-session-form" @submit.prevent="handleSubmit">
      <h2>新增抱石紀錄</h2>

      <p v-if="errorMessage" class="error-text">{{ errorMessage }}</p>
      <p v-if="successMessage" class="success-text">{{ successMessage }}</p>

      <div class="form-field">
        <label for="date">日期</label>
        <input id="date" v-model="date" type="date" required />
      </div>

      <div class="form-field">
        <label for="gymName">岩館名稱</label>
        <GymNameInput id="gymName" v-model="gymName" placeholder="選填" />
      </div>

      <div class="grade-rows">
        <div v-for="(row, index) in gradeRows" :key="index" class="grade-row">
          <div class="form-field">
            <label :for="`grade-${index}`">難度（V）</label>
            <input :id="`grade-${index}`" v-model="row.grade" type="number" min="0" required />
          </div>

          <div class="form-field">
            <label :for="`completed-${index}`">完攀數</label>
            <input :id="`completed-${index}`" v-model="row.completedCount" type="number" min="0" />
          </div>

          <div class="form-field">
            <label :for="`uncompleted-${index}`">未完攀數</label>
            <input :id="`uncompleted-${index}`" v-model="row.uncompletedCount" type="number" min="0" />
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

      <button
        class="btn-primary submit-btn"
        :class="{ 'btn-loading': isSubmitting }"
        type="submit"
        :disabled="isSubmitting"
      >
        <LoadingSpinner v-if="isSubmitting" :size="16" />
        <span>{{ isSubmitting ? '建立中...' : '建立紀錄' }}</span>
      </button>
    </form>
  </div>
</template>

<style scoped>
.create-session-form {
  max-width: 640px;
  margin: 0 auto 24px;
}

.create-session-form h2 {
  margin-top: 0;
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

.submit-btn {
  width: 100%;
}

.success-text {
  color: #16a34a;
  font-size: 0.9rem;
  margin: 0 0 12px;
}

@media (min-width: 768px) {
  .grade-row {
    grid-template-columns: repeat(3, 1fr) auto;
    align-items: end;
  }
}
</style>
