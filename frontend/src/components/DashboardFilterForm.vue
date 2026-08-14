<script setup lang="ts">
import { ref } from 'vue'
import LoadingSpinner from './LoadingSpinner.vue'

const props = defineProps<{
  isLoading?: boolean
  initialDateFrom: string
  initialDateTo: string
}>()

const emit = defineEmits<{
  filter: [range: { dateFrom: string; dateTo: string }]
}>()

const dateFrom = ref<string>(props.initialDateFrom)
const dateTo = ref<string>(props.initialDateTo)

function handleSubmit(): void {
  if (props.isLoading) {
    return
  }
  emit('filter', { dateFrom: dateFrom.value, dateTo: dateTo.value })
}
</script>

<template>
  <form class="card filter-form" @submit.prevent="handleSubmit">
    <div class="filter-row">
      <div class="form-field">
        <label for="dashboard-filter-date-from">起始日期</label>
        <input id="dashboard-filter-date-from" v-model="dateFrom" type="date" />
      </div>

      <div class="form-field">
        <label for="dashboard-filter-date-to">結束日期</label>
        <input id="dashboard-filter-date-to" v-model="dateTo" type="date" />
      </div>
    </div>

    <button
      class="btn-primary filter-submit"
      :class="{ 'btn-loading': isLoading }"
      type="submit"
      :disabled="isLoading"
    >
      <LoadingSpinner v-if="isLoading" :size="16" />
      <span>{{ isLoading ? '查詢中...' : '查詢' }}</span>
    </button>
  </form>
</template>

<style scoped>
.filter-form {
  margin-bottom: 24px;
}

.filter-row {
  display: grid;
  grid-template-columns: 1fr;
}

.filter-submit {
  width: auto;
  padding: 10px 24px;
}

@media (min-width: 768px) {
  .filter-row {
    grid-template-columns: repeat(2, 1fr);
    gap: 16px;
    align-items: end;
  }

  .filter-row .form-field {
    margin-bottom: 16px;
  }
}
</style>
