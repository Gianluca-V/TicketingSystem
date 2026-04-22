import client from './client'
import { API_CONFIG } from './config'

const E = API_CONFIG.ENDPOINTS

export const authApi = {
  login: (email, password) =>
    client.post(E.LOGIN, { email, password }).then((r) => r.data),

  register: (payload) =>
    client.post(E.USERS, payload).then((r) => r.data),

  getProfile: (id) =>
    client.get(E.USER(id)).then((r) => r.data),

  updateProfile: (id, payload) =>
    client.put(E.USER(id), payload).then((r) => r.data),
}
