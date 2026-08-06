<script setup lang="ts">
import SideMenu from '../components/SideMenu.vue'
</script>

<template>
  <div class="app-layout">
    <SideMenu />
    <main class="app-layout-content">
      <RouterView />
    </main>
  </div>
</template>

<style scoped>
.app-layout {
  display: flex;
  flex-direction: column;
  min-height: 100%;
}

.app-layout-content {
  flex: 1;
  min-width: 0;
}

@media (min-width: 768px) {
  .app-layout {
    flex-direction: row;
  }
}
</style>
