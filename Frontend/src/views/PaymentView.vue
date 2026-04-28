<template>
  <div class="payment-page">
    <div class="container narrow">

      <!-- Guard -->
      <div v-if="!res.hasActiveReservation" class="empty-state">
        <p class="empty-glyph">◎</p>
        <p>No hay reserva activa para pagar.</p>
        <router-link to="/" class="btn btn-outline" style="margin-top:var(--space-lg)">Ver eventos</router-link>
      </div>

      <template v-else>
        <router-link to="/reservation" class="back-link">← Volver a reserva</router-link>

        <p class="eyebrow">Paso final</p>
        <h1 class="page-title">Completar pago</h1>

        <!-- Countdown pill -->
        <div class="countdown-pill" :class="{ urgent: res.secondsRemaining < 120 }">
          <span>⏱</span>
          <span>Reserva vence en <strong class="mono">{{ formattedTime }}</strong></span>
        </div>

        <div class="layout">
          <!-- Order summary -->
          <aside class="summary-panel card">
            <p class="panel-title">Resumen de compra</p>
            <div class="summary-rows">
              <div class="summary-row">
                <span>{{ res.event?.name }}</span>
              </div>
              <div class="summary-row">
                <span class="s-label">Sector</span>
                <span>{{ res.sector?.name }}</span>
              </div>
              <div class="summary-row">
                <span class="s-label">Asiento</span>
                <span class="mono">{{ res.seat?.seatNumber }}</span>
              </div>
              <div class="summary-row total-row">
                <span class="s-label">Total</span>
                <span class="total-price">${{ res.sector?.price?.toLocaleString('es-AR') }}</span>
              </div>
            </div>
          </aside>

          <!-- Payment form -->
          <div class="payment-panel">
            <div v-if="res.error" class="alert alert-error" style="margin-bottom:var(--space-md)">
              <span>⚠</span> {{ res.error }}
              <button class="btn btn-ghost btn-sm" style="margin-left:auto" @click="res.clearError">✕</button>
            </div>

            <form class="pay-form card" @submit.prevent="handlePay">
              <p class="panel-title">Datos de pago</p>

              <!-- Simulated card fields -->
              <div class="form-group">
                <label class="form-label">Número de tarjeta</label>
                <input
                  v-model="card.number"
                  class="form-input mono-input"
                  type="text"
                  placeholder="4242 4242 4242 4242"
                  maxlength="19"
                  required
                  @input="formatCard"
                />
              </div>

              <div class="form-row-2">
                <div class="form-group">
                  <label class="form-label">Vencimiento</label>
                  <input
                    v-model="card.expiry"
                    class="form-input mono-input"
                    type="text"
                    placeholder="MM/AA"
                    maxlength="5"
                    required
                    @input="formatExpiry"
                  />
                </div>
                <div class="form-group">
                  <label class="form-label">CVV</label>
                  <input
                    v-model="card.cvv"
                    class="form-input mono-input"
                    type="password"
                    placeholder="•••"
                    maxlength="4"
                    required
                  />
                </div>
              </div>

              <div class="form-group">
                <label class="form-label">Nombre en la tarjeta</label>
                <input
                  v-model.trim="card.name"
                  class="form-input"
                  type="text"
                  placeholder="Tal como figura en la tarjeta"
                  required
                />
              </div>

              <div class="secure-badge">
                <span>🔒</span>
                <span>Pago seguro simulado — no se procesarán datos reales</span>
              </div>

              <button
                type="submit"
                class="btn btn-primary btn-lg"
                style="width:100%;justify-content:center"
                :disabled="res.loading"
              >
                <LoadingSpinner v-if="res.loading" size="18px" />
                <span v-else>Pagar ${{ res.sector?.price?.toLocaleString('es-AR') }} →</span>
              </button>
            </form>
          </div>
        </div>
      </template>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, watch } from 'vue'
import { useRouter } from 'vue-router'
import { useReservationStore } from '@/stores/reservation'
import LoadingSpinner from '@/components/LoadingSpinner.vue'

const res    = useReservationStore()
const router = useRouter()

const card = ref({ number: '', expiry: '', cvv: '', name: '' })

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

function formatCard() {
  card.value.number = card.value.number
    .replace(/\D/g, '').slice(0, 16)
    .replace(/(.{4})/g, '$1 ').trim()
}
function formatExpiry() {
  let v = card.value.expiry.replace(/\D/g, '').slice(0, 4)
  if (v.length >= 3) v = v.slice(0,2) + '/' + v.slice(2)
  card.value.expiry = v
}

async function handlePay() {
  // In a real app, the card processor returns a transactionId.
  // Here we simulate it with a UUID-like string.
  const txnId = `TXN-${Date.now()}-${Math.random().toString(36).slice(2,9).toUpperCase()}`
  const ok = await res.pay(txnId)
  if (ok) router.push('/confirmation')
}
</script>

<style scoped>
.payment-page { padding-block: var(--space-xl) var(--space-2xl); }
.narrow { max-width: 800px; }

.back-link { display: inline-flex; align-items: center; gap: 6px; font-size: .78rem; font-weight: 700; letter-spacing: .08em; text-transform: uppercase; color: var(--c-text-3); transition: color var(--t-fast); margin-bottom: var(--space-xl); }
.back-link:hover { color: var(--c-gold); }

.eyebrow { font-size: .72rem; font-weight: 700; letter-spacing: .14em; text-transform: uppercase; color: var(--c-gold); margin-bottom: 8px; }
.page-title { font-family: var(--f-display); font-size: clamp(1.8rem, 4vw, 2.8rem); font-weight: 600; color: var(--c-text); margin-bottom: var(--space-md); }

.countdown-pill {
  display: inline-flex; align-items: center; gap: 8px;
  padding: 8px 16px;
  background: rgba(201,168,76,.08);
  border: 1px solid var(--c-gold-dim);
  border-radius: 99px;
  font-size: .82rem; color: var(--c-text-2);
  margin-bottom: var(--space-xl);
  transition: all var(--t-base);
}
.countdown-pill.urgent { background: var(--c-red-bg); border-color: rgba(192,57,43,.3); color: #e06b5e; animation: pulse-glow 1.5s ease infinite; }
.mono { font-family: var(--f-mono); color: var(--c-gold); font-weight: 500; }
.countdown-pill.urgent .mono { color: #e06b5e; }

.layout {
  display: grid;
  grid-template-columns: 1fr 1.4fr;
  gap: var(--space-lg);
  align-items: start;
}
@media (max-width: 640px) { .layout { grid-template-columns: 1fr; } }

.panel-title { font-size: .75rem; font-weight: 700; letter-spacing: .1em; text-transform: uppercase; color: var(--c-text-3); margin-bottom: var(--space-md); padding: var(--space-md) var(--space-md) 0; }
.summary-panel { overflow: visible; }
.summary-rows { padding-bottom: 4px; }
.summary-row { display: flex; justify-content: space-between; padding: 10px var(--space-md); border-bottom: 1px solid var(--c-border); font-size: .875rem; color: var(--c-text); gap: 12px; }
.summary-row:last-child { border-bottom: none; }
.s-label { color: var(--c-text-3); font-size: .78rem; }
.total-row { background: rgba(201,168,76,.04); }
.total-price { font-family: var(--f-mono); font-size: 1.1rem; color: var(--c-gold); font-weight: 500; }

.pay-form { padding: var(--space-md); display: flex; flex-direction: column; gap: var(--space-md); }
.pay-form .panel-title { padding: 0; }
.form-row-2 { display: grid; grid-template-columns: 1fr 1fr; gap: var(--space-md); }
.mono-input { font-family: var(--f-mono); letter-spacing: .06em; }
.secure-badge { display: flex; align-items: center; gap: 8px; font-size: .75rem; color: var(--c-text-3); background: var(--c-surface-2); padding: 10px 14px; border-radius: var(--r-sm); }

.empty-state { text-align: center; padding: var(--space-2xl) 0; color: var(--c-text-3); }
.empty-glyph { font-size: 3rem; color: var(--c-border-2); margin-bottom: var(--space-md); }
</style>
