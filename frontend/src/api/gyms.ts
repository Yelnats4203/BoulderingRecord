import apiClient from './client'

export function getGymNames(): Promise<string[]> {
  return apiClient.get<string[]>('/gyms/names').then((response) => response.data)
}
