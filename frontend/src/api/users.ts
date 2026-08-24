import apiClient from './client'
import type { CreateUserRequest, UserResponse, UserSearchResult } from '../types/users'

export function createUser(request: CreateUserRequest): Promise<UserResponse> {
  return apiClient.post<UserResponse>('/users', request).then((response) => response.data)
}

export function getUsers(): Promise<UserResponse[]> {
  return apiClient.get<UserResponse[]>('/users').then((response) => response.data)
}

export function searchUsers(keyword: string): Promise<UserSearchResult[]> {
  return apiClient.get<UserSearchResult[]>('/users/search', { params: { keyword } }).then((response) => response.data)
}
