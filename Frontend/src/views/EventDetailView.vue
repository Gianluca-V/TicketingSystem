<template>
  <div class="detail-page">
    <div class="container">

      <!-- Back -->
      <router-link to="/" class="back-link">← Volver a eventos</router-link>

      <!-- Loading -->
      <LoadingSpinner v-if="loading" label="Cargando evento..." style="margin-top:var(--space-2xl)" />

      <!-- Error -->
      <div v-else-if="error" class="alert alert-error" style="margin-top:var(--space-lg)">
        <span>⚠</span> {{ error }}
      </div>

      <template v-else-if="event">
        <!-- Event hero -->
        <div class="event-hero">
          <div>
            <p class="event-date badge badge-gold">{{ formattedDate }}</p>
            <h1 class="event-name">{{ event.name }}</h1>
            <p class="event-venue">
              <span class="venue-icon">◎</span>
              {{ event.venue }}
            </p>
          </div>
        </div>

        <hr class="divider" />

        <!-- Sectors -->
        <section>
          <h2 class="section-title">Seleccioná un sector</h2>
          <p class="section-desc">Cada sector tiene un precio fijo. Después podrás elegir tu asiento.</p>

          <div v-if="sectorsLoading" class="sectors-skeleton">
            <div v-for="i in 3" :key="i" class="skeleton" style="height:120px"></div>
          </div>

          <div v-else-if="!sectors.length" class="empty-state">
            <p>No hay sectores disponibles para este evento.</p>
          </div>

          <div v-else class="sectors-grid">
            <router-link
              v-for="(sector, i) in sectors"
              :key="sector.id"
              :to="`/events/${eventId}/sectors/${sector.id}/seats`"
              class="sector-card card fade-in"
              :style="{ animationDelay: `${i * 0.08}s` }"
            >
              <div class="sector-header">
                <h3 class="sector-name">{{ sector.name }}</h3>
                <span class="sector-price">${{ sector.price?.toLocaleString('es-AR') }}</span>
              </div>
              <div class="sector-meta">
                <span class="capacity-bar-wrap">
                  <span class="capacity-label">Capacidad</span>
                  <span class="capacity-val">{{ sector.capacity }}</span>
                </span>
                <span class="arrow">→</span>
              </div>
            </router-link>
          </div>
        </section>
      </template>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { eventsApi, sectorsApi } from '@/api/events'
import LoadingSpinner from '@/components/LoadingSpinner.vue'

const props = defineProps({ eventId: { type: String, required: true } })

const event          = ref(null)
const sectors        = ref([])
const loading        = ref(false)
const sectorsLoading = ref(false)
const error          = ref(null)

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
  display: inline-flex; align-items: center; gap: 6px;
  font-size: .78rem; font-weight: 700;
  letter-spacing: .08em; text-transform: uppercase;
  color: var(--c-text-3);
  transition: color var(--t-fast);
  margin-bottom: var(--space-xl);
}
.back-link:hover { color: var(--c-gold); }

.event-hero { padding-block: var(--space-lg); }
.event-date {
  margin-bottom: 12px;
  font-size: .72rem;
}
.event-name {
  font-family: var(--f-display);
  font-size: clamp(1.8rem, 4vw, 3rem);
  font-weight: 600;
  line-height: 1.15;
  color: var(--c-text);
  margin-bottom: 10px;
}
.event-venue {
  display: flex; align-items: center; gap: 8px;
  font-size: .9rem; color: var(--c-text-3);
}
.venue-icon { color: var(--c-gold); }

.section-title {
  font-family: var(--f-ui);
  font-size: 1rem;
  font-weight: 700;
  letter-spacing: .06em;
  text-transform: uppercase;
  color: var(--c-text);
  margin-bottom: 6px;
}
.section-desc {
  font-size: .85rem; color: var(--c-text-3);
  margin-bottom: var(--space-lg);
}

.sectors-skeleton,
.sectors-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(260px, 1fr));
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
  transform: translateY(-3px);
}
.sector-header {
  display: flex; align-items: flex-start;
  justify-content: space-between; gap: 12px;
}
.sector-name {
  font-family: var(--f-display);
  font-size: 1.4rem;
  font-weight: 500;
  color: var(--c-text);
}
.sector-price {
  font-family: var(--f-mono);
  font-size: .85rem;
  color: var(--c-gold);
  white-space: nowrap;
}
.sector-meta {
  display: flex; align-items: center; justify-content: space-between;
  font-size: .78rem; color: var(--c-text-3);
}
.capacity-bar-wrap { display: flex; gap: 6px; }
.capacity-label { text-transform: uppercase; letter-spacing: .06em; }
.capacity-val { color: var(--c-text-2); font-family: var(--f-mono); }
.arrow { color: var(--c-gold); transition: transform var(--t-fast); }
.sector-card:hover .arrow { transform: translateX(4px); }

.empty-state {
  padding: var(--space-xl); text-align: center;
  color: var(--c-text-3); border: 1px dashed var(--c-border); border-radius: var(--r-md);
}
</style>
