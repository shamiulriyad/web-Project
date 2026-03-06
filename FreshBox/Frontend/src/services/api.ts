import axios from 'axios'

const defaultApiUrl = import.meta.env.VITE_API_URL || 'http://localhost:5259';

export const api = axios.create({
  baseURL: defaultApiUrl,
  headers: { 'Content-Type': 'application/json' }
})

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token')
  if (token) {
    config.headers = config.headers ?? {}
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

export default api
