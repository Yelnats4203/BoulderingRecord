<script setup lang="ts">
import LoadingSpinner from './LoadingSpinner.vue'

defineProps<{
  isLoading?: boolean
}>()

const dateFrom = defineModel<string>('dateFrom', { required: true })
const dateTo = defineModel<string>('dateTo', { required: true })

const emit = defineEmits<{
  filter: []
}>()
</script>

<template>
  <form class="card filter-form" @submit.prevent="emit('filter')">
    <div class="filter-row">
      <div class="form-field">
        <label for="session-filter-date-from">日期（起）</label>
        <input id="session-filter-date-from" v-model="dateFrom" type="date" />
      </div>

      <div class="form-field">
        <label for="session-filter-date-to">日期（迄）</label>
        <input id="session-filter-date-to" v-model="dateTo" type="date" />
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
