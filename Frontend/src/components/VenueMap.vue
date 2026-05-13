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

      <!-- VIP / PIT strip -->
      <rect
        x="130" y="108" width="240" height="22" rx="5"
        :fill="slotFill('vip', !!vipSector)"
        :stroke="slotStroke('vip', !!vipSector)"
        :stroke-width="hover === 'vip' ? 2 : 1"
        :class="{ clickable: vipSector }"
        @mouseenter="vipSector && (hover = 'vip')"
        @click="vipSector && emit('select', vipSector)"
      />
      <text x="250" y="119"
        text-anchor="middle" dominant-baseline="middle"
        :fill="labelFill('vip', !!vipSector)"
        font-family="Syne, sans-serif" font-size="9.5" font-weight="700" letter-spacing="3"
        class="no-ptr"
      >{{ vipSector ? vipSector.name.toUpperCase() : 'PIT' }}</text>

      <!-- Campo sections (stacked: Delantero 30%, Trasero 70%) -->
      <g
        v-for="(campo, ci) in campoSectors" :key="`campo-${ci}`"
        @mouseenter="hover = `campo-${ci}`"
        @click="emit('select', campo)"
      >
        <rect
          x="130" :y="campoY(ci)" width="240" :height="campoH(ci)" rx="6"
          :fill="slotFill(`campo-${ci}`, true)"
          :stroke="slotStroke(`campo-${ci}`, true)"
          :stroke-width="hover === `campo-${ci}` ? 2 : 1"
          class="clickable"
        />
        <text
          x="250" :y="campoY(ci) + campoH(ci) / 2 - 10"
          text-anchor="middle" dominant-baseline="middle"
          :fill="labelFill(`campo-${ci}`, true)"
          font-family="Syne, sans-serif" font-size="9.5" font-weight="700" letter-spacing="1.5"
          class="no-ptr"
        >{{ campo.name.toUpperCase() }}</text>
        <text
          x="250" :y="campoY(ci) + campoH(ci) / 2 + 10"
          text-anchor="middle"
          :fill="priceFill(`campo-${ci}`)"
          font-size="9" font-family="DM Mono, monospace"
          class="no-ptr"
        >${{ campo.price?.toLocaleString('es-AR') }}</text>
      </g>

      <!-- Fallback when no campo sector -->
      <g v-if="!campoSectors.length">
        <rect x="130" y="138" width="240" height="268" rx="6"
          fill="#0a0a0a" stroke="#191919" stroke-width="1" />
        <text x="250" y="272"
          text-anchor="middle" dominant-baseline="middle"
          fill="#252525" font-family="Syne, sans-serif" font-size="12" font-weight="700" letter-spacing="3"
        >CAMPO</text>
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

const W = 500
const H = 428

// ── Sector grouping ───────────────────────────────────────────────────────────

const campoSectors = computed(() =>
  props.sectors
    .filter(s => s.name.toLowerCase().includes('campo'))
    .sort((a, b) => {
      const aD = a.name.toLowerCase().includes('delantero')
      const bD = b.name.toLowerCase().includes('delantero')
      return aD === bD ? 0 : aD ? -1 : 1
    })
)

const vipSector = computed(() =>
  props.sectors.find(s =>
    s.name.toLowerCase().includes('vip') || s.name.toLowerCase().includes('pit')
  ) ?? null
)

const innerSectors = computed(() =>
  props.sectors
    .filter(s =>
      !s.name.toLowerCase().includes('campo') &&
      !s.name.toLowerCase().includes('vip') &&
      !s.name.toLowerCase().includes('pit')
    )
    .sort((a, b) => {
      const aLeft = a.name.toLowerCase().includes('izquierda')
      const bLeft = b.name.toLowerCase().includes('izquierda')
      if (aLeft !== bLeft) return aLeft ? -1 : 1
      const aIs2 = a.name.trimEnd().endsWith('2')
      const bIs2 = b.name.trimEnd().endsWith('2')
      return aIs2 === bIs2 ? 0 : aIs2 ? 1 : -1
    })
    .slice(0, 4)
)

const hoveredSector = computed(() => {
  const key = hover.value ?? activeKey.value
  if (!key) return null
  if (key.startsWith('campo-')) return campoSectors.value[parseInt(key.split('-')[1])] ?? null
  if (key === 'vip')            return vipSector.value
  if (key.startsWith('l'))      return innerSectors.value[parseInt(key[1])] ?? null
  if (key.startsWith('r'))      return innerSectors.value[parseInt(key[1]) + 2] ?? null
  return null
})

// ── Campo layout (Delantero 30%, Trasero 70%) ─────────────────────────────────

const CAMPO_TOP  = 138
const CAMPO_FULL = 268
const CAMPO_GAP  = 8
const CAMPO_RATIOS = [0.30, 0.70]

const campoHeights = computed(() => {
  const n = campoSectors.value.length
  if (n <= 1) return [CAMPO_FULL]
  const usable = CAMPO_FULL - (n - 1) * CAMPO_GAP
  return CAMPO_RATIOS.slice(0, n).map(r => Math.round(usable * r))
})

function campoH(i) {
  return campoHeights.value[i] ?? campoHeights.value[0]
}

function campoY(i) {
  let y = CAMPO_TOP
  for (let j = 0; j < i; j++) y += campoHeights.value[j] + CAMPO_GAP
  return y
}

// ── Side slots (Platea 1 = 70% height, Platea 2 = 30%) ───────────────────────

const LSLOTS = [
  { x: 20,  y: 138, w: 102, h: 188 },
  { x: 20,  y: 334, w: 102, h:  72 },
]

const RSLOTS = [
  { x: 378, y: 138, w: 102, h: 188 },
  { x: 378, y: 334, w: 102, h:  72 },
]

// ── Active key: resolves activeId (from sidebar hover) to an internal slot key ─

const activeKey = computed(() => {
  if (!props.activeId) return null
  if (vipSector.value?.id === props.activeId)   return 'vip'
  const ci = campoSectors.value.findIndex(s => s.id === props.activeId)
  if (ci !== -1) return `campo-${ci}`
  const li = innerSectors.value.findIndex((s, i) => i < 2  && s?.id === props.activeId)
  if (li !== -1) return `l${li}`
  const ri = innerSectors.value.findIndex((s, i) => i >= 2 && s?.id === props.activeId)
  if (ri !== -1) return `r${ri - 2}`
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
