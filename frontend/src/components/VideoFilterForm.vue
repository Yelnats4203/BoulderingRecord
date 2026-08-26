<script setup lang="ts">
import { ref } from 'vue'
import type { VideoRecordFilter } from '../types/sends'
import LoadingSpinner from './LoadingSpinner.vue'
import { getDefaultOneMonthRange } from '../utils/dateRange'

const props = defineProps<{
  isLoading?: boolean
}>()

const emit = defineEmits<{
  filter: [filter: Partial<VideoRecordFilter>]
}>()

const defaultRange = getDefaultOneMonthRange()

const gymName = ref<string>('')
const climbAtFrom = ref<string>(defaultRange.climbAtFrom)
const climbAtTo = ref<string>(defaultRange.climbAtTo)
const minDifficulty = ref<string>('')
const maxDifficulty = ref<string>('')

function handleSubmit(): void {
  if (props.isLoading) {
    return
  }
  emit('filter', {
    gymName: gymName.value,
    climbAtFrom: climbAtFrom.value,
    climbAtTo: climbAtTo.value,
    minDifficulty: minDifficulty.value,
    maxDifficulty: maxDifficulty.value,
  })
}
</script>

<template>
  <form class="card filter-form" @submit.prevent="handleSubmit">
    <div class="filter-row">
      <div class="form-field">
        <label for="filter-gym-name">岩館名稱</label>
        <input id="filter-gym-name" v-model="gymName" type="text" placeholder="輸入岩館名稱關鍵字" />
      </div>

      <div class="form-field">
        <label for="filter-climb-at-from">攀爬日期（起）</label>
        <input id="filter-climb-at-from" v-model="climbAtFrom" type="date" />
      </div>

      <div class="form-field">
        <label for="filter-climb-at-to">攀爬日期（迄）</label>
        <input id="filter-climb-at-to" v-model="climbAtTo" type="date" />
      </div>

      <div class="form-field">
        <label for="filter-min-difficulty">難度（最小）</label>
        <input id="filter-min-difficulty" v-model="minDifficulty" type="number" />
      </div>

      <div class="form-field">
        <label for="filter-max-difficulty">難度（最大）</label>
        <input id="filter-max-difficulty" v-model="maxDifficulty" type="number" />
      </div>
    </div>

    <button
      class="btn-primary filter-submit"
      :class="{ 'btn-loading': isLoading }"
      type="submit"
      :disabled="isLoading"
    >
      <LoadingSpinner v-if="isLoading" :size="16" />
      <span>{{ isLoading ? '篩選中...' : '篩選' }}</span>
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
    grid-template-columns: repeat(5, 1fr);
    gap: 16px;
    align-items: end;
  }

  .filter-row .form-field {
    margin-bottom: 16px;
  }
}
</style>
