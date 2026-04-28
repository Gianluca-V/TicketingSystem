<template>
  <div class="events-page">
    <div class="container">

      <!-- Hero heading -->
      <header class="page-header">
        <p class="page-eyebrow">Próximos eventos</p>
        <h1 class="page-title">Elegí tu experiencia</h1>
        <p class="page-desc">Seleccioná un evento para ver la disponibilidad de asientos en tiempo real.</p>
      </header>

      <!-- Search -->
      <div class="search-bar">
        <input
          v-model.trim="search"
          class="form-input search-input"
          type="search"
          placeholder="Buscar evento o dirección..."
          aria-label="Buscar eventos"
        />
      </div>

      <!-- Loading state -->
      <div v-if="loading" class="grid-skeleton">
        <div v-for="i in 6" :key="i" class="skeleton" style="height:180px"></div>
      </div>

      <!-- Error -->
      <div v-else-if="error" class="alert alert-error" style="margin-top:var(--space-lg)">
        <span>⚠</span> {{ error }}
        <button class="btn btn-ghost btn-sm" style="margin-left:auto" @click="load">Reintentar</button>
      </div>

      <!-- Empty -->
      <div v-else-if="!filtered.length" class="empty-state">
        <p class="empty-glyph">◎</p>
        <p>{{ search ? 'Ningún evento coincide con tu búsqueda.' : 'No hay eventos disponibles por el momento.' }}</p>
      </div>

      <!-- Events grid -->
      <div v-else class="events-grid">
        <EventCard
          v-for="(event, i) in filtered"
          :key="event.id"
          :event="event"
          class="fade-in"
          :style="{ animationDelay: `${i * 0.05}s` }"
        />
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { eventsApi } from '@/api/events'
import EventCard from '@/components/EventCard.vue'

const events  = ref([])
const loading = ref(false)
const error   = ref(null)
const search  = ref('')

const filtered = computed(() => {
  const q = search.value.toLowerCase()
  if (!q) return events.value
  return events.value.filter(e =>
    e.name?.toLowerCase().includes(q) ||
    e.venue?.toLowerCase().includes(q),
  )
})

async function load() {
  loading.value = true
  error.value   = null
  try {
    events.value = await eventsApi.list()
  } catch (e) {
    error.value = e.message
  } finally {
    loading.value = false
  }
}

onMounted(load)
</script>

<style scoped>
.events-page { padding-block: var(--space-2xl); }

.page-header { margin-bottom: var(--space-xl); }
.page-eyebrow {
  font-size: .72rem;
  font-weight: 700;
  letter-spacing: .18em;
  text-transform: uppercase;
  color: var(--c-gold);
  margin-bottom: 8px;
}
.page-title {
  font-family: var(--f-display);
  font-size: clamp(2rem, 5vw, 3.5rem);
  font-weight: 600;
  line-height: 1.1;
  color: var(--c-text);
  margin-bottom: 12px;
}
.page-desc {
  font-size: .9rem;
  color: var(--c-text-3);
  max-width: 480px;
}

.search-bar { margin-bottom: var(--space-xl); max-width: 480px; }
.search-input { font-size: .9rem; }

.grid-skeleton,
.events-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: var(--space-lg);
}

.empty-state {
  text-align: center;
  padding: var(--space-2xl) 0;
  color: var(--c-text-3);
}
.empty-glyph { font-size: 3rem; color: var(--c-border-2); margin-bottom: var(--space-md); }
</style>
