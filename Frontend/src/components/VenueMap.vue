<template>
  <div class="venue-wrap" @mouseleave="hover = null">
    <svg :viewBox="`0 0 ${W} ${H}`" class="venue-svg">

      <!-- Arena boundary -->
      <rect x="6" y="6" :width="W - 12" :height="H - 12" rx="22"
        fill="#0c0c0c" stroke="#1e1e1e" stroke-width="1.5" />

      <!-- Stage -->
      <rect x="130" y="22" width="240" height="78" rx="8"
        fill="#131313" stroke="#4a3918" stroke-width="1.5" />
      <text x="250" y="61"
        text-anchor="middle" dominant-baseline="middle"
        fill="#4a3918" font-family="Syne, sans-serif"
        font-size="11" font-weight="700" letter-spacing="4"
      >ESCENARIO</text>

      <!-- VIP / PIT strip (only when the sector exists) -->
      <g v-if="vipSector"
        @mouseenter="hover = 'vip'"
        @click="emit('select', vipSector)"
      >
        <rect
          x="130" y="108" width="240" height="22" rx="5"
          :fill="slotFill('vip', true)"
          :stroke="slotStroke('vip', true)"
          :stroke-width="isOn('vip') ? 2 : 1"
          class="clickable"
        />
        <text x="250" y="119"
          text-anchor="middle" dominant-baseline="middle"
          :fill="labelFill('vip', true)"
          font-family="Syne, sans-serif" font-size="9.5" font-weight="700" letter-spacing="3"
          class="no-ptr"
        >{{ vipSector.name.toUpperCase() }}</text>
        <text x="250" y="128"
          text-anchor="middle"
          :fill="priceFill('vip')"
          font-size="7.5" font-family="DM Mono, monospace"
          class="no-ptr"
        >${{ vipSector.price?.toLocaleString('es-AR') }}</text>
      </g>

      <!-- Campo — single unified central area -->
      <g
        :class="{ clickable: campoSector }"
        @mouseenter="campoSector && (hover = 'campo')"
        @click="campoSector && emit('select', campoSector)"
      >
        <rect
          x="130" :y="CAMPO_TOP" width="240" :height="CAMPO_FULL" rx="6"
          :fill="slotFill('campo', !!campoSector)"
          :stroke="slotStroke('campo', !!campoSector)"
          :stroke-width="isOn('campo') ? 2 : 1"
        />
        <text
          x="250" :y="CAMPO_TOP + CAMPO_FULL / 2 - 10"
          text-anchor="middle" dominant-baseline="middle"
          :fill="labelFill('campo', !!campoSector)"
          font-family="Syne, sans-serif" font-size="9.5" font-weight="700" letter-spacing="1.5"
          class="no-ptr"
        >{{ campoSector ? campoSector.name.toUpperCase() : 'CAMPO' }}</text>
        <text
          v-if="campoSector"
          x="250" :y="CAMPO_TOP + CAMPO_FULL / 2 + 10"
          text-anchor="middle"
          :fill="priceFill('campo')"
          font-size="9" font-family="DM Mono, monospace"
          class="no-ptr"
        >${{ campoSector.price?.toLocaleString('es-AR') }}</text>
      </g>

      <!-- Left side slots -->
      <g
        v-for="(slot, i) in LSLOTS" :key="`l${i}`"
        @mouseenter="innerSectors[i] && (hover = `l${i}`)"
        @click="innerSectors[i] && emit('select', innerSectors[i])"
      >
        <rect
          :x="slot.x" :y="slot.y" :width="slot.w" :height="slot.h" rx="5"
          :fill="slotFill(`l${i}`, !!innerSectors[i])"
          :stroke="slotStroke(`l${i}`, !!innerSectors[i])"
          :stroke-width="hover === `l${i}` ? 2 : 1"
          :class="{ clickable: innerSectors[i] }"
        />
        <template v-if="innerSectors[i]">
          <text
            :x="slot.x + slot.w / 2" :y="slot.y + slot.h / 2 - 12"
            text-anchor="middle"
            :fill="labelFill(`l${i}`, true)"
            font-size="7.5" font-family="Syne, sans-serif" font-weight="700"
            class="no-ptr"
          >{{ nameLine1(innerSectors[i].name) }}</text>
          <text
            :x="slot.x + slot.w / 2" :y="slot.y + slot.h / 2 + 2"
            text-anchor="middle"
            :fill="labelFill(`l${i}`, true)"
            font-size="7.5" font-family="Syne, sans-serif" font-weight="700"
            class="no-ptr"
          >{{ nameLine2(innerSectors[i].name) }}</text>
          <text
            :x="slot.x + slot.w / 2" :y="slot.y + slot.h / 2 + 15"
            text-anchor="middle"
            :fill="priceFill(`l${i}`)"
            font-size="7.5" font-family="DM Mono, monospace"
            class="no-ptr"
          >${{ innerSectors[i].price?.toLocaleString('es-AR') }}</text>
        </template>
      </g>

      <!-- Right side slots -->
      <g
        v-for="(slot, i) in RSLOTS" :key="`r${i}`"
        @mouseenter="innerSectors[i + 2] && (hover = `r${i}`)"
        @click="innerSectors[i + 2] && emit('select', innerSectors[i + 2])"
      >
        <rect
          :x="slot.x" :y="slot.y" :width="slot.w" :height="slot.h" rx="5"
          :fill="slotFill(`r${i}`, !!innerSectors[i + 2])"
          :stroke="slotStroke(`r${i}`, !!innerSectors[i + 2])"
          :stroke-width="hover === `r${i}` ? 2 : 1"
          :class="{ clickable: innerSectors[i + 2] }"
        />
        <template v-if="innerSectors[i + 2]">
          <text
            :x="slot.x + slot.w / 2" :y="slot.y + slot.h / 2 - 12"
            text-anchor="middle"
            :fill="labelFill(`r${i}`, true)"
            font-size="7.5" font-family="Syne, sans-serif" font-weight="700"
            class="no-ptr"
          >{{ nameLine1(innerSectors[i + 2].name) }}</text>
          <text
            :x="slot.x + slot.w / 2" :y="slot.y + slot.h / 2 + 2"
            text-anchor="middle"
            :fill="labelFill(`r${i}`, true)"
            font-size="7.5" font-family="Syne, sans-serif" font-weight="700"
            class="no-ptr"
          >{{ nameLine2(innerSectors[i + 2].name) }}</text>
          <text
            :x="slot.x + slot.w / 2" :y="slot.y + slot.h / 2 + 15"
            text-anchor="middle"
            :fill="priceFill(`r${i}`)"
            font-size="7.5" font-family="DM Mono, monospace"
            class="no-ptr"
          >${{ innerSectors[i + 2].price?.toLocaleString('es-AR') }}</text>
        </template>
      </g>

      <!-- Extra sectors — stacked strips at the back of the venue -->
      <g
        v-for="(sector, i) in extraSectors" :key="`extra-${i}`"
        @mouseenter="hover = `extra-${i}`"
        @click="emit('select', sector)"
      >
        <rect
          :x="20" :y="EXTRA_TOP + i * (EXTRA_H + EXTRA_GAP)"
          :width="W - 40" :height="EXTRA_H" rx="5"
          :fill="slotFill(`extra-${i}`, true)"
          :stroke="slotStroke(`extra-${i}`, true)"
          :stroke-width="isOn(`extra-${i}`) ? 2 : 1"
          class="clickable"
        />
        <text
          :x="W / 2" :y="EXTRA_TOP + i * (EXTRA_H + EXTRA_GAP) + EXTRA_H / 2 - 8"
          text-anchor="middle"
          :fill="labelFill(`extra-${i}`, true)"
          font-size="9" font-family="Syne, sans-serif" font-weight="700" letter-spacing="1"
          class="no-ptr"
        >{{ sector.name.toUpperCase() }}</text>
        <text
          :x="W / 2" :y="EXTRA_TOP + i * (EXTRA_H + EXTRA_GAP) + EXTRA_H / 2 + 8"
          text-anchor="middle"
          :fill="priceFill(`extra-${i}`)"
          font-size="8.5" font-family="DM Mono, monospace"
          class="no-ptr"
        >${{ sector.price?.toLocaleString('es-AR') }}</text>
      </g>

    </svg>

    <!-- Sector info bar -->
    <div class="venue-info">
      <transition name="info" mode="out-in">
        <div v-if="hoveredSector" :key="hoveredSector.name" class="venue-info__sector">
          <div class="venue-info__left">
            <p class="info-name">{{ hoveredSector.name }}</p>
            <p class="info-meta">{{ hoveredSector.capacity }} asientos</p>
          </div>
          <div class="venue-info__right">
            <span class="info-price">${{ hoveredSector.price?.toLocaleString('es-AR') }}</span>
            <button class="btn btn-primary btn-sm" @click="emit('select', hoveredSector)">
              Ver asientos →
            </button>
          </div>
        </div>
        <div v-else key="placeholder" class="venue-info__placeholder">
          Posicioná el cursor sobre un sector para ver detalles
        </div>
      </transition>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'

const props = defineProps({
  sectors:  { type: Array,  required: true },
  activeId: { type: Number, default: null  },
})

const emit = defineEmits(['select'])

const hover = ref(null)

const W          = 500
const H_BASE     = 428
const EXTRA_H    = 42
const EXTRA_GAP  = 6
const EXTRA_TOP  = 414  // CAMPO_BOTTOM (406) + 8px gap

// ── Sector grouping — by name (position reflects meaning, not creation order) ─

const vipSector = computed(() =>
  props.sectors.find(s =>
    s.name.toLowerCase().includes('vip') || s.name.toLowerCase().includes('pit')
  ) ?? null
)

// Single campo sector → full central area
const campoSector = computed(() =>
  props.sectors.find(s => s.name.toLowerCase().includes('campo')) ?? null
)

// Known side sectors keep their slot; generics fill remaining slots and go last
function innerPriority(name) {
  const n = name.toLowerCase()
  if (n.includes('izquierda') && !n.trimEnd().endsWith('2')) return 0
  if (n.includes('izquierda') &&  n.trimEnd().endsWith('2')) return 1
  if (n.includes('derecha')   && !n.trimEnd().endsWith('2')) return 2
  if (n.includes('derecha')   &&  n.trimEnd().endsWith('2')) return 3
  return 4
}

// Side slots fill by creation order (first 4 non-VIP, non-campo sectors)
const innerSectors = computed(() =>
  props.sectors
    .filter(s =>
      !s.name.toLowerCase().includes('campo') &&
      !s.name.toLowerCase().includes('vip') &&
      !s.name.toLowerCase().includes('pit')
    )
    .slice(0, 4)
)

const extraSectors = computed(() => {
  const used = new Set([
    vipSector.value?.id,
    campoSector.value?.id,
    ...innerSectors.value.map(s => s.id),
  ].filter(Boolean))
  return props.sectors.filter(s => !used.has(s.id))
})

const hoveredSector = computed(() => {
  const key = hover.value ?? activeKey.value
  if (!key) return null
  if (key === 'vip')            return vipSector.value
  if (key === 'campo')          return campoSector.value
  if (key.startsWith('l'))      return innerSectors.value[parseInt(key[1])] ?? null
  if (key.startsWith('r'))      return innerSectors.value[parseInt(key[1]) + 2] ?? null
  if (key.startsWith('extra-')) return extraSectors.value[parseInt(key.split('-')[1])] ?? null
  return null
})

// ── Campo layout (Delantero 30%, Trasero 70%) ─────────────────────────────────

const CAMPO_TOP  = computed(() => vipSector.value ? 138 : 108)
const CAMPO_FULL = computed(() => 406 - CAMPO_TOP.value)

// ── Side slots — derived from CAMPO_TOP so they follow when VIP disappears ───

const LSLOTS = computed(() => {
  const top  = CAMPO_TOP.value
  const full = CAMPO_FULL.value
  const h1   = Math.round(full * 0.70)
  const h2   = full - h1 - 8
  return [
    { x: 20,  y: top,         w: 102, h: h1 },
    { x: 20,  y: top + h1 + 8, w: 102, h: h2 },
  ]
})

const RSLOTS = computed(() => {
  const top  = CAMPO_TOP.value
  const full = CAMPO_FULL.value
  const h1   = Math.round(full * 0.70)
  const h2   = full - h1 - 8
  return [
    { x: 378, y: top,          w: 102, h: h1 },
    { x: 378, y: top + h1 + 8, w: 102, h: h2 },
  ]
})

const H = computed(() => {
  const n = extraSectors.value.length
  return n > 0 ? H_BASE + n * (EXTRA_H + EXTRA_GAP) : H_BASE
})

// ── Active key: resolves activeId (from sidebar hover) to an internal slot key ─

const activeKey = computed(() => {
  if (!props.activeId) return null
  if (vipSector.value?.id   === props.activeId) return 'vip'
  if (campoSector.value?.id === props.activeId) return 'campo'
  const li = innerSectors.value.findIndex((s, i) => i < 2  && s?.id === props.activeId)
  if (li !== -1) return `l${li}`
  const ri = innerSectors.value.findIndex((s, i) => i >= 2 && s?.id === props.activeId)
  if (ri !== -1) return `r${ri - 2}`
  const ei = extraSectors.value.findIndex(s => s.id === props.activeId)
  if (ei !== -1) return `extra-${ei}`
  return null
})

function isOn(key) {
  return hover.value === key || activeKey.value === key
}

// ── Color helpers (keeps template clean) ─────────────────────────────────────

function slotFill(key, hasContent) {
  if (!hasContent) return '#111'
  return isOn(key) ? 'rgba(201,168,76,.20)' : 'rgba(201,168,76,.09)'
}

function slotStroke(key, hasContent) {
  if (!hasContent) return '#191919'
  return isOn(key) ? '#C9A84C' : '#5a4a22'
}

function labelFill(key, hasContent) {
  if (!hasContent) return '#2e2e2e'
  return isOn(key) ? '#E8C96A' : '#8a6a2e'
}

function priceFill(key) {
  return isOn(key) ? 'rgba(232,201,106,.75)' : 'rgba(120,90,32,.75)'
}

// ── Text helpers ──────────────────────────────────────────────────────────────

function nameLine1(name) {
  const idx = name.indexOf(' ')
  return idx === -1 ? name : name.slice(0, idx)
}

function nameLine2(name) {
  const idx = name.indexOf(' ')
  return idx === -1 ? '' : name.slice(idx + 1)
}
</script>

<style scoped>
.venue-wrap {
  position: relative;
  width: 100%;
}

.venue-svg {
  width: 100%;
  height: auto;
  display: block;
}

.clickable { cursor: pointer; }
.no-ptr    { pointer-events: none; }

.venue-info {
  margin-top: var(--space-md);
  min-height: 64px;
  display: flex;
  align-items: center;
  padding: var(--space-md) var(--space-lg);
  background: var(--c-surface);
  border: 1px solid var(--c-border);
  border-radius: var(--r-md);
}

.venue-info__sector {
  display: flex;
  align-items: center;
  justify-content: space-between;
  width: 100%;
  gap: var(--space-lg);
}

.venue-info__left { display: flex; flex-direction: column; gap: 3px; }

.info-name {
  font-family: var(--f-ui);
  font-size: .95rem;
  font-weight: 700;
  color: var(--c-gold);
  letter-spacing: .04em;
  text-transform: uppercase;
}
.info-meta {
  font-size: .78rem;
  color: var(--c-text-3);
  letter-spacing: .04em;
}

.venue-info__right {
  display: flex;
  align-items: center;
  gap: var(--space-lg);
  flex-shrink: 0;
}

.info-price {
  font-family: var(--f-mono);
  font-size: 1.3rem;
  font-weight: 500;
  color: var(--c-text);
}

.venue-info__placeholder {
  font-size: .82rem;
  color: var(--c-text-3);
  letter-spacing: .03em;
  width: 100%;
  text-align: center;
}

.info-enter-active, .info-leave-active { transition: opacity .15s ease; }
.info-enter-from, .info-leave-to       { opacity: 0; }
</style>
