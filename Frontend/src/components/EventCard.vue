<template>
  <router-link :to="`/events/${event.id}`" class="event-card card">
    <!-- Decorative header -->
    <div class="card-header">
      <div class="card-glyph">◈</div>
      <div class="card-meta">
        <span class="badge badge-gold">{{ formattedDate }}</span>
      </div>
    </div>

    <div class="card-body">
      <h3 class="event-name">{{ event.name }}</h3>
      <p class="event-venue">
        <span class="venue-icon">◎</span>
        {{ event.venue }}
      </p>
    </div>

    <div class="card-footer">
      <span class="sector-count">{{ event.sectorCount }} sectores</span>
      <span class="arrow">→</span>
    </div>
  </router-link>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  event: { type: Object, required: true },
})

const formattedDate = computed(() => {
  if (!props.event.date) return '—'
  return new Date(props.event.date).toLocaleDateString('es-AR', {
    day: '2-digit', month: 'short', year: 'numeric',
  })
})
</script>

<style scoped>
.event-card {
  display: flex;
  flex-direction: column;
  cursor: pointer;
  transition: border-color var(--t-base), box-shadow var(--t-base), transform var(--t-base);
}
.event-card:hover {
  border-color: var(--c-gold-dim);
  box-shadow: var(--shadow-gold);
  transform: translateY(-3px);
}

.card-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 18px 20px 12px;
  border-bottom: 1px solid var(--c-border);
}
.card-glyph {
  font-size: 1.2rem;
  color: var(--c-gold);
  opacity: .5;
}

.card-body { padding: 16px 20px; flex: 1; }
.event-name {
  font-family: var(--f-display);
  font-size: 1.45rem;
  font-weight: 600;
  line-height: 1.25;
  color: var(--c-text);
  margin-bottom: 8px;
}
.event-venue {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: .82rem;
  color: var(--c-text-3);
  letter-spacing: .03em;
}
.venue-icon { color: var(--c-gold); font-size: .9rem; }

.card-footer {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 12px 20px;
  border-top: 1px solid var(--c-border);
}
.sector-count {
  font-size: .75rem;
  color: var(--c-text-3);
  letter-spacing: .06em;
  text-transform: uppercase;
}
.arrow {
  color: var(--c-gold);
  font-size: 1rem;
  transition: transform var(--t-fast);
}
.event-card:hover .arrow { transform: translateX(4px); }
</style>
