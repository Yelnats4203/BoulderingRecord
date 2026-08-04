export type SendVisibility = 'Private' | 'Public' | 'Shareable'

export interface SendResponse {
  id: string
  gymName: string | null
  uploadedAt: string
  difficulty: number | null
  uploaderId: string
  note: string | null
  visibility: SendVisibility
}

export interface UploadSendForm {
  video: File
  gymName: string
  difficulty: string
  note: string
}
