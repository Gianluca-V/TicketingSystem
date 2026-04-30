/**
 * Admin API module.
 * All operations that require elevated privileges live here,
 * clearly separated from the public-facing API modules.
 */
import client from './client'
import { API_CONFIG } from './config'

const E = API_CONFIG.ENDPOINTS

// ── Users (admin) ─────────────────────────────────────────────────────────────
export const adminUsersApi = {
  list: (params = {}) =>
    client.get(E.USERS, { params }).then((r) => r.data),

  get: (id) =>
    client.get(E.USER(id)).then((r) => r.data),

  update: (id, payload) =>
    client.put(E.USER(id), payload).then((r) => r.data),

  delete: (id) =>
    client.delete(E.USER(id)).then((r) => r.data),
}

// ── Events (admin) ────────────────────────────────────────────────────────────
export const adminEventsApi = {
  list: (params = {}) =>
    client.get(E.EVENTS, { params }).then((r) => r.data),

  get: (id) =>
    client.get(E.EVENT(id)).then((r) => r.data),

  create: (payload) =>
    client.post(E.EVENTS, payload).then((r) => r.data),

  update: (id, payload) =>
    client.put(E.EVENT(id), payload).then((r) => r.data),

  delete: (id) =>
    client.delete(E.EVENT(id)).then((r) => r.data),
}

// ── Sectors (admin) ───────────────────────────────────────────────────────────
export const adminSectorsApi = {
  list: (eventId) =>
    client.get(E.SECTORS(eventId)).then((r) => r.data),

  get: (eventId, sectorId) =>
    client.get(E.SECTOR(eventId, sectorId)).then((r) => r.data),

  create: (eventId, payload) =>
    client.post(E.SECTORS(eventId), payload).then((r) => r.data),

  update: (eventId, sectorId, payload) =>
    client.put(E.SECTOR(eventId, sectorId), payload).then((r) => r.data),

  delete: (eventId, sectorId) =>
    client.delete(E.SECTOR(eventId, sectorId)).then((r) => r.data),
}

// ── Seats (admin) ─────────────────────────────────────────────────────────────
export const adminSeatsApi = {
  listBySector: (eventId, sectorId) =>
    client.get(E.SEATS_BY_SECTOR(eventId, sectorId)).then((r) => r.data),

  create: (eventId, sectorId, payload) =>
    client.post(E.SEATS_BY_SECTOR(eventId, sectorId), payload).then((r) => r.data),

  createBulk: (eventId, sectorId, seats) =>
    client.post(`${E.SEATS_BY_SECTOR(eventId, sectorId)}/bulk`, seats).then((r) => r.data),

  update: (id, payload) =>
    client.put(E.SEAT(id), payload).then((r) => r.data),

  delete: (id) =>
    client.delete(E.SEAT(id)).then((r) => r.data),
}

// ── Reservations (admin) ──────────────────────────────────────────────────────
export const adminReservationsApi = {
  /** List all reservations, optionally filtered */
  listAll: (params = {}) =>
    client.get(E.RESERVATIONS, { params }).then((r) => r.data),

  listBySeat: (seatId) =>
    client.get(E.RESERVE_SEAT(seatId)).then((r) => r.data),

  get: (reservationId) =>
    client.get(E.RESERVATION(reservationId)).then((r) => r.data),
}

// ── Audit logs (admin) ────────────────────────────────────────────────────────
export const adminAuditApi = {
  list: (params = {}) =>
    client.get(E.AUDIT_LOGS, { params }).then((r) => r.data),
}
