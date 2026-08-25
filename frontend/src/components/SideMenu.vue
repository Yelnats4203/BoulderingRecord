<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { logout as logoutApi } from '../api/auth'
import { useAuthStore } from '../stores/auth'
import { useFriendRequestsStore } from '../stores/friendRequests'
import LoadingSpinner from './LoadingSpinner.vue'

const authStore = useAuthStore()
const friendRequestsStore = useFriendRequestsStore()
const router = useRouter()

const isLoggingOut = ref<boolean>(false)

async function handleLogout(): Promise<void> {
  if (isLoggingOut.value) {
    return
  }
  isLoggingOut.value = true
  try {
    await logoutApi()
  } finally {
    authStore.clearSession()
    friendRequestsStore.clear()
    isLoggingOut.value = false
    await router.push({ name: 'login' })
  }
}

onMounted(() => {
  void friendRequestsStore.refreshPendingCount()
})
</script>

<template>
  <nav class="side-menu">
    <RouterLink class="side-menu-title" :to="{ name: 'home' }">攀岩紀錄</RouterLink>

    <ul class="side-menu-list">
      <li>
        <RouterLink class="side-menu-link" :to="{ name: 'sessions' }">抱石紀錄</RouterLink>
      </li>
      <li>
        <RouterLink class="side-menu-link" :to="{ name: 'upload' }">上傳影片</RouterLink>
      </li>
      <li>
        <RouterLink class="side-menu-link" :to="{ name: 'videos' }">影片紀錄清單</RouterLink>
      </li>
      <li>
        <RouterLink class="side-menu-link" :to="{ name: 'friends' }">
          好友
          <span v-if="friendRequestsStore.pendingCount > 0" class="side-menu-badge" aria-label="有待處理的好友邀請"></span>
        </RouterLink>
      </li>
      <li v-if="authStore.hasEditPermission">
        <RouterLink class="side-menu-link" :to="{ name: 'users' }">新增使用者</RouterLink>
      </li>
      <li v-if="authStore.hasEditPermission">
        <RouterLink class="side-menu-link" :to="{ name: 'userList' }">使用者清單</RouterLink>
      </li>
      <li>
        <RouterLink class="side-menu-link" :to="{ name: 'changePassword' }">修改密碼</RouterLink>
      </li>
    </ul>

    <button
      class="btn-secondary side-menu-logout"
      :class="{ 'btn-loading': isLoggingOut }"
      type="button"
      :disabled="isLoggingOut"
      @click="handleLogout"
    >
      <LoadingSpinner v-if="isLoggingOut" :size="16" />
      <span>{{ isLoggingOut ? '登出中...' : '登出' }}</span>
    </button>
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
  display: block;
  font-size: 1.25rem;
  font-weight: 700;
  margin: 0 0 16px;
  color: var(--color-text);
  text-decoration: none;
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
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 10px 12px;
  border-radius: var(--radius);
  color: var(--color-text);
  text-decoration: none;
  font-weight: 600;
}

.side-menu-badge {
  display: inline-block;
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background: var(--color-danger);
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
