<template>
  <div class="confirmation-page">
    <div class="container narrow">

      <div v-if="!res.paymentDone" class="empty-state">
        <p class="empty-glyph">◎</p>
        <p>No hay compra para confirmar.</p>
        <router-link to="/" class="btn btn-outline" style="margin-top:var(--space-lg)">Ver eventos</router-link>
      </div>

      <template v-else>
        <!-- Success animation -->
        <div class="success-icon">
          <svg viewBox="0 0 80 80" fill="none">
            <circle cx="40" cy="40" r="38" stroke="var(--c-gold)" stroke-width="2" class="circle-draw" />
            <path d="M24 41L35 52L56 30" stroke="var(--c-gold)" stroke-width="3" stroke-linecap="round" stroke-linejoin="round" class="check-draw" />
          </svg>
        </div>

        <p class="eyebrow">¡Compra exitosa!</p>
        <h1 class="page-title">Tu entrada está confirmada</h1>
        <p class="page-desc">Recibirás un correo con los detalles. Guardá este comprobante.</p>

        <!-- Ticket card -->
        <div class="ticket-card">
          <div class="ticket-left">
            <p class="ticket-event">{{ res.event?.name ?? '—' }}</p>
            <p class="ticket-sector">{{ res.sector?.name }} — Asiento <span class="mono">{{ res.seat?.seatNumber }}</span></p>
            <p class="ticket-id">Reserva: <span class="mono">{{ shortId }}</span></p>
          </div>
          <div class="ticket-right">
            <p class="ticket-price">${{ res.sector?.price?.toLocaleString('es-AR') }}</p>
            <span class="badge badge-green">Pagado</span>
          </div>
          <!-- Decorative perforations -->
          <div class="perforation"></div>
        </div>

        <div class="action-row">
          <button class="btn btn-ghost" @click="handleDone">Volver a eventos</button>
        </div>
      </template>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import { useReservationStore } from '@/stores/reservation'

const res    = useReservationStore()
const router = useRouter()

const shortId = computed(() => res.reservationId?.slice(0, 8)?.toUpperCase() ?? '—')

function handleDone() {
  res.clear()
  router.push('/')
}
</script>

<style scoped>
.confirmation-page { padding-block: var(--space-2xl); }
.narrow { max-width: 560px; }

.success-icon {
  width: 80px; height: 80px;
  margin: 0 auto var(--space-xl);
}
.circle-draw {
  stroke-dasharray: 240;
  stroke-dashoffset: 240;
  animation: draw-circle .6s ease forwards;
}
.check-draw {
  stroke-dasharray: 50;
  stroke-dashoffset: 50;
  animation: draw-check .4s ease .5s forwards;
}
@keyframes draw-circle { to { stroke-dashoffset: 0; } }
@keyframes draw-check  { to { stroke-dashoffset: 0; } }

.eyebrow { font-size: .72rem; font-weight: 700; letter-spacing: .14em; text-transform: uppercase; color: var(--c-gold); margin-bottom: 8px; }
.page-title { font-family: var(--f-display); font-size: clamp(1.8rem, 4vw, 2.8rem); font-weight: 600; color: var(--c-text); margin-bottom: 12px; }
.page-desc  { font-size: .9rem; color: var(--c-text-3); margin-bottom: var(--space-xl); line-height: 1.7; }

/* Ticket */
.ticket-card {
  position: relative;
  background: var(--c-surface);
  border: 1px solid var(--c-gold-dim);
  border-radius: var(--r-lg);
  display: flex;
  align-items: stretch;
  overflow: hidden;
  box-shadow: var(--shadow-gold);
  margin-bottom: var(--space-xl);
}
.ticket-left { padding: var(--space-lg); flex: 1; }
.ticket-right {
  padding: var(--space-lg);
  background: rgba(201,168,76,.06);
  border-left: 2px dashed var(--c-gold-dim);
  display: flex; flex-direction: column;
  align-items: center; justify-content: center;
  gap: 10px;
  min-width: 120px;
}
.ticket-event  { font-family: var(--f-display); font-size: 1.3rem; font-weight: 600; color: var(--c-text); margin-bottom: 6px; }
.ticket-sector { font-size: .85rem; color: var(--c-text-2); margin-bottom: 4px; }
.ticket-id     { font-size: .78rem; color: var(--c-text-3); margin-top: var(--space-sm); }
.mono          { font-family: var(--f-mono); color: var(--c-gold); }
.ticket-price  { font-family: var(--f-mono); font-size: 1.3rem; color: var(--c-gold); font-weight: 500; }

.action-row { display: flex; gap: 12px; }

.empty-state { text-align: center; padding: var(--space-2xl) 0; color: var(--c-text-3); }
.empty-glyph { font-size: 3rem; color: var(--c-border-2); margin-bottom: var(--space-md); }
</style>
