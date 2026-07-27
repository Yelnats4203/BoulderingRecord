export type RecordVisibility = 'Private' | 'Public' | 'Shareable'

export interface RecordResponse {
  id: string
  gymName: string | null
  uploadedAt: string
  difficulty: number | null
  uploaderId: string
  note: string | null
  visibility: RecordVisibility
}

export interface UploadRecordForm {
  video: File
  gymName: string
  difficulty: string
  note: string
}
