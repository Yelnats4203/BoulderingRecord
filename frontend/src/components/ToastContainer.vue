<script setup lang="ts">
import { useToastStore } from '../stores/toast'
import Toast from './Toast.vue'

const toastStore = useToastStore()
</script>

<template>
  <div class="toast-container">
    <Toast
      v-for="toast in toastStore.toasts"
      :key="toast.id"
      :message="toast.message"
      :type="toast.type"
      :duration="toast.duration"
      @dismiss="toastStore.dismissToast(toast.id)"
    />
  </div>
</template>

<style scoped>
.toast-container {
  position: fixed;
  top: 16px;
  left: 50%;
  transform: translateX(-50%);
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
  z-index: 300;
  pointer-events: none;
}
</style>
