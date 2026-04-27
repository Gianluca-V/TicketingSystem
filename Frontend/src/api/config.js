/**
 * Central API configuration.
 * Change BASE_URL or any endpoint path here without touching business logic.
 */
export const API_CONFIG = {
  BASE_URL: import.meta.env.VITE_API_BASE_URL || 'http://0.0.0.0:8080/api/v1',

  ENDPOINTS: {
    // Auth
    LOGIN:        '/auth/login',
    REGISTER:     '/auth/register',

    // Users
    USERS:        '/users',
    USER:         (id)  => `/users/${id}`,

    // Events
    EVENTS:       '/events',
    EVENT:        (id)  => `/events/${id}`,

    // Sectors
    SECTORS:      (eventId)             => `/events/${eventId}/sectors`,
    SECTOR:       (eventId, sectorId)   => `/events/${eventId}/sectors/${sectorId}`,

    // Seats
    SEATS_BY_SECTOR: (eventId, sectorId) => `/events/${eventId}/sectors/${sectorId}/seats`,
    SEATS:        '/seats',
    SEAT:         (id)  => `/seats/${id}`,

    // Reservations
    RESERVE_SEAT: (seatId) => `/seats/${seatId}/reservations`,
    RESERVATION:  (id)     => `/reservations/${id}`,
    RESERVATIONS: '/reservations',

    // Payments
    PAYMENT:      (reservationId) => `/reservations/${reservationId}/payments`,

    // Audit
    AUDIT_LOGS:   '/audit-logs',
  },

  /** Reservation hold window in seconds (must match backend TTL) */
  RESERVATION_TTL_SECONDS: 600,
}

