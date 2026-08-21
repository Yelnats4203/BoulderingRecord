export interface CreateUserRequest {
  username: string
  acc: string
  psw: string
  hasEditPermission: boolean
  isDemoAcc: boolean
}

export interface UserResponse {
  id: string
  username: string
  acc: string
  hasEditPermission: boolean
  isDemoAcc: boolean
  createdAt: string
}
