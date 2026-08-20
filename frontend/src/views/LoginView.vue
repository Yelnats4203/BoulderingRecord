<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { login } from '../api/auth'
import { useAuthStore } from '../stores/auth'
import LoadingSpinner from '../components/LoadingSpinner.vue'
import PasswordInput from '../components/PasswordInput.vue'

const authStore = useAuthStore()
const router = useRouter()
const route = useRoute()

const acc = ref<string>('')
const psw = ref<string>('')
const errorMessage = ref<string>('')
const isSubmitting = ref<boolean>(false)

const loginTimeoutMessagesByReason: Record<string, string> = {
  'duplicate-login': '此帳號已在其他裝置重新登入，您已被登出。',
  'session-expired': '登入已逾時，請重新登入。',
}

onMounted(() => {
  const reason: string | null = typeof route.query.reason === 'string' ? route.query.reason : null
  if (reason && reason in loginTimeoutMessagesByReason) {
    errorMessage.value = loginTimeoutMessagesByReason[reason]
    router.replace({ name: 'login' })
  }
})

async function handleSubmit(): Promise<void> {
  errorMessage.value = ''
  isSubmitting.value = true
  try {
    const response = await login({ acc: acc.value, psw: psw.value })
    authStore.setSession(response.token, response.expiresAt, response.hasEditPermission)
    await router.push({ name: 'dashboard' })
  } catch {
    errorMessage.value = '帳號或密碼錯誤'
  } finally {
    isSubmitting.value = false
  }
}
</script>

<template>
  <div class="page login-page">
    <form class="card login-card" @submit.prevent="handleSubmit">
      <h1>登入</h1>

      <p v-if="errorMessage" class="error-text">{{ errorMessage }}</p>

      <div class="form-field">
        <label for="acc">帳號</label>
        <input id="acc" v-model="acc" type="text" autocomplete="username" required />
      </div>

      <div class="form-field">
        <label for="psw">密碼</label>
        <PasswordInput id="psw" v-model="psw" autocomplete="current-password" />
      </div>

      <button class="btn-primary" :class="{ 'btn-loading': isSubmitting }" type="submit" :disabled="isSubmitting">
        <LoadingSpinner v-if="isSubmitting" :size="16" />
        <span>{{ isSubmitting ? '登入中...' : '登入' }}</span>
      </button>

      <p class="hint-text">登入後，其他裝置上原本的登入狀態將自動失效。</p>
    </form>
  </div>
</template>

<style scoped>
.login-page {
  display: flex;
  align-items: center;
  justify-content: center;
}

.login-card {
  max-width: 400px;
}

.login-card h1 {
  margin-top: 0;
  font-size: 1.5rem;
  text-align: center;
}
</style>
