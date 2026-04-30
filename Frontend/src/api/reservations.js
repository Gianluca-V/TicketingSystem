import client from './client'
import { API_CONFIG } from './config'

const E = API_CONFIG.ENDPOINTS

// ── Reservations ──────────────────────────────────────────────────────────────
export const reservationsApi = {
  /**
   * Creates a temporary reservation (seat hold).
   * @param {number} seatId
   * @param {number} userId
   */
  create: (seatId, userId) =>
    client.post(E.RESERVE_SEAT(seatId), { seatId, userId }).then((r) => r.data),

  get: (reservationId) =>
    client.get(E.RESERVATION(reservationId)).then((r) => r.data),

  list: (params = {}) =>
    client.get(E.RESERVATIONS, { params }).then((r) => r.data),
}

// ── Payments ──────────────────────────────────────────────────────────────────
export const paymentsApi = {
  /**
   * Processes payment for a held reservation.
   * @param {string} reservationId  GUID
   * @param {string} transactionId  Processor-issued transaction ID
   */
  process: (reservationId, transactionId) =>
    client
      .post(E.PAYMENT(reservationId), { reservationId, transactionId })
      .then((r) => r.data),
}
