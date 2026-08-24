export type SendVisibility = 'Private' | 'Public' | 'Shareable'

export interface SendResponse {
  id: string
  gymName: string | null
  climbAt: string
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
  climbAt: string
  isPublic: boolean
}

export interface VideoPlaybackResponse {
  playbackUrl: string
}

export interface VideoRecordResponse {
  id: string
  gymName: string | null
  climbAt: string
  difficulty: number | null
  note: string | null
  thumbnailUrl: string
}

export interface VideoRecordFilter {
  gymName: string
  climbAtFrom: string
  climbAtTo: string
  minDifficulty: string
  maxDifficulty: string
}

export interface UpdateSendPayload {
  climbAt: string
  gymName: string
  difficulty: string
  note: string
}

export interface UploadEligibilityResponse {
  isAllowed: boolean
}
