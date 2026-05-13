<template>
  <div class="detail-page">
    <div class="container">

      <router-link to="/" class="back-link">← Volver a eventos</router-link>

      <LoadingSpinner v-if="loading" label="Cargando evento..." style="margin-top:var(--space-2xl)" />

      <div v-else-if="error" class="alert alert-error" style="margin-top:var(--space-lg)">
        <span>⚠</span> {{ error }}
      </div>

      <div v-else-if="event" class="detail-layout">

        <!-- Left: event info -->
        <aside class="detail-aside">
          <p class="event-date badge badge-gold">{{ formattedDate }}</p>
          <h1 class="event-name">{{ event.name }}</h1>
          <p class="event-venue">
            <span class="venue-icon">◎</span>
            {{ event.venue }}
          </p>

          <hr class="divider" />

          <p class="aside-label">Sectores</p>

          <div v-if="sectorsLoading" class="sectors-list">
            <div v-for="n in 4" :key="n" class="skeleton sector-skeleton"></div>
          </div>

          <nav v-else class="sectors-list">
            <router-link
              v-for="s in sectors"
              :key="s.id"
              :to="`/events/${eventId}/sectors/${s.id}/seats`"
              class="sector-item"
              @mouseenter="hoveredSectorId = s.id"
              @mouseleave="hoveredSectorId = null"
            >
              <div class="sector-item__info">
                <span class="sector-item__name">{{ s.name }}</span>
                <span class="sector-item__cap">{{ s.capacity }} asientos</span>
              </div>
              <div class="sector-item__right">
                <span class="sector-item__price">${{ s.price?.toLocaleString('es-AR') }}</span>
                <span class="sector-item__arrow">→</span>
              </div>
            </router-link>
          </nav>
        </aside>

        <!-- Right: venue map -->
        <section class="detail-main">
          <div v-if="sectorsLoading" class="skeleton venue-skeleton"></div>

          <div v-else-if="!sectors.length" class="empty-state">
            <p>No hay sectores disponibles para este evento.</p>
          </div>

          <template v-else>
            <!-- Venue map (when sector names are recognizable) -->
            <template v-if="useVenueMap">
              <VenueMap
                :sectors="sectors.slice(0, 12)"
                :active-id="hoveredSectorId"
                @select="s => $router.push(`/events/${eventId}/sectors/${s.id}/seats`)"
              />

              <div v-if="sectors.length > 12" class="overflow-sectors">
                <p class="overflow-label">Otros sectores</p>
                <div class="overflow-chips">
                  <router-link
                    v-for="s in sectors.slice(12)"
                    :key="s.id"
                    :to="`/events/${eventId}/sectors/${s.id}/seats`"
                    class="sector-chip"
                  >{{ s.name }} · ${{ s.price?.toLocaleString('es-AR') }}</router-link>
                </div>
              </div>
            </template>

            <!-- Fallback: card grid for events with unrecognized sector names -->
            <div v-else class="sectors-grid">
              <router-link
                v-for="(s, i) in sectors"
                :key="s.id"
                :to="`/events/${eventId}/sectors/${s.id}/seats`"
                class="sector-card card fade-in"
                :style="{ animationDelay: `${i * 0.08}s` }"
              >
                <div class="sector-header">
                  <h3 class="sector-name">{{ s.name }}</h3>
                  <span class="sector-price">${{ s.price?.toLocaleString('es-AR') }}</span>
                </div>
                <div class="sector-meta">
                  <span class="capacity-wrap">
                    <span class="capacity-label">Capacidad</span>
                    <span class="capacity-val">{{ s.capacity }}</span>
                  </span>
                  <span class="arrow">→</span>
                </div>
              </router-link>
            </div>
          </template>
        </section>

      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { eventsApi, sectorsApi } from '@/api/events'
import LoadingSpinner from '@/components/LoadingSpinner.vue'
import VenueMap from '@/components/VenueMap.vue'

const props = defineProps({ eventId: { type: String, required: true } })

const event            = ref(null)
const hoveredSectorId  = ref(null)
const sectors        = ref([])
const loading        = ref(false)
const sectorsLoading = ref(false)
const error          = ref(null)

const MAP_KEYWORDS = ['vip', 'pit', 'campo', 'platea', 'izquierda', 'derecha', 'tribuna', 'palco']

const useVenueMap = computed(() =>
  sectors.value.some(s =>
    MAP_KEYWORDS.some(kw => s.name.toLowerCase().includes(kw))
  )
)

const formattedDate = computed(() => {
  if (!event.value?.date) return '—'
  return new Date(event.value.date).toLocaleDateString('es-AR', {
    weekday: 'long', day: '2-digit', month: 'long', year: 'numeric',
  })
})

onMounted(async () => {
  loading.value = true
  try {
    event.value = await eventsApi.get(props.eventId)
  } catch (e) {
    error.value = e.message
    return
  } finally {
    loading.value = false
  }

  sectorsLoading.value = true
  try {
    sectors.value = await sectorsApi.list(props.eventId)
  } catch (e) {
    error.value = e.message
  } finally {
    sectorsLoading.value = false
  }
})
</script>

<style scoped>
.detail-page { padding-block: var(--space-xl) var(--space-2xl); }

.back-link {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  font-size: .78rem;
  font-weight: 700;
  letter-spacing: .08em;
  text-transform: uppercase;
  color: var(--c-text-3);
  transition: color var(--t-fast);
  margin-bottom: var(--space-xl);
}
.back-link:hover { color: var(--c-gold); }

/* ── Two-column layout ───────────────────────────────────────────────────────── */
.detail-layout {
  display: grid;
  grid-template-columns: 300px 1fr;
  gap: var(--space-2xl);
  align-items: start;
}

/* ── Left column ─────────────────────────────────────────────────────────────── */
.detail-aside {
  position: sticky;
  top: calc(64px + var(--space-xl));
}

.event-date { font-size: .72rem; margin-bottom: 12px; }

.event-name {
  font-family: var(--f-display);
  font-size: clamp(1.6rem, 2.5vw, 2.4rem);
  font-weight: 600;
  line-height: 1.15;
  color: var(--c-text);
  margin-bottom: 10px;
}

.event-venue {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: .9rem;
  color: var(--c-text-3);
}
.venue-icon { color: var(--c-gold); }

.aside-label {
  font-size: .7rem;
  font-weight: 700;
  letter-spacing: .12em;
  text-transform: uppercase;
  color: var(--c-text-3);
  margin-bottom: var(--space-sm);
}

.sectors-list {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.sector-skeleton {
  height: 52px;
  border-radius: var(--r-sm);
}

.sector-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--space-sm);
  padding: 10px 12px;
  border: 1px solid var(--c-border);
  border-radius: var(--r-sm);
  transition: border-color var(--t-fast), background var(--t-fast);
}
.sector-item:hover {
  border-color: var(--c-gold-dim);
  background: rgba(201,168,76,.04);
}
.sector-item:hover .sector-item__arrow { transform: translateX(3px); color: var(--c-gold); }

.sector-item__info {
  display: flex;
  flex-direction: column;
  gap: 2px;
  min-width: 0;
}
.sector-item__name {
  font-size: .82rem;
  font-weight: 600;
  color: var(--c-text);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
.sector-item__cap {
  font-size: .7rem;
  color: var(--c-text-3);
  letter-spacing: .03em;
}

.sector-item__right {
  display: flex;
  align-items: center;
  gap: 10px;
  flex-shrink: 0;
}
.sector-item__price {
  font-family: var(--f-mono);
  font-size: .82rem;
  color: var(--c-gold);
}
.sector-item__arrow {
  font-size: .8rem;
  color: var(--c-text-3);
  transition: transform var(--t-fast), color var(--t-fast);
}

/* ── Right column ────────────────────────────────────────────────────────────── */
.venue-skeleton {
  width: 100%;
  aspect-ratio: 500 / 428;
  border-radius: 22px;
}

.overflow-sectors { margin-top: var(--space-lg); }
.overflow-label {
  font-size: .7rem;
  font-weight: 700;
  letter-spacing: .1em;
  text-transform: uppercase;
  color: var(--c-text-3);
  margin-bottom: var(--space-sm);
}
.overflow-chips { display: flex; flex-wrap: wrap; gap: 8px; }
.sector-chip {
  display: inline-flex;
  align-items: center;
  padding: 6px 14px;
  background: var(--c-surface-2);
  border: 1px solid var(--c-border);
  border-radius: 99px;
  font-size: .78rem;
  color: var(--c-text-2);
  transition: border-color var(--t-fast), color var(--t-fast);
}
.sector-chip:hover { border-color: var(--c-gold-dim); color: var(--c-gold); }

/* ── Fallback sector cards ───────────────────────────────────────────────────── */
.sectors-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(180px, 1fr));
  gap: var(--space-md);
}

.sector-card {
  padding: var(--space-lg);
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
  cursor: pointer;
  transition: border-color var(--t-base), transform var(--t-base), box-shadow var(--t-base);
}
.sector-card:hover {
  border-color: var(--c-gold-dim);
  box-shadow: var(--shadow-gold);
  transform: translateY(-2px);
}
.sector-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 8px;
}
.sector-name {
  font-family: var(--f-display);
  font-size: 1.2rem;
  font-weight: 500;
  color: var(--c-text);
}
.sector-price {
  font-family: var(--f-mono);
  font-size: .82rem;
  color: var(--c-gold);
  white-space: nowrap;
}
.sector-meta {
  display: flex;
  align-items: center;
  justify-content: space-between;
  font-size: .75rem;
  color: var(--c-text-3);
}
.capacity-wrap   { display: flex; gap: 6px; }
.capacity-label  { text-transform: uppercase; letter-spacing: .06em; }
.capacity-val    { font-family: var(--f-mono); color: var(--c-text-2); }
.arrow {
  color: var(--c-gold);
  transition: transform var(--t-fast);
}
.sector-card:hover .arrow { transform: translateX(4px); }

.empty-state {
  padding: var(--space-xl);
  text-align: center;
  color: var(--c-text-3);
  border: 1px dashed var(--c-border);
  border-radius: var(--r-md);
}

/* ── Mobile ──────────────────────────────────────────────────────────────────── */
@media (max-width: 768px) {
  .detail-layout {
    grid-template-columns: 1fr;
    gap: var(--space-lg);
  }
  .detail-aside { position: static; }
}
</style>
