import { ref } from 'vue'
import { defineStore } from 'pinia'

export type ToastType = 'success' | 'error'

export interface ToastItem {
  id: number
  message: string
  type: ToastType
  duration: number
}

const DEFAULT_DURATION_BY_TYPE: Record<ToastType, number> = {
  success: 3000,
  error: 5000,
}

let nextId = 0

export const useToastStore = defineStore('toast', () => {
  const toasts = ref<ToastItem[]>([])

  function showToast(message: string, type: ToastType = 'success', duration?: number): void {
    toasts.value.push({ id: nextId++, message, type, duration: duration ?? DEFAULT_DURATION_BY_TYPE[type] })
  }

  function dismissToast(id: number): void {
    const index: number = toasts.value.findIndex((toast) => toast.id === id)
    if (index !== -1) {
      toasts.value.splice(index, 1)
    }
  }

  return { toasts, showToast, dismissToast }
})
