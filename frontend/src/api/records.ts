import apiClient from './client'
import type { RecordResponse, UploadRecordForm } from '../types/records'

export function getAllRecords(): Promise<RecordResponse[]> {
  return apiClient.get<RecordResponse[]>('/records').then((response) => response.data)
}

export function uploadRecord(form: UploadRecordForm): Promise<RecordResponse> {
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

  return apiClient.post<RecordResponse>('/records', formData).then((response) => response.data)
}
