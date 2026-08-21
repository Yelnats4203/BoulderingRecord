import axios from 'axios'
import apiClient from './client'
import type {
  CreateSendPayload,
  SendResponse,
  UpdateSendPayload,
  UploadAuthorization,
  UploadEligibilityResponse,
  VideoPlaybackResponse,
  VideoRecordFilter,
  VideoRecordResponse,
} from '../types/sends'

export function getMySends(filter: Partial<VideoRecordFilter>): Promise<VideoRecordResponse[]> {
  return apiClient
    .get<VideoRecordResponse[]>('/sends/mine', {
      params: {
        gymName: filter.gymName || undefined,
        climbAtFrom: filter.climbAtFrom || undefined,
        climbAtTo: filter.climbAtTo || undefined,
        minDifficulty: filter.minDifficulty || undefined,
        maxDifficulty: filter.maxDifficulty || undefined,
      },
    })
    .then((response) => response.data)
}

export function getUploadEligibility(): Promise<UploadEligibilityResponse> {
  return apiClient.get<UploadEligibilityResponse>('/sends/upload-eligibility').then((response) => response.data)
}

export function getUploadAuthorization(): Promise<UploadAuthorization> {
  return apiClient.post<UploadAuthorization>('/sends/upload-authorization').then((response) => response.data)
}

export function uploadVideoToCloudinary(video: File, auth: UploadAuthorization): Promise<void> {
  const formData = new FormData()
  formData.append('file', video)
  formData.append('public_id', auth.publicId)
  formData.append('folder', auth.folder)
  formData.append('timestamp', String(auth.timestamp))
  formData.append('api_key', auth.apiKey)
  formData.append('signature', auth.signature)
  formData.append('type', 'authenticated')

  return axios
    .post(`https://api.cloudinary.com/v1_1/${auth.cloudName}/video/upload`, formData)
    .then(() => undefined)
}

export function createSend(payload: CreateSendPayload): Promise<SendResponse> {
  return apiClient
    .post<SendResponse>('/sends', {
      sendId: payload.sendId,
      gymName: payload.gymName || null,
      difficulty: payload.difficulty ? Number(payload.difficulty) : null,
      note: payload.note || null,
      climbAt: payload.climbAt || undefined,
    })
    .then((response) => response.data)
}

export function updateSend(id: string, payload: UpdateSendPayload): Promise<SendResponse> {
  return apiClient
    .put<SendResponse>(`/sends/${id}`, {
      climbAt: payload.climbAt,
      gymName: payload.gymName || null,
      difficulty: payload.difficulty ? Number(payload.difficulty) : null,
      note: payload.note || null,
    })
    .then((response) => response.data)
}

export function deleteSend(id: string): Promise<void> {
  return apiClient.delete(`/sends/${id}`).then(() => undefined)
}

export function getSendVideo(id: string): Promise<VideoPlaybackResponse> {
  return apiClient.get<VideoPlaybackResponse>(`/sends/${id}/video`).then((response) => response.data)
}
