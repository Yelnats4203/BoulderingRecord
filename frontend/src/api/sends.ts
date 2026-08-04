import apiClient from './client'
import type { SendResponse, UploadSendForm } from '../types/sends'

export function getAllSends(): Promise<SendResponse[]> {
  return apiClient.get<SendResponse[]>('/sends').then((response) => response.data)
}

export function uploadSend(form: UploadSendForm): Promise<SendResponse> {
  const formData = new FormData()
  formData.append('Video', form.video)
  if (form.gymName) {
    formData.append('GymName', form.gymName)
  }
  if (form.difficulty) {
    formData.append('Difficulty', form.difficulty)
  }
  if (form.note) {
    formData.append('Note', form.note)
  }

  return apiClient.post<SendResponse>('/sends', formData).then((response) => response.data)
}
