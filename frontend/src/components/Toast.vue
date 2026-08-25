<script setup lang="ts">
import { onMounted, onUnmounted } from 'vue'

const props = withDefaults(
  defineProps<{
    message: string
    type?: 'success' | 'error'
    duration?: number
  }>(),
  {
    type: 'success',
    duration: 3000,
  },
)

const emit = defineEmits<{
  dismiss: []
}>()

let timer: ReturnType<typeof setTimeout> | undefined

onMounted(() => {
  timer = setTimeout(() => emit('dismiss'), props.duration)
})

onUnmounted(() => {
  clearTimeout(timer)
})
</script>

<template>
  <div class="toast" :class="type">
    <span>{{ message }}</span>
    <button class="toast-close" type="button" aria-label="關閉" @click="emit('dismiss')">×</button>
  </div>
</template>

<style scoped>
.toast {
  position: fixed;
  top: 16px;
  left: 50%;
  transform: translateX(-50%);
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px 16px;
  border-radius: var(--radius);
  background: var(--color-surface);
  border: 2px solid var(--color-border);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
  z-index: 300;
}

.toast.success span {
  color: #16a34a;
}

.toast.error span {
  color: var(--color-danger);
}

.toast-close {
  min-height: auto;
  padding: 0 4px;
  border: none;
  background: transparent;
  color: var(--color-text-muted);
  font-size: 1.1rem;
  line-height: 1;
}

.toast-close:hover {
  color: var(--color-text);
}
</style>
