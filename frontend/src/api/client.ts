import axios, { AxiosError, type InternalAxiosRequestConfig } from 'axios'
import { useAuthStore } from '../stores/auth'
import router from '../router'

const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL,
})

apiClient.interceptors.request.use((config: InternalAxiosRequestConfig) => {
  const authStore = useAuthStore()
  if (authStore.token) {
    config.headers.set('Authorization', `Bearer ${authStore.token}`)
  }
  return config
})

type UnauthorizedReason = 'SessionExpired' | 'DuplicateLogin'

interface UnauthorizedErrorResponse {
  reason?: UnauthorizedReason
}

const loginQueryReasonByUnauthorizedReason: Record<UnauthorizedReason, string> = {
  SessionExpired: 'session-expired',
  DuplicateLogin: 'duplicate-login',
}

apiClient.interceptors.response.use(
  (response) => response,
  (error: AxiosError<UnauthorizedErrorResponse>) => {
    if (error.response?.status === 401) {
      const authStore = useAuthStore()
      const reason: UnauthorizedReason = error.response.data?.reason ?? 'SessionExpired'
      authStore.clearSession()
      if (router.currentRoute.value.name !== 'login') {
        router.push({ name: 'login', query: { reason: loginQueryReasonByUnauthorizedReason[reason] } })
      }
    }
    return Promise.reject(error)
  },
)

export default apiClient
