import apiClient from './client'
import type { ChangePasswordRequest, LoginRequest, LoginResponse, RefreshTokenResponse } from '../types/auth'

export function login(request: LoginRequest): Promise<LoginResponse> {
  return apiClient.post<LoginResponse>('/auth/login', request).then((response) => response.data)
}

export function logout(): Promise<void> {
  return apiClient.post('/auth/logout').then(() => undefined)
}

export function refreshToken(): Promise<RefreshTokenResponse> {
  return apiClient.post<RefreshTokenResponse>('/auth/refresh').then((response) => response.data)
}

export function changePassword(request: ChangePasswordRequest): Promise<void> {
  return apiClient.post('/auth/change-password', request).then(() => undefined)
}
