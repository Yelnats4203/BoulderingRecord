import { computed, ref } from 'vue'
import { defineStore } from 'pinia'

const STORAGE_KEY = 'auth'

interface StoredSession {
  token: string
  expiresAt: string
  hasEditPermission: boolean
}

function loadStoredSession(): StoredSession | null {
  const raw: string | null = localStorage.getItem(STORAGE_KEY)
  if (!raw) {
    return null
  }
  try {
    return JSON.parse(raw) as StoredSession
  } catch {
    return null
  }
}

export const useAuthStore = defineStore('auth', () => {
  const stored: StoredSession | null = loadStoredSession()
  const token = ref<string | null>(stored?.token ?? null)
  const expiresAt = ref<string | null>(stored?.expiresAt ?? null)
  const hasEditPermission = ref<boolean>(stored?.hasEditPermission ?? false)

  const isAuthenticated = computed<boolean>(() => {
    if (!token.value || !expiresAt.value) {
      return false
    }
    return new Date(expiresAt.value).getTime() > Date.now()
  })

  function setSession(newToken: string, newExpiresAt: string, newHasEditPermission: boolean): void {
    token.value = newToken
    expiresAt.value = newExpiresAt
    hasEditPermission.value = newHasEditPermission
    localStorage.setItem(
      STORAGE_KEY,
      JSON.stringify({ token: newToken, expiresAt: newExpiresAt, hasEditPermission: newHasEditPermission }),
    )
  }

  function clearSession(): void {
    token.value = null
    expiresAt.value = null
    hasEditPermission.value = false
    localStorage.removeItem(STORAGE_KEY)
  }

  return { token, expiresAt, hasEditPermission, isAuthenticated, setSession, clearSession }
})
