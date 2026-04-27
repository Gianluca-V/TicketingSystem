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

// ── Error Translation Helper ────────────────────────────────────────────────
const translateError = (msg) => {
  if (!msg) return msg
  
  const translations = {
    'User already exists': 'El usuario ya existe',
    'Invalid credentials': 'Correo o contraseña incorrectos',
    'User not found': 'Usuario no encontrado',
    'An error occurred during user creation': 'Ocurrió un error al crear el usuario',
    'Concurrent update conflict': 'Conflicto de actualización concurrente. Reintentá.',
  }

  // Check for partial matches (like Identity errors)
  if (msg.includes('User creation failed:')) {
    if (msg.includes('is already taken')) return 'Este correo ya está registrado'
    if (msg.includes('Passwords must be at least')) return 'La contraseña no cumple con los requisitos de seguridad'
    return 'Error al registrar el usuario'
  }

  return translations[msg] || msg
}

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
    }

    const rawMessage =
      data?.error ||
      data?.message ||
      data?.title  ||
      (typeof data === 'string' ? data : null) ||
      `Error ${status}`

    return Promise.reject(new Error(translateError(rawMessage)))
  },
)

export default client
