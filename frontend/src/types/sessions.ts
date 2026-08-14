export interface GradeCountResponse {
  grade: number
  completedCount: number
  uncompletedCount: number
}

export interface SessionResponse {
  id: string
  userId: string
  date: string
  gymName: string | null
  gradeCounts: GradeCountResponse[]
}

export interface GradeCountRequest {
  grade: number
  completedCount: number
  uncompletedCount: number
}

export interface CreateSessionPayload {
  date: string
  gymName: string
  gradeCounts: GradeCountRequest[]
}
