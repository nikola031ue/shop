import { createContext, useContext, useState, type ReactNode } from 'react'
import { login as loginApi } from '../api/auth'
import { setStoredToken, clearStoredToken, getStoredToken } from '../api/client'

interface AuthContextValue {
  token: string | null
  login: (username: string, password: string) => Promise<void>
  logout: () => void
}

const AuthContext = createContext<AuthContextValue | null>(null)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [token, setToken] = useState<string | null>(getStoredToken)

  async function login(username: string, password: string) {
    const res = await loginApi(username, password)
    setStoredToken(res.accessToken)
    setToken(res.accessToken)
  }

  function logout() {
    clearStoredToken()
    setToken(null)
  }

  return (
    <AuthContext.Provider value={{ token, login, logout }}>
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth() {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used within AuthProvider')
  return ctx
}
