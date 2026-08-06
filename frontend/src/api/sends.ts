import axios from 'axios'
import apiClient from './client'
import type { CreateSendPayload, SendResponse, UploadAuthorization } from '../types/sends'

export function getAllSends(): Promise<SendResponse[]> {
  return apiClient.get<SendResponse[]>('/sends').then((response) => response.data)
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
    })
    .then((response) => response.data)
}
