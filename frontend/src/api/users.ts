import apiClient from './client'
import type { CreateUserRequest, UserResponse } from '../types/users'

export function createUser(request: CreateUserRequest): Promise<UserResponse> {
  return apiClient.post<UserResponse>('/users', request).then((response) => response.data)
}
