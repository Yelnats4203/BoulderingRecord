export interface LoginRequest {
  acc: string
  psw: string
}

export interface LoginResponse {
  token: string
  expiresAt: string
  hasEditPermission: boolean
  userId: string
}

export interface RefreshTokenResponse {
  token: string
  expiresAt: string
  hasEditPermission: boolean
}

export interface ChangePasswordRequest {
  oldPsw: string
  newPsw: string
}
