<script setup lang="ts">
import type { SessionResponse } from '../types/sessions'

defineProps<{
  records: SessionResponse[]
}>()

const emit = defineEmits<{
  select: [record: SessionResponse]
}>()

function formatDateOnly(value: string): string {
  return new Date(value).toLocaleDateString()
}

function formatGradeCounts(record: SessionResponse): string {
  return [...record.gradeCounts]
    .sort((a, b) => b.grade - a.grade)
    .map((g) => `V${g.grade}: ${g.completedCount}/${g.completedCount + g.uncompletedCount}`)
    .join('、')
}
</script>

<template>
  <div v-if="records.length === 0" class="card empty-state">查無符合條件的抱石紀錄。</div>

  <div v-else class="session-cards">
    <button
      v-for="record in records"
      :key="record.id"
      type="button"
      class="card session-card"
      @click="emit('select', record)"
    >
      <div class="session-card-header">
        <span class="session-date">{{ formatDateOnly(record.date) }}</span>
        <span class="session-gym">{{ record.gymName ?? '-' }}</span>
      </div>
      <div class="session-grades">{{ formatGradeCounts(record) }}</div>
    </button>
  </div>
</template>

<style scoped>
.empty-state {
  text-align: center;
  color: var(--color-text-muted);
}

.session-cards {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.session-card {
  display: flex;
  flex-direction: column;
  gap: 4px;
  text-align: left;
  cursor: pointer;
}

.session-card-header {
  display: flex;
  justify-content: space-between;
  gap: 12px;
  font-weight: 600;
}

.session-grades {
  color: var(--color-text-muted);
  font-size: 0.9rem;
}
</style>
