import axios from 'axios'
import { API_CONFIG } from './config'
import { useAuthStore } from '@/stores/auth'

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
    if (!error.response) {
      return Promise.reject(new Error('No se pudo conectar con el servidor. Verificá tu conexión.'))
    }

    const { status, data } = error.response

    if (status === 401) {
      const auth = useAuthStore()
      auth.logout()
      // Let the router guard handle redirect via the store state
    }

    const message =
      data?.error ||
      data?.message ||
      data?.title  ||
      (typeof data === 'string' ? data : null) ||
      `Error ${status}`

    return Promise.reject(new Error(message))
  },
)

export default client
