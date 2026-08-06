<script setup lang="ts">
import { useRouter } from 'vue-router'
import { logout as logoutApi } from '../api/auth'
import { useAuthStore } from '../stores/auth'

const authStore = useAuthStore()
const router = useRouter()

async function handleLogout(): Promise<void> {
  try {
    await logoutApi()
  } finally {
    authStore.clearSession()
    await router.push({ name: 'login' })
  }
}
</script>

<template>
  <nav class="side-menu">
    <h1 class="side-menu-title">攀岩紀錄</h1>

    <ul class="side-menu-list">
      <li>
        <RouterLink class="side-menu-link" :to="{ name: 'upload' }">上傳影片</RouterLink>
      </li>
      <li>
        <RouterLink class="side-menu-link" :to="{ name: 'videos' }">影片紀錄清單</RouterLink>
      </li>
    </ul>

    <button class="btn-secondary side-menu-logout" type="button" @click="handleLogout">登出</button>
  </nav>
</template>

<style scoped>
.side-menu {
  display: flex;
  flex-direction: column;
  width: 100%;
  padding: 16px;
  background: var(--color-surface);
  border-bottom: 1px solid var(--color-border);
}

.side-menu-title {
  font-size: 1.25rem;
  margin: 0 0 16px;
}

.side-menu-list {
  list-style: none;
  margin: 0 0 16px;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.side-menu-link {
  display: block;
  padding: 10px 12px;
  border-radius: var(--radius);
  color: var(--color-text);
  text-decoration: none;
  font-weight: 600;
}

.side-menu-link:hover {
  background: var(--color-bg);
}

.side-menu-link.router-link-active {
  background: var(--color-primary);
  color: #ffffff;
}

.side-menu-logout {
  margin-top: auto;
}

@media (min-width: 768px) {
  .side-menu {
    width: 220px;
    min-height: 100%;
    border-bottom: none;
    border-right: 1px solid var(--color-border);
  }
}
</style>
