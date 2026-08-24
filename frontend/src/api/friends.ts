import apiClient from './client'
import type { FriendRequestSummary, FriendSummary, FriendVideo, SendFriendRequestPayload } from '../types/friends'
import type { VideoRecordResponse } from '../types/sends'

export function getFriends(): Promise<FriendSummary[]> {
  return apiClient.get<FriendSummary[]>('/friends').then((response) => response.data)
}

export function getPendingFriendRequests(): Promise<FriendRequestSummary[]> {
  return apiClient.get<FriendRequestSummary[]>('/friends/requests').then((response) => response.data)
}

export function sendFriendRequest(payload: SendFriendRequestPayload): Promise<FriendRequestSummary> {
  return apiClient.post<FriendRequestSummary>('/friends/requests', payload).then((response) => response.data)
}

export function acceptFriendRequest(id: string): Promise<FriendSummary> {
  return apiClient.post<FriendSummary>(`/friends/${id}/accept`).then((response) => response.data)
}

export function deleteFriendRequest(id: string): Promise<void> {
  return apiClient.delete(`/friends/${id}`).then(() => undefined)
}

export function getFriendVideos(userId: string): Promise<VideoRecordResponse[]> {
  return apiClient.get<VideoRecordResponse[]>(`/friends/${userId}/videos`).then((response) => response.data)
}

export function getRecentFriendVideos(): Promise<FriendVideo[]> {
  return apiClient.get<FriendVideo[]>('/friends/videos/recent').then((response) => response.data)
}
