<script setup lang="ts">
defineProps<{
  title: string
  message: string
  confirmDisabled?: boolean
}>()

const emit = defineEmits<{
  confirm: []
  cancel: []
}>()
</script>

<template>
  <div class="confirm-overlay" @click.self="emit('cancel')">
    <div class="card confirm-content">
      <h3>{{ title }}</h3>
      <p>{{ message }}</p>
      <div class="confirm-actions">
        <button class="btn-secondary" type="button" @click="emit('cancel')">取消</button>
        <button class="btn-danger" type="button" :disabled="confirmDisabled" @click="emit('confirm')">確定</button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.confirm-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 16px;
  z-index: 200;
}

.confirm-content {
  width: 100%;
  max-width: 360px;
}

.confirm-content h3 {
  margin-top: 0;
}

.confirm-actions {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  margin-top: 16px;
}
</style>
