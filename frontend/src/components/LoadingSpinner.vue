<script setup lang="ts">
withDefaults(
  defineProps<{
    size?: number
  }>(),
  {
    size: 16,
  },
)
</script>

<template>
  <span class="loading-spinner" :style="{ width: size + 'px', height: size + 'px' }" role="status" aria-hidden="true"></span>
</template>

<style scoped>
.loading-spinner {
  display: inline-block;
  flex-shrink: 0;
  border-radius: 50%;
  border: 2px solid currentColor;
  border-right-color: transparent;
  animation: spin 0.6s linear infinite;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}
</style>
