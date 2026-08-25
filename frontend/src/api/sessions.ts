import apiClient from './client'
import type { CreateSessionPayload, SessionResponse } from '../types/sessions'

export function getSessions(dateFrom?: string, dateTo?: string): Promise<SessionResponse[]> {
  return apiClient
    .get<SessionResponse[]>('/Sessions', {
      params: {
        dateFrom: dateFrom || undefined,
        dateTo: dateTo || undefined,
      },
    })
    .then((response) => response.data)
}

export function createSession(payload: CreateSessionPayload): Promise<SessionResponse> {
  return apiClient
    .post<SessionResponse>('/Sessions', {
      date: payload.date,
      gymName: payload.gymName || null,
      gradeCounts: payload.gradeCounts,
    })
    .then((response) => response.data)
}

export function updateSession(id: string, payload: CreateSessionPayload): Promise<SessionResponse> {
  return apiClient
    .put<SessionResponse>(`/Sessions/${id}`, {
      date: payload.date,
      gymName: payload.gymName || null,
      gradeCounts: payload.gradeCounts,
    })
    .then((response) => response.data)
}

export function deleteSession(id: string): Promise<void> {
  return apiClient.delete(`/Sessions/${id}`).then(() => undefined)
}
