export interface LoginRequest {
  acc: string
  psw: string
}

export interface LoginResponse {
  token: string
  expiresAt: string
}

export interface RefreshTokenResponse {
  token: string
  expiresAt: string
}
