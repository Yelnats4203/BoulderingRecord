<script setup lang="ts">
import { ref } from 'vue'
import axios from 'axios'
import { changePassword } from '../api/auth'
import LoadingSpinner from '../components/LoadingSpinner.vue'
import PasswordInput from '../components/PasswordInput.vue'

const oldPsw = ref<string>('')
const newPsw = ref<string>('')
const isSubmitting = ref<boolean>(false)
const errorMessage = ref<string>('')
const successMessage = ref<string>('')

async function handleSubmit(): Promise<void> {
  errorMessage.value = ''
  successMessage.value = ''
  isSubmitting.value = true
  try {
    await changePassword({ oldPsw: oldPsw.value, newPsw: newPsw.value })
    successMessage.value = '密碼修改成功。'
    oldPsw.value = ''
    newPsw.value = ''
  } catch (error) {
    errorMessage.value =
      axios.isAxiosError(error) && typeof error.response?.data === 'string' && error.response.data
        ? error.response.data
        : '密碼修改失敗，請稍後再試。'
  } finally {
    isSubmitting.value = false
  }
}
</script>

<template>
  <div class="page change-password-page">
    <form class="card change-password-form" @submit.prevent="handleSubmit">
      <h2>修改密碼</h2>

      <p v-if="errorMessage" class="error-text">{{ errorMessage }}</p>
      <p v-if="successMessage" class="success-text">{{ successMessage }}</p>

      <div class="form-field">
        <label for="oldPsw">目前密碼</label>
        <PasswordInput id="oldPsw" v-model="oldPsw" autocomplete="current-password" />
      </div>

      <div class="form-field">
        <label for="newPsw">新密碼</label>
        <PasswordInput id="newPsw" v-model="newPsw" autocomplete="new-password" />
        <p class="hint-text">密碼需至少 8 碼，並包含大小寫英文、數字與特殊符號。</p>
      </div>

      <button class="btn-primary" :class="{ 'btn-loading': isSubmitting }" type="submit" :disabled="isSubmitting">
        <LoadingSpinner v-if="isSubmitting" :size="16" />
        <span>{{ isSubmitting ? '送出中...' : '修改密碼' }}</span>
      </button>
    </form>
  </div>
</template>

<style scoped>
.change-password-form {
  max-width: 480px;
  margin: 0 auto 24px;
}

.change-password-form h2 {
  margin-top: 0;
}

.success-text {
  color: #16a34a;
  font-size: 0.9rem;
  margin: 0 0 12px;
}
</style>
