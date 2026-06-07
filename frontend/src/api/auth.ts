import client from './client'

export interface LoginResponse {
  accessToken: string
  refreshToken: string
  expiresAt: string
}

export async function login(username: string, password: string): Promise<LoginResponse> {
  const { data } = await client.post<LoginResponse>('/auth/login', { username, password })
  return data
}
