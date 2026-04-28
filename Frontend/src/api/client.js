import axios from 'axios'
import { API_CONFIG } from './config'
import { useAuthStore } from '@/stores/auth'
import { getErrorMessage } from './errors'

const client = axios.create({
  baseURL: API_CONFIG.BASE_URL,
  timeout: 15_000,
  headers: { 'Content-Type': 'application/json' },
})

// ── Request interceptor: inject Bearer token ─────────────────────────────────
client.interceptors.request.use(
  (config) => {
    // authStore is accessed lazily to avoid circular imports at module init
    const auth = useAuthStore()
    if (auth.token) {
      config.headers.Authorization = `Bearer ${auth.token}`
    }
    return config
  },
  (error) => Promise.reject(error),
)

// ── Response interceptor: normalise errors & handle 401 ──────────────────────
client.interceptors.response.use(
  (response) => response,
  (error) => {
    const { status } = error.response || {}

    if (status === 401) {
      const auth = useAuthStore()
      auth.logout()
    }

    // Use centralized error handling logic
    const friendlyMessage = getErrorMessage(error)
    
    return Promise.reject(new Error(friendlyMessage))
  },
)

export default client
