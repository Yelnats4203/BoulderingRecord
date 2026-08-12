<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { getUsers } from '../api/users'
import type { UserResponse } from '../types/users'

const users = ref<UserResponse[]>([])
const isLoading = ref<boolean>(false)
const errorMessage = ref<string>('')

function formatDateOnly(value: string): string {
  return new Date(value).toLocaleDateString()
}

async function fetchUsers(): Promise<void> {
  isLoading.value = true
  errorMessage.value = ''
  try {
    users.value = await getUsers()
  } catch {
    errorMessage.value = '讀取使用者清單失敗，請稍後再試。'
  } finally {
    isLoading.value = false
  }
}

onMounted(() => {
  void fetchUsers()
})
</script>

<template>
  <div class="page user-list-page">
    <div class="page-header">
      <h2>使用者清單</h2>
      <RouterLink class="btn-primary create-user-link" :to="{ name: 'users' }">新增使用者</RouterLink>
    </div>

    <p v-if="errorMessage" class="error-text">{{ errorMessage }}</p>
    <p v-else-if="isLoading" class="hint-text">載入中...</p>
    <p v-else-if="users.length === 0" class="card empty-state">目前尚無使用者。</p>

    <table v-else class="card user-table">
      <thead>
        <tr>
          <th>使用者名稱</th>
          <th>帳號</th>
          <th>編輯權限</th>
          <th>建立時間</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="user in users" :key="user.id">
          <td>{{ user.username }}</td>
          <td>{{ user.acc }}</td>
          <td>{{ user.hasEditPermission ? '是' : '否' }}</td>
          <td>{{ formatDateOnly(user.createdAt) }}</td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<style scoped>
.page-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
}

.page-header h2 {
  margin: 0;
}

.create-user-link {
  width: auto;
  display: inline-block;
  text-decoration: none;
  white-space: nowrap;
}

.empty-state {
  text-align: center;
  color: var(--color-text-muted);
}

.user-table {
  width: 100%;
  border-collapse: collapse;
}

.user-table th,
.user-table td {
  padding: 10px 12px;
  text-align: left;
  border-bottom: 1px solid var(--color-border);
}

.user-table th {
  color: var(--color-text-muted);
  font-weight: 600;
}

.user-table tbody tr:last-child td {
  border-bottom: none;
}
</style>
