import { useState } from 'react'
import type { FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import api from '../services/api'
import { useAuth } from '../context/useAuth'

type AuthResponse = {
  token: string
  refreshToken: string
  expiresAt: string
}

export default function Login() {
  const navigate = useNavigate()
  const { login } = useAuth()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState<string | null>(null)

  async function onSubmit(e: FormEvent) {
    e.preventDefault()
    setError(null)

    try {
      const { data } = await api.post<AuthResponse>('/api/auth/login', { email, password })
      localStorage.setItem('token', data.token)
      localStorage.setItem('refreshToken', data.refreshToken)
      login(email)
      navigate('/shop')
    } catch {
      setError('Login failed. Check email/password and try again.')
    }
  }

  function cancel() {
    localStorage.removeItem('token')
    localStorage.removeItem('refreshToken')
    navigate('/')
  }

  return (
    <div className="max-w-md mx-auto p-6">
      <h2 className="text-xl font-semibold mb-4">Login</h2>
      <form onSubmit={onSubmit} className="flex flex-col gap-3">
        {error && <p className="text-sm text-red-600">{error}</p>}
        <input value={email} onChange={e => setEmail(e.target.value)} placeholder="Email" className="px-3 py-2 border rounded" />
        <input value={password} onChange={e => setPassword(e.target.value)} type="password" placeholder="Password" className="px-3 py-2 border rounded" />
        <div className="flex gap-2">
          <button type="submit" className="px-4 py-2 bg-dibi-500 text-white rounded">Sign in</button>
          <button type="button" onClick={cancel} className="px-4 py-2 border rounded">Cancel</button>
        </div>
      </form>
    </div>
  )
}
