import { useMemo, useState, type ReactNode } from 'react'
import { AuthContext } from './AuthContextValue'

type User = { name: string } | null

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User>(() => {
    const token = localStorage.getItem('token')
    if (!token) return null
    return { name: localStorage.getItem('userName') || 'User' }
  })

  const login = (name: string) => {
    localStorage.setItem('userName', name)
    setUser({ name })
  }

  const logout = () => {
    localStorage.removeItem('token')
    localStorage.removeItem('refreshToken')
    localStorage.removeItem('userName')
    setUser(null)
  }

  const value = useMemo(() => ({ user, isLoggedIn: !!user, login, logout }), [user])

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  )
}

