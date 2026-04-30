<template>
  <div class="reservation-page">
    <div class="container narrow">

      <!-- Guard: no active reservation -->
      <div v-if="!res.hasActiveReservation && !res.paymentDone" class="empty-state">
        <p class="empty-glyph">◎</p>
        <p>No tenés una reserva activa.</p>
        <router-link to="/" class="btn btn-outline" style="margin-top:var(--space-lg)">Ver eventos</router-link>
      </div>

      <template v-else-if="res.hasActiveReservation">
        <p class="eyebrow">Reserva confirmada</p>
        <h1 class="page-title">Tu asiento está reservado</h1>
        <p class="page-desc">Tenés <strong class="highlight">{{ formattedTime }}</strong> para completar el pago antes de que se libere automáticamente.</p>

        <!-- Countdown ring -->
        <div class="countdown-wrap">
          <svg class="ring" viewBox="0 0 120 120">
            <circle class="ring-track" cx="60" cy="60" r="52" fill="none" stroke-width="6" />
            <circle
              class="ring-arc"
              cx="60" cy="60" r="52"
              fill="none"
              stroke-width="6"
              stroke-linecap="round"
              :stroke-dasharray="circumference"
              :stroke-dashoffset="dashOffset"
            />
          </svg>
          <div class="ring-inner">
            <span class="ring-time" :class="{ red: res.secondsRemaining < 60 }">{{ formattedTime }}</span>
            <span class="ring-label">restantes</span>
          </div>
        </div>

        <!-- Summary card -->
        <div class="summary-card card">
          <div class="summary-row">
            <span class="s-label">Evento</span>
            <span class="s-val">{{ res.event?.name ?? '—' }}</span>
          </div>
          <div class="summary-row">
            <span class="s-label">Sector</span>
            <span class="s-val">{{ res.sector?.name ?? '—' }}</span>
          </div>
          <div class="summary-row">
            <span class="s-label">Asiento</span>
            <span class="s-val seat">{{ res.seat?.seatNumber ?? '—' }}</span>
          </div>
          <div class="summary-row total-row">
            <span class="s-label">Total</span>
            <span class="s-val price">${{ res.sector?.price?.toLocaleString('es-AR') ?? '—' }}</span>
          </div>
        </div>

        <!-- Actions -->
        <div class="action-row">
          <button class="btn btn-danger" @click="handleRelease">Liberar asiento</button>
          <router-link to="/payment" class="btn btn-primary btn-lg">Continuar al pago →</router-link>
        </div>
      </template>
    </div>
  </div>
</template>

<script setup>
import { computed, watch } from 'vue'
import { useRouter } from 'vue-router'
import { useReservationStore } from '@/stores/reservation'
import { API_CONFIG } from '@/api/config'

const res    = useReservationStore()
const router = useRouter()

const TTL         = API_CONFIG.RESERVATION_TTL_SECONDS
const circumference = 2 * Math.PI * 52  // r=52

// Automatically handle expiration
watch(() => res.secondsRemaining, (val) => {
  if (val <= 0 && res.hasActiveReservation) {
    res.clear()
    router.push('/')
  }
})

const formattedTime = computed(() => {
  const m = Math.floor(res.secondsRemaining / 60)
  const s = res.secondsRemaining % 60
  return `${String(m).padStart(2,'0')}:${String(s).padStart(2,'0')}`
})

const dashOffset = computed(() => {
  const progress = res.secondsRemaining / TTL
  return circumference * (1 - progress)
})

async function handleRelease() {
  const ok = await res.cancel()
  if (ok) router.push('/')
}
</script>

<style scoped>
.reservation-page { padding-block: var(--space-2xl); }
.narrow { max-width: 560px; }

.eyebrow { font-size: .72rem; font-weight: 700; letter-spacing: .14em; text-transform: uppercase; color: var(--c-gold); margin-bottom: 8px; }
.page-title { font-family: var(--f-display); font-size: clamp(1.8rem, 4vw, 2.8rem); font-weight: 600; color: var(--c-text); margin-bottom: 12px; }
.page-desc  { font-size: .9rem; color: var(--c-text-3); margin-bottom: var(--space-xl); line-height: 1.7; }
.highlight  { color: var(--c-gold); font-family: var(--f-mono); font-weight: 500; }

.countdown-wrap {
  position: relative;
  width: 160px; height: 160px;
  margin: 0 auto var(--space-xl);
}
.ring { width: 100%; height: 100%; transform: rotate(-90deg); }
.ring-track { stroke: var(--c-border); }
.ring-arc   { stroke: var(--c-gold); transition: stroke-dashoffset 1s linear; }
.ring-inner {
  position: absolute; inset: 0;
  display: flex; flex-direction: column;
  align-items: center; justify-content: center;
}
.ring-time  { font-family: var(--f-mono); font-size: 1.6rem; font-weight: 500; color: var(--c-gold); line-height: 1; }
.ring-time.red { color: #e06b5e; }
.ring-label { font-size: .7rem; color: var(--c-text-3); letter-spacing: .08em; text-transform: uppercase; margin-top: 4px; }

.summary-card { overflow: visible; }
.summary-row {
  display: flex; align-items: center; justify-content: space-between;
  padding: 14px 20px;
  border-bottom: 1px solid var(--c-border);
  gap: 12px;
}
.summary-row:last-child { border-bottom: none; }
.s-label { font-size: .75rem; letter-spacing: .08em; text-transform: uppercase; color: var(--c-text-3); }
.s-val   { font-size: .9rem; color: var(--c-text); font-weight: 500; }
.s-val.seat  { font-family: var(--f-mono); color: var(--c-gold); }
.total-row   { background: rgba(201,168,76,.04); }
.s-val.price { font-family: var(--f-mono); font-size: 1.1rem; color: var(--c-gold); }

.action-row {
  display: flex; gap: 12px; flex-wrap: wrap;
  margin-top: var(--space-xl);
  align-items: center;
  justify-content: space-between;
}

.empty-state { text-align: center; padding: var(--space-2xl) 0; color: var(--c-text-3); }
.empty-glyph { font-size: 3rem; color: var(--c-border-2); margin-bottom: var(--space-md); }
</style>
