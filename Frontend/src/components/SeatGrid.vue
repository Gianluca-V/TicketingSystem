<template>
  <div class="seat-grid-wrapper">
    <!-- Legend -->
    <div class="legend">
      <div class="legend-item">
        <span class="seat-dot available"></span> Disponible
      </div>
      <div class="legend-item">
        <span class="seat-dot reserved"></span> Reservado
      </div>
      <div class="legend-item">
        <span class="seat-dot sold"></span> Vendido
      </div>
      <div class="legend-item">
        <span class="seat-dot selected"></span> Seleccionado
      </div>
    </div>

    <!-- Stage indicator -->
    <div class="stage-bar">
      <span>ESCENARIO</span>
    </div>

    <!-- Grid -->
    <div v-if="seats.length" class="seat-grid">
      <button
        v-for="seat in seats"
        :key="seat.id"
        class="seat"
        :class="getSeatClass(seat)"
        :disabled="!isSelectable(seat)"
        :title="seat.seatNumber"
        :aria-label="`Asiento ${seat.seatNumber} - ${seat.status}`"
        @click="$emit('select', seat)"
      >
        <span class="seat-num">{{ shortLabel(seat.seatNumber) }}</span>
      </button>
    </div>

    <div v-else class="empty-grid">
      <p>No hay asientos registrados para este sector.</p>
    </div>
  </div>
</template>

<script setup>
defineProps({
  seats:    { type: Array,  required: true },
  selected: { type: Object, default: null },
})

defineEmits(['select'])

function getSeatClass(seat) {
  const base = seat.status?.toLowerCase() ?? 'available'
  return {
    available: base === 'available',
    reserved:  base === 'reserved',
    sold:      base === 'sold' || base === 'confirmed',
    selected:  false, // overridden below
  }
}

// Override selected – can't do it in getSeatClass easily with prop reactivity
function getSeatClassFull(seat) {
  // handled in template via :class binding above + separate watcher
}

function isSelectable(seat) {
  return seat.status?.toLowerCase() === 'available'
}

function shortLabel(name) {
  // Show last segment: "VIP-12" → "12", "A-5" → "5"
  const parts = name?.split('-') ?? []
  return parts.at(-1) ?? name ?? '?'
}
</script>

<style scoped>
.seat-grid-wrapper { display: flex; flex-direction: column; gap: var(--space-lg); }

.legend {
  display: flex;
  gap: var(--space-lg);
  flex-wrap: wrap;
  font-size: .78rem;
  color: var(--c-text-3);
  letter-spacing: .04em;
}
.legend-item { display: flex; align-items: center; gap: 6px; }
.seat-dot {
  width: 14px; height: 14px;
  border-radius: 3px;
  flex-shrink: 0;
}
.seat-dot.available { background: var(--c-surface-3); border: 1px solid var(--c-gold-dim); }
.seat-dot.reserved  { background: var(--c-amber-bg); border: 1px solid var(--c-amber); }
.seat-dot.sold      { background: var(--c-border); border: 1px solid var(--c-border); }
.seat-dot.selected  { background: var(--c-gold); border: 1px solid var(--c-gold-light); }

.stage-bar {
  display: flex;
  align-items: center;
  justify-content: center;
  height: 36px;
  background: linear-gradient(90deg, transparent, rgba(201,168,76,.08), transparent);
  border-top: 2px solid var(--c-gold-dim);
  font-size: .7rem;
  letter-spacing: .18em;
  color: var(--c-gold-dim);
  text-transform: uppercase;
  font-weight: 700;
}

.seat-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(52px, 1fr));
  gap: 6px;
}

.seat {
  aspect-ratio: 1;
  border-radius: 6px;
  border: none;
  cursor: pointer;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  transition: all .15s ease;
  position: relative;
  overflow: hidden;
}

.seat.available {
  background: var(--c-surface-3);
  border: 1px solid var(--c-gold-dim);
  color: var(--c-text-2);
}
.seat.available:hover {
  background: rgba(201,168,76,.12);
  border-color: var(--c-gold);
  transform: scale(1.08);
  z-index: 1;
  box-shadow: 0 0 12px rgba(201,168,76,.25);
}

.seat.reserved {
  background: var(--c-amber-bg);
  border: 1px solid rgba(217,119,6,.4);
  color: var(--c-amber);
  cursor: not-allowed;
  opacity: .75;
}

.seat.sold {
  background: var(--c-surface-2);
  border: 1px solid var(--c-border);
  color: var(--c-text-3);
  cursor: not-allowed;
  opacity: .45;
}
.seat.sold::after {
  content: '×';
  position: absolute;
  font-size: 1.2rem;
  color: var(--c-text-3);
}

.seat-num {
  font-family: var(--f-mono);
  font-size: .68rem;
  font-weight: 500;
  line-height: 1;
  pointer-events: none;
}

.seat.selected-seat {
  background: var(--c-gold) !important;
  border-color: var(--c-gold-light) !important;
  color: #0a0a0a !important;
  transform: scale(1.1);
  box-shadow: 0 0 16px rgba(201,168,76,.4);
  z-index: 2;
}

.empty-grid {
  text-align: center;
  padding: var(--space-xl);
  color: var(--c-text-3);
  font-size: .9rem;
  border: 1px dashed var(--c-border);
  border-radius: var(--r-md);
}
</style>
