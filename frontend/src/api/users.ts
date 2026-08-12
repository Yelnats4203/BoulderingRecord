import apiClient from './client'
import type { CreateUserRequest, UserResponse } from '../types/users'

export function createUser(request: CreateUserRequest): Promise<UserResponse> {
  return apiClient.post<UserResponse>('/users', request).then((response) => response.data)
}

export function getUsers(): Promise<UserResponse[]> {
  return apiClient.get<UserResponse[]>('/users').then((response) => response.data)
}
