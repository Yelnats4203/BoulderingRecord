<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { getSessions } from '../api/sessions'
import type { SessionResponse } from '../types/sessions'
import SessionFilterForm from '../components/SessionFilterForm.vue'
import SessionList from '../components/SessionList.vue'
import SessionFormModal from '../components/SessionFormModal.vue'
import { useToastStore } from '../stores/toast'

function formatDateOnly(value: Date): string {
  const year = String(value.getFullYear())
  const month = String(value.getMonth() + 1).padStart(2, '0')
  const day = String(value.getDate()).padStart(2, '0')
  return `${year}-${month}-${day}`
}

function defaultDateFrom(): string {
  const now = new Date()
  return formatDateOnly(new Date(now.getFullYear(), now.getMonth() - 1, now.getDate()))
}

function defaultDateTo(): string {
  return formatDateOnly(new Date())
}

const toastStore = useToastStore()

const records = ref<SessionResponse[]>([])
const isLoading = ref<boolean>(false)
const dateFrom = ref<string>(defaultDateFrom())
const dateTo = ref<string>(defaultDateTo())

const isCreateModalOpen = ref<boolean>(false)
const editingSession = ref<SessionResponse | null>(null)

async function fetchSessions(): Promise<void> {
  isLoading.value = true
  try {
    records.value = await getSessions(dateFrom.value, dateTo.value)
  } catch {
    toastStore.showToast('讀取抱石紀錄清單失敗，請稍後再試。', 'error')
  } finally {
    isLoading.value = false
  }
}

function handleSelect(record: SessionResponse): void {
  editingSession.value = record
}

function handleFormSaved(): void {
  isCreateModalOpen.value = false
  editingSession.value = null
  void fetchSessions()
}

function handleFormDeleted(): void {
  editingSession.value = null
  void fetchSessions()
}

onMounted(() => {
  void fetchSessions()
})
</script>

<template>
  <div class="page session-list-page">
    <div class="page-header">
      <h2>抱石紀錄</h2>
      <button class="btn-primary" type="button" @click="isCreateModalOpen = true">新增抱石紀錄</button>
    </div>

    <SessionFilterForm v-model:date-from="dateFrom" v-model:date-to="dateTo" :is-loading="isLoading" @filter="fetchSessions" />

    <p v-if="isLoading" class="hint-text">載入中...</p>
    <SessionList v-else :records="records" @select="handleSelect" />

    <SessionFormModal v-if="isCreateModalOpen" @close="isCreateModalOpen = false" @saved="handleFormSaved" />

    <SessionFormModal
      v-if="editingSession"
      :session="editingSession"
      @close="editingSession = null"
      @saved="handleFormSaved"
      @deleted="handleFormDeleted"
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

.page-header .btn-primary {
  width: auto;
  white-space: nowrap;
}
</style>
