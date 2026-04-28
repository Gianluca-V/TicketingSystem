import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { reservationsApi } from '@/api/reservations'
import { paymentsApi }     from '@/api/reservations'
import { API_CONFIG }      from '@/api/config'

export const useReservationStore = defineStore('reservation', () => {
  const reservationId = ref(null)
  const expiresAt     = ref(null)  // ISO string
  const seat          = ref(null)  // SeatDto snapshot
  const sector        = ref(null)  // SectorDto snapshot
  const event         = ref(null)  // EventDto snapshot
  const loading       = ref(false)
  const error         = ref(null)
  const paymentDone   = ref(false)
  const now           = ref(Date.now())

  // Keep 'now' updated every second for reactive countdowns
  setInterval(() => {
    now.value = Date.now()
  }, 1000)

  const hasActiveReservation = computed(() => !!reservationId.value && !paymentDone.value)

  const secondsRemaining = computed(() => {
    if (!expiresAt.value) return 0
    return Math.max(0, Math.floor((new Date(expiresAt.value) - now.value) / 1000))
  })

  async function reserve(seatId, userId, seatSnapshot, sectorSnapshot, eventSnapshot) {
    loading.value = true
    error.value   = null
    try {
      const data = await reservationsApi.create(seatId, userId)
      reservationId.value = data.reservationId
      expiresAt.value     = data.expiresAt
      seat.value          = seatSnapshot
      sector.value        = sectorSnapshot
      event.value         = eventSnapshot
      paymentDone.value   = false
      return true
    } catch (e) {
      error.value = e.message
      return false
    } finally {
      loading.value = false
    }
  }

  async function pay(transactionId) {
    if (!reservationId.value) return false
    loading.value = true
    error.value   = null
    try {
      await paymentsApi.process(reservationId.value, transactionId)
      paymentDone.value = true
      return true
    } catch (e) {
      error.value = e.message
      return false
    } finally {
      loading.value = false
    }
  }

  function clear() {
    reservationId.value = null
    expiresAt.value     = null
    seat.value          = null
    sector.value        = null
    event.value         = null
    paymentDone.value   = false
    error.value         = null
  }

  function clearError() { error.value = null }

  return {
    reservationId, expiresAt, seat, sector, event,
    loading, error, paymentDone,
    hasActiveReservation, secondsRemaining,
    reserve, pay, clear, clearError,
  }
})
