import axios from 'axios'

const SESSION_KEY = 'shop_session_id'
const TOKEN_KEY = 'admin_token'

function getSessionId(): string {
  let id = localStorage.getItem(SESSION_KEY)
  if (!id) {
    id = crypto.randomUUID()
    localStorage.setItem(SESSION_KEY, id)
  }
  return id
}

export function getStoredToken(): string | null {
  return localStorage.getItem(TOKEN_KEY)
}

export function setStoredToken(token: string): void {
  localStorage.setItem(TOKEN_KEY, token)
}

export function clearStoredToken(): void {
  localStorage.removeItem(TOKEN_KEY)
}

const client = axios.create({
  baseURL: '/api',
  headers: { 'Content-Type': 'application/json' },
})

client.interceptors.request.use((config) => {
  config.headers['X-Session-Id'] = getSessionId()
  const token = getStoredToken()
  if (token) {
    config.headers['Authorization'] = `Bearer ${token}`
  }
  return config
})

export default client
