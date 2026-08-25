<script setup lang="ts">
import { ref } from 'vue'
import axios from 'axios'
import { resetUserPassword } from '../api/users'
import ConfirmDialog from './ConfirmDialog.vue'
import PasswordInput from './PasswordInput.vue'

const props = defineProps<{
  acc: string
  username: string
}>()

const emit = defineEmits<{
  close: []
  updated: []
}>()

const newPsw = ref<string>('')
const errorMessage = ref<string>('')
const isConfirming = ref<boolean>(false)
const isSubmitting = ref<boolean>(false)

function handleRequestConfirm(): void {
  errorMessage.value = ''
  isConfirming.value = true
}

async function handleConfirmReset(): Promise<void> {
  isSubmitting.value = true
  try {
    await resetUserPassword({ acc: props.acc, newPsw: newPsw.value })
    isConfirming.value = false
    emit('updated')
    emit('close')
  } catch (error) {
    isConfirming.value = false
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
  <div class="modal-overlay" @click.self="emit('close')">
    <div class="card modal-content">
      <div class="modal-header">
        <h2>修改密碼</h2>
        <button class="btn-secondary" type="button" @click="emit('close')">關閉</button>
      </div>

      <p class="hint-text">帳號：{{ props.acc }}（{{ props.username }}）</p>
      <p v-if="errorMessage" class="error-text">{{ errorMessage }}</p>

      <form class="edit-form" @submit.prevent="handleRequestConfirm">
        <div class="form-field">
          <label for="new-psw">新密碼</label>
          <PasswordInput id="new-psw" v-model="newPsw" autocomplete="new-password" />
          <p class="hint-text">密碼需至少 8 碼，並包含大小寫英文、數字與特殊符號。</p>
        </div>

        <div class="edit-actions">
          <button class="btn-secondary" type="button" @click="emit('close')">取消</button>
          <button class="btn-primary" type="submit">確認修改</button>
        </div>
      </form>
    </div>

    <ConfirmDialog
      v-if="isConfirming"
      title="修改密碼"
      message="是否確定要修改此使用者的密碼？"
      :confirm-disabled="isSubmitting"
      :confirm-loading="isSubmitting"
      confirm-loading-text="修改中..."
      @confirm="handleConfirmReset"
      @cancel="isConfirming = false"
    />
  </div>
</template>

<style scoped>
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 16px;
  z-index: 100;
}

.modal-content {
  width: 100%;
  max-width: 480px;
  max-height: calc(100vh - 32px);
  overflow-y: auto;
}

.modal-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 16px;
}

.modal-header h2 {
  margin: 0;
  font-size: 1.1rem;
}

.edit-form .form-field:last-of-type {
  margin-bottom: 0;
}

.edit-actions {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  margin-top: 8px;
}
</style>
