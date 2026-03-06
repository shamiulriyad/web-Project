import { createContext } from 'react'

type User = { name: string } | null

export type AuthContextType = {
  user: User
  isLoggedIn: boolean
  login: (name: string) => void
  logout: () => void
}

export const AuthContext = createContext<AuthContextType | undefined>(undefined)
