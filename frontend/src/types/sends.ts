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

export interface UploadAuthorization {
  sendId: string
  publicId: string
  folder: string
  cloudName: string
  apiKey: string
  timestamp: number
  signature: string
}

export interface CreateSendPayload {
  sendId: string
  gymName: string
  difficulty: string
  note: string
}
