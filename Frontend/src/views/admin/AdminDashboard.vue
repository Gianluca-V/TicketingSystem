<template>
  <div class="dashboard">
    <div class="page-heading">
      <h1 class="page-title">Dashboard</h1>
      <p class="page-desc">Resumen general del sistema.</p>
    </div>

    <!-- Stats cards -->
    <div class="stats-grid">
      <div v-for="stat in stats" :key="stat.label" class="stat-card card">
        <div class="stat-icon">{{ stat.icon }}</div>
        <div class="stat-body">
          <p class="stat-label">{{ stat.label }}</p>
          <p class="stat-value">
            <span v-if="stat.loading" class="skeleton" style="width:40px;height:24px;display:inline-block"></span>
            <span v-else>{{ stat.value }}</span>
          </p>
        </div>
        <router-link :to="stat.to" class="stat-link">Ver →</router-link>
      </div>
    </div>

    <!-- Quick links -->
    <div class="quick-section">
      <h2 class="section-title">Acciones rápidas</h2>
      <div class="quick-grid">
        <router-link to="/admin/events" class="quick-card card">
          <span class="quick-icon">◆</span>
          <span class="quick-label">Crear evento</span>
          <span class="quick-arrow">→</span>
        </router-link>
        <router-link to="/admin/users" class="quick-card card">
          <span class="quick-icon">◎</span>
          <span class="quick-label">Gestionar usuarios</span>
          <span class="quick-arrow">→</span>
        </router-link>
        <router-link to="/admin/reservations" class="quick-card card">
          <span class="quick-icon">◇</span>
          <span class="quick-label">Ver reservas activas</span>
          <span class="quick-arrow">→</span>
        </router-link>
        <router-link to="/admin/audit" class="quick-card card">
          <span class="quick-icon">≡</span>
          <span class="quick-label">Registros de auditoría</span>
          <span class="quick-arrow">→</span>
        </router-link>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { adminEventsApi, adminUsersApi, adminReservationsApi, adminAuditApi } from '@/api/admin'

const stats = ref([
  { label: 'Eventos',     icon: '◆', value: '—', to: '/admin/events',       loading: true },
  { label: 'Usuarios',    icon: '◎', value: '—', to: '/admin/users',        loading: true },
  { label: 'Reservas',    icon: '◇', value: '—', to: '/admin/reservations', loading: true },
  { label: 'Auditoría',   icon: '≡', value: '—', to: '/admin/audit',        loading: true },
])

onMounted(async () => {
  try {
    const [events, users, reservations, audit] = await Promise.allSettled([
      adminEventsApi.list(),
      adminUsersApi.list(),
      adminReservationsApi.listAll(),
      adminAuditApi.list(),
    ])
    
    if (events.status === 'fulfilled') {
      const list = Array.isArray(events.value) ? events.value : events.value?.items ?? []
      stats.value[0].value = list.length
    }
    if (users.status === 'fulfilled') {
      const list = Array.isArray(users.value) ? users.value : users.value?.items ?? []
      stats.value[1].value = list.length
    }
    if (reservations.status === 'fulfilled') {
      const list = Array.isArray(reservations.value) ? reservations.value : reservations.value?.items ?? []
      stats.value[2].value = list.length
    }
    if (audit.status === 'fulfilled') {
      const list = Array.isArray(audit.value) ? audit.value : audit.value?.items ?? []
      stats.value[3].value = list.length
    }
  } finally {
    stats.value.forEach(s => s.loading = false)
  }
})
</script>

<style scoped>
.dashboard { max-width: 1000px; }
.page-heading { margin-bottom: var(--space-xl); }
.page-title { font-family: var(--f-display); font-size: 2rem; font-weight: 600; color: var(--c-text); margin-bottom: 6px; }
.page-desc  { font-size: .875rem; color: var(--c-text-3); }

.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
  gap: var(--space-md);
  margin-bottom: var(--space-xl);
}
.stat-card {
  display: flex; align-items: center; gap: var(--space-md);
  padding: var(--space-lg);
  position: relative; overflow: visible;
}
.stat-icon { font-size: 1.4rem; color: var(--c-gold); opacity: .6; flex-shrink: 0; }
.stat-body { flex: 1; min-width: 0; }
.stat-label { font-size: .7rem; font-weight: 700; letter-spacing: .1em; text-transform: uppercase; color: var(--c-text-3); margin-bottom: 4px; }
.stat-value { font-family: var(--f-mono); font-size: 1.6rem; font-weight: 500; color: var(--c-text); line-height: 1; }
.stat-link { position: absolute; bottom: 12px; right: 14px; font-size: .72rem; color: var(--c-gold); opacity: 0; transition: opacity var(--t-fast); }
.stat-card:hover .stat-link { opacity: 1; }

.section-title {
  font-size: .75rem; font-weight: 700; letter-spacing: .1em; text-transform: uppercase;
  color: var(--c-text-3); margin-bottom: var(--space-md);
}
.quick-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(200px, 1fr)); gap: var(--space-md); }
.quick-card {
  display: flex; align-items: center; gap: 12px;
  padding: var(--space-md) var(--space-lg);
  cursor: pointer;
  transition: border-color var(--t-base), transform var(--t-base);
}
.quick-card:hover { border-color: var(--c-gold-dim); transform: translateY(-2px); }
.quick-icon { font-size: 1rem; color: var(--c-gold); flex-shrink: 0; }
.quick-label { flex: 1; font-size: .85rem; font-weight: 600; color: var(--c-text-2); }
.quick-arrow { color: var(--c-gold); transition: transform var(--t-fast); }
.quick-card:hover .quick-arrow { transform: translateX(3px); }
</style>
