<script setup lang="ts">
import { ref } from 'vue'

defineProps<{
  id: string
  autocomplete?: string
}>()

const modelValue = defineModel<string>({ required: true })
const showPassword = ref<boolean>(false)
</script>

<template>
  <div class="password-input-wrapper">
    <input
      :id="id"
      v-model="modelValue"
      :type="showPassword ? 'text' : 'password'"
      :autocomplete="autocomplete"
      required
    />
    <button
      type="button"
      class="password-toggle"
      :aria-label="showPassword ? '隱藏密碼' : '顯示密碼'"
      @click="showPassword = !showPassword"
    >
      <svg v-if="showPassword" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
        <path d="M17.94 17.94A10.94 10.94 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94M9.9 4.24A10.94 10.94 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19m-6.72-1.07a3 3 0 1 1-4.24-4.24" />
        <line x1="1" y1="1" x2="23" y2="23" />
      </svg>
      <svg v-else viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
        <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z" />
        <circle cx="12" cy="12" r="3" />
      </svg>
    </button>
  </div>
</template>

<style scoped>
.password-input-wrapper {
  position: relative;
}

.password-input-wrapper input {
  padding-right: 44px;
}

.password-toggle {
  position: absolute;
  top: 50%;
  right: 4px;
  transform: translateY(-50%);
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: auto;
  width: 36px;
  height: 36px;
  padding: 0;
  border: none;
  border-radius: var(--radius);
  background: transparent;
  color: var(--color-text-muted);
}

.password-toggle:hover {
  color: var(--color-text);
  background: var(--color-bg);
}

.password-toggle svg {
  width: 20px;
  height: 20px;
}
</style>
