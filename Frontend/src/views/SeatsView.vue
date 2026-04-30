<template>
  <div class="seats-page">
    <div class="container">

      <!-- Breadcrumb -->
      <nav class="breadcrumb" aria-label="breadcrumb">
        <router-link to="/">Eventos</router-link>
        <span class="sep">›</span>
        <router-link :to="`/events/${eventId}`">{{ sector?.eventName ?? 'Evento' }}</router-link>
        <span class="sep">›</span>
        <span>{{ sector?.name ?? 'Sector' }}</span>
      </nav>

      <!-- Sector info -->
      <div v-if="sector" class="sector-info">
        <div>
          <p class="eyebrow">Sector</p>
          <h1 class="sector-title">{{ sector.name }}</h1>
        </div>
        <div class="sector-price-block">
          <p class="price-label">Precio por asiento</p>
          <p class="price-value">${{ sector.price?.toLocaleString('es-AR') }}</p>
        </div>
      </div>

      <!-- Loading -->
      <LoadingSpinner v-if="loading" label="Cargando asientos..." style="margin-top:var(--space-2xl)" />

      <!-- Error -->
      <div v-else-if="error" class="alert alert-error">
        <span>⚠</span> {{ error }}
        <button class="btn btn-ghost btn-sm" style="margin-left:auto" @click="load">Reintentar</button>
      </div>

      <!-- Seat grid -->
      <template v-else>
        <SeatGrid
          :seats="seats"
          :selected="selectedSeat"
          @select="handleSelect"
        />

        <!-- Selected seat drawer -->
        <transition name="drawer">
          <div v-if="selectedSeat" class="seat-drawer card">
            <div class="drawer-info">
              <div>
                <p class="drawer-label">Asiento seleccionado</p>
                <p class="drawer-seat">{{ selectedSeat.seatNumber }}</p>
              </div>
              <div>
                <p class="drawer-label">Sector</p>
                <p class="drawer-sector">{{ sector?.name }}</p>
              </div>
              <div>
                <p class="drawer-label">Precio</p>
                <p class="drawer-price">${{ sector?.price?.toLocaleString('es-AR') }}</p>
              </div>
            </div>
            <div class="drawer-actions">
              <button class="btn btn-ghost" @click="selectedSeat = null">Cancelar</button>
              <button
                class="btn btn-primary"
                :disabled="reserving"
                @click="handleReserve"
              >
                <LoadingSpinner v-if="reserving" size="18px" />
                <span v-else>Reservar asiento →</span>
              </button>
            </div>
          </div>
        </transition>

        <!-- Reserve error -->
        <div v-if="reserveError" class="alert alert-error" style="margin-top:var(--space-md)">
          <span>⚠</span> {{ reserveError }}
        </div>
      </template>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { sectorsApi, seatsApi } from '@/api/events'
import { useAuthStore } from '@/stores/auth'
import { useReservationStore } from '@/stores/reservation'
import SeatGrid from '@/components/SeatGrid.vue'
import LoadingSpinner from '@/components/LoadingSpinner.vue'

const props = defineProps({
  eventId:  { type: String, required: true },
  sectorId: { type: String, required: true },
})

const auth        = useAuthStore()
const resStore    = useReservationStore()
const router      = useRouter()

const sector       = ref(null)
const seats        = ref([])
const loading      = ref(false)
const error        = ref(null)
const selectedSeat = ref(null)
const reserving    = ref(false)
const reserveError = ref(null)

// Highlight selected seat in the grid by patching seat list
function handleSelect(seat) {
  selectedSeat.value = seat
  reserveError.value = null
}

async function load() {
  loading.value = true
  error.value   = null
  try {
    const [sec, seatList] = await Promise.all([
      sectorsApi.get(props.eventId, props.sectorId),
      seatsApi.listBySector(props.eventId, props.sectorId),
    ])
    sector.value = sec
    seats.value  = seatList.sort((a, b) => 
      a.seatNumber.localeCompare(b.seatNumber, undefined, { numeric: true })
    )
  } catch (e) {
    error.value = e.message
  } finally {
    loading.value = false
  }
}

async function handleReserve() {
  if (!selectedSeat.value || !auth.userId) return
  reserving.value    = true
  reserveError.value = null
  try {
    const ok = await resStore.reserve(
      selectedSeat.value.id,
      auth.userId,
      selectedSeat.value,
      sector.value,
      { id: props.eventId, name: sector.value?.eventName },
    )
    if (ok) {
      router.push('/reservation')
    } else {
      reserveError.value = resStore.error ?? 'No se pudo reservar el asiento.'
    }
  } finally {
    reserving.value = false
  }
}

onMounted(load)
</script>

<style scoped>
.seats-page { padding-block: var(--space-xl) var(--space-2xl); }

.breadcrumb {
  display: flex; align-items: center; gap: 8px;
  font-size: .75rem; color: var(--c-text-3);
  margin-bottom: var(--space-xl);
  letter-spacing: .04em;
}
.breadcrumb a { transition: color var(--t-fast); }
.breadcrumb a:hover { color: var(--c-gold); }
.sep { color: var(--c-border-2); }

.sector-info {
  display: flex; align-items: flex-start; justify-content: space-between;
  gap: var(--space-lg);
  margin-bottom: var(--space-xl);
  flex-wrap: wrap;
}
.eyebrow {
  font-size: .7rem; font-weight: 700;
  letter-spacing: .14em; text-transform: uppercase;
  color: var(--c-gold); margin-bottom: 4px;
}
.sector-title {
  font-family: var(--f-display);
  font-size: clamp(1.6rem, 3vw, 2.4rem);
  font-weight: 600;
  color: var(--c-text);
}
.sector-price-block { text-align: right; }
.price-label { font-size: .72rem; color: var(--c-text-3); letter-spacing: .06em; text-transform: uppercase; margin-bottom: 4px; }
.price-value { font-family: var(--f-mono); font-size: 1.5rem; color: var(--c-gold); }

/* Drawer */
.seat-drawer {
  margin-top: var(--space-xl);
  padding: var(--space-lg);
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--space-lg);
  flex-wrap: wrap;
  border-color: var(--c-gold-dim);
  background: rgba(201,168,76,.04);
}
.drawer-info {
  display: flex; gap: var(--space-xl); flex-wrap: wrap;
}
.drawer-label { font-size: .7rem; letter-spacing: .1em; text-transform: uppercase; color: var(--c-text-3); margin-bottom: 4px; }
.drawer-seat  { font-family: var(--f-mono); font-size: 1.1rem; color: var(--c-gold); font-weight: 500; }
.drawer-sector { font-size: .95rem; color: var(--c-text); font-weight: 600; }
.drawer-price  { font-family: var(--f-mono); font-size: 1.1rem; color: var(--c-text); }
.drawer-actions { display: flex; gap: 10px; }

.drawer-enter-active, .drawer-leave-active { transition: all .3s ease; }
.drawer-enter-from, .drawer-leave-to { opacity: 0; transform: translateY(12px); }
</style>
