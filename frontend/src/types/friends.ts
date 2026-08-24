import type { VideoRecordResponse } from './sends'

export type FriendRelationStatus = 'None' | 'RequestSentByMe' | 'RequestReceivedFromThem' | 'Friends'

export interface FriendSummary {
  id: string
  userId: string
  username: string
  friendsSince: string
}

export interface FriendRequestSummary {
  id: string
  otherUserId: string
  otherUsername: string
  createdAt: string
}

export interface SendFriendRequestPayload {
  addresseeId: string
}

export interface FriendVideo {
  friendUserId: string
  friendUsername: string
  video: VideoRecordResponse
}
