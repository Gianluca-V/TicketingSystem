import client from './client'
import { API_CONFIG } from './config'

const E = API_CONFIG.ENDPOINTS

// ── Events ───────────────────────────────────────────────────────────────────
export const eventsApi = {
  list: (params = {}) =>
    client.get(E.EVENTS, { params }).then((r) => r.data),

  get: (id) =>
    client.get(E.EVENT(id)).then((r) => r.data),
}

// ── Sectors ──────────────────────────────────────────────────────────────────
export const sectorsApi = {
  list: (eventId) =>
    client.get(E.SECTORS(eventId)).then((r) => r.data),

  get: (eventId, sectorId) =>
    client.get(E.SECTOR(eventId, sectorId)).then((r) => r.data),
}

// ── Seats ─────────────────────────────────────────────────────────────────────
export const seatsApi = {
  listBySector: (eventId, sectorId) =>
    client.get(E.SEATS_BY_SECTOR(eventId, sectorId)).then((r) => r.data),

  get: (id) =>
    client.get(E.SEAT(id)).then((r) => r.data),
}
