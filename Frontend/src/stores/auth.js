import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { authApi } from '@/api/auth'
import { ADMIN_CONFIG } from '@/api/config'

/**
 * Auth store.
 * Token is kept in sessionStorage (cleared on tab close, not localStorage).
 * Admin status is derived from the JWT payload — no separate role endpoint needed.
 */
export const useAuthStore = defineStore('auth', () => {
  // ── State ──────────────────────────────────────────────────────────────────
  const token   = ref(sessionStorage.getItem('_tk') ?? null)
  const user    = ref(JSON.parse(sessionStorage.getItem('_usr') ?? 'null'))
  const loading = ref(false)
  const error   = ref(null)

  // ── Getters ────────────────────────────────────────────────────────────────
  const isAuthenticated = computed(() => !!token.value)
  const userId          = computed(() => user.value?.id ?? null)

  // ── Helpers ────────────────────────────────────────────────────────────────
  function _decodeJwt(tkn) {
    try {
      return JSON.parse(atob(tkn.split('.')[1]))
    } catch {
      return {}
    }
  }

  function _persist(tkn, usr) {
    token.value = tkn
    user.value  = usr
    sessionStorage.setItem('_tk',  tkn)
    sessionStorage.setItem('_usr', JSON.stringify(usr))
  }

  function _clear() {
    token.value = null
    user.value  = null
    sessionStorage.removeItem('_tk')
    sessionStorage.removeItem('_usr')
  }

  // ── Actions ────────────────────────────────────────────────────────────────
  async function login(email, password) {
    loading.value = true
    error.value   = null
    try {
      const data    = await authApi.login(email, password)
      const payload = _decodeJwt(data.token)
      _persist(data.token, {
        id:      payload.sub ?? payload.nameid ?? payload.userId,
        email:   payload.email ?? email,
        name:    payload.name  ?? email.split('@')[0],
        ...payload,
      })
      return true
    } catch (e) {
      error.value = e.message
      return false
    } finally {
      loading.value = false
    }
  }

  async function register(name, email, password) {
    loading.value = true
    error.value   = null
    try {
      await authApi.register({ name, email, password })
      return await login(email, password)
    } catch (e) {
      error.value = e.message
      return false
    } finally {
      loading.value = false
    }
  }

  function logout() { _clear() }
  function clearError() { error.value = null }

  return {
    token, user, loading, error,
    isAuthenticated, userId,
    login, register, logout, clearError,
  }
})
