<script setup lang="ts">
import { ref } from 'vue'
import { createUser } from '../api/users'
import LoadingSpinner from '../components/LoadingSpinner.vue'

const username = ref<string>('')
const acc = ref<string>('')
const psw = ref<string>('')
const hasEditPermission = ref<boolean>(false)
const isSubmitting = ref<boolean>(false)
const errorMessage = ref<string>('')
const successMessage = ref<string>('')

async function handleSubmit(): Promise<void> {
  errorMessage.value = ''
  successMessage.value = ''
  isSubmitting.value = true
  try {
    await createUser({
      username: username.value,
      acc: acc.value,
      psw: psw.value,
      hasEditPermission: hasEditPermission.value,
    })
    successMessage.value = '使用者建立成功。'
    username.value = ''
    acc.value = ''
    psw.value = ''
    hasEditPermission.value = false
  } catch {
    errorMessage.value = '建立失敗，請確認帳號是否已被使用後再試一次。'
  } finally {
    isSubmitting.value = false
  }
}
</script>

<template>
  <div class="page create-user-page">
    <form class="card create-user-form" @submit.prevent="handleSubmit">
      <h2>新增使用者</h2>

      <p v-if="errorMessage" class="error-text">{{ errorMessage }}</p>
      <p v-if="successMessage" class="success-text">{{ successMessage }}</p>

      <div class="form-field">
        <label for="username">使用者名稱</label>
        <input id="username" v-model="username" type="text" required />
      </div>

      <div class="form-field">
        <label for="acc">帳號</label>
        <input id="acc" v-model="acc" type="text" autocomplete="off" required />
      </div>

      <div class="form-field">
        <label for="psw">密碼</label>
        <input id="psw" v-model="psw" type="password" autocomplete="new-password" required />
      </div>

      <div class="form-field form-field-checkbox">
        <label for="hasEditPermission">
          <input id="hasEditPermission" v-model="hasEditPermission" type="checkbox" />
          賦予編輯權限
        </label>
      </div>

      <button class="btn-primary" :class="{ 'btn-loading': isSubmitting }" type="submit" :disabled="isSubmitting">
        <LoadingSpinner v-if="isSubmitting" :size="16" />
        <span>{{ isSubmitting ? '建立中...' : '建立使用者' }}</span>
      </button>
    </form>
  </div>
</template>

<style scoped>
.create-user-form {
  max-width: 480px;
  margin: 0 auto 24px;
}

.create-user-form h2 {
  margin-top: 0;
}

.form-field-checkbox label {
  display: flex;
  align-items: center;
  gap: 8px;
  font-weight: normal;
}

.form-field-checkbox input[type='checkbox'] {
  width: auto;
}

.success-text {
  color: #16a34a;
  font-size: 0.9rem;
  margin: 0 0 12px;
}
</style>
