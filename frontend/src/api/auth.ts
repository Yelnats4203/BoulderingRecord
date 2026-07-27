import apiClient from './client'
import type { LoginRequest, LoginResponse, RefreshTokenResponse } from '../types/auth'

export function login(request: LoginRequest): Promise<LoginResponse> {
  return apiClient.post<LoginResponse>('/api/auth/login', request).then((response) => response.data)
}

export function logout(): Promise<void> {
  return apiClient.post('/api/auth/logout').then(() => undefined)
}

export function refreshToken(): Promise<RefreshTokenResponse> {
  return apiClient.post<RefreshTokenResponse>('/api/auth/refresh').then((response) => response.data)
}
