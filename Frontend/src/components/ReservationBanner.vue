<template>
  <div class="banner" :class="{ urgent: secondsLeft < 120 }">
    <div class="container banner-inner">
      <div class="banner-info">
        <span class="banner-icon">⏱</span>
        <span class="banner-text">
          Asiento reservado:
          <strong class="seat-id">{{ reservation.seat?.seatNumber }}</strong>
          — tu reserva vence en
        </span>
        <span class="countdown" :class="{ red: secondsLeft < 60 }">
          {{ formattedTime }}
        </span>
      </div>
      <div class="banner-actions">
        <router-link to="/payment" class="btn btn-primary btn-sm">
          Pagar ahora
        </router-link>
        <button class="btn btn-ghost btn-sm" @click="handleRelease">
          Liberar
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed, onMounted, onUnmounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useReservationStore } from '@/stores/reservation'

const reservation = useReservationStore()
const router      = useRouter()
const secondsLeft = ref(reservation.secondsRemaining)
let timer

onMounted(() => {
  timer = setInterval(() => {
    secondsLeft.value = reservation.secondsRemaining
    if (secondsLeft.value <= 0) {
      reservation.clear()
      router.push('/')
    }
  }, 1000)
})

onUnmounted(() => clearInterval(timer))

const formattedTime = computed(() => {
  const m = Math.floor(secondsLeft.value / 60)
  const s = secondsLeft.value % 60
  return `${String(m).padStart(2, '0')}:${String(s).padStart(2, '0')}`
})

function handleRelease() {
  reservation.clear()
}
</script>

<style scoped>
.banner {
  background: var(--c-surface-2);
  border-bottom: 1px solid var(--c-border);
  transition: background var(--t-base);
}
.banner.urgent {
  background: rgba(201, 168, 76, .08);
  border-bottom-color: var(--c-gold-dim);
  animation: pulse-glow 2s ease infinite;
}
.banner-inner {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--space-md);
  min-height: 48px;
  padding-block: 10px;
  flex-wrap: wrap;
}
.banner-info {
  display: flex;
  align-items: center;
  gap: 10px;
  font-size: .85rem;
  color: var(--c-text-2);
  flex-wrap: wrap;
}
.banner-icon { font-size: 1rem; }
.seat-id {
  font-family: var(--f-mono);
  color: var(--c-gold);
  font-size: .8rem;
}
.countdown {
  font-family: var(--f-mono);
  font-size: 1rem;
  font-weight: 500;
  color: var(--c-gold);
  background: rgba(201,168,76,.1);
  padding: 2px 10px;
  border-radius: 4px;
}
.countdown.red { color: #e06b5e; background: var(--c-red-bg); }
.banner-actions { display: flex; gap: 8px; flex-shrink: 0; }
</style>
