import type { FriendRelationStatus } from './friends'

export interface CreateUserRequest {
  username: string
  acc: string
  psw: string
  hasEditPermission: boolean
  isDemoAcc: boolean
}

export interface AdminResetPasswordRequest {
  acc: string
  newPsw: string
}

export interface UserResponse {
  id: string
  username: string
  acc: string
  hasEditPermission: boolean
  isDemoAcc: boolean
  createdAt: string
}

export interface UserSearchResult {
  id: string
  username: string
  relationStatus: FriendRelationStatus
  friendRequestId: string | null
}
