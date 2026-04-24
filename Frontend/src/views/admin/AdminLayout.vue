<template>
  <div class="admin-shell">
    <!-- Sidebar -->
    <aside class="sidebar" :class="{ collapsed: sidebarCollapsed }">
      <div class="sidebar-header">
        <router-link to="/" class="sidebar-logo">
          <span class="logo-icon">◈</span>
          <span v-if="!sidebarCollapsed" class="logo-text">STAGEFRONT</span>
        </router-link>
        <button class="collapse-btn" @click="sidebarCollapsed = !sidebarCollapsed">
          {{ sidebarCollapsed ? '›' : '‹' }}
        </button>
      </div>

      <div class="sidebar-section-label" v-if="!sidebarCollapsed">Panel Admin</div>

      <nav class="sidebar-nav">
        <router-link
          v-for="item in navItems"
          :key="item.to"
          :to="item.to"
          class="nav-item"
          active-class="nav-item--active"
          :exact="item.exact"
        >
          <span class="nav-icon">{{ item.icon }}</span>
          <span v-if="!sidebarCollapsed" class="nav-label">{{ item.label }}</span>
        </router-link>
      </nav>

      <div class="sidebar-footer">
        <router-link to="/" class="nav-item">
          <span class="nav-icon">←</span>
          <span v-if="!sidebarCollapsed" class="nav-label">Volver al sitio</span>
        </router-link>
        <button class="nav-item nav-item--logout" @click="handleLogout">
          <span class="nav-icon">⏻</span>
          <span v-if="!sidebarCollapsed" class="nav-label">Salir</span>
        </button>
      </div>
    </aside>

    <!-- Main area -->
    <div class="admin-main">
      <!-- Topbar -->
      <header class="admin-topbar">
        <div class="topbar-left">
          <button class="mobile-menu-btn" @click="mobileSidebar = !mobileSidebar">☰</button>
          <nav class="breadcrumb" aria-label="breadcrumb">
            <router-link to="/admin" class="bc-item">Dashboard</router-link>
            <template v-for="crumb in breadcrumbs" :key="crumb.label">
              <span class="bc-sep">›</span>
              <router-link v-if="crumb.to" :to="crumb.to" class="bc-item">{{ crumb.label }}</router-link>
              <span v-else class="bc-item bc-item--current">{{ crumb.label }}</span>
            </template>
          </nav>
        </div>
        <div class="topbar-right">
          <span class="admin-badge">Admin</span>
          <span class="topbar-user">{{ auth.user?.name }}</span>
        </div>
      </header>

      <!-- Page content -->
      <main class="admin-content">
        <router-view v-slot="{ Component, route: r }">
          <transition name="page" mode="out-in">
            <component :is="Component" :key="r.fullPath" />
          </transition>
        </router-view>
      </main>
    </div>

    <!-- Mobile sidebar overlay -->
    <transition name="fade">
      <div v-if="mobileSidebar" class="mobile-overlay" @click="mobileSidebar = false" />
    </transition>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useReservationStore } from '@/stores/reservation'

const auth        = useAuthStore()
const reservation = useReservationStore()
const router      = useRouter()
const route       = useRoute()

const sidebarCollapsed = ref(false)
const mobileSidebar    = ref(false)

const navItems = [
  { to: '/admin',              label: 'Dashboard',    icon: '◉', exact: true },
  { to: '/admin/events',       label: 'Eventos',      icon: '◆' },
  { to: '/admin/users',        label: 'Usuarios',     icon: '◎' },
  { to: '/admin/reservations', label: 'Reservas',     icon: '◇' },
  { to: '/admin/audit',        label: 'Auditoría',    icon: '≡' },
]

const breadcrumbs = computed(() => {
  const map = {
    '/admin/events':       [{ label: 'Eventos' }],
    '/admin/users':        [{ label: 'Usuarios' }],
    '/admin/reservations': [{ label: 'Reservas' }],
    '/admin/audit':        [{ label: 'Auditoría' }],
  }
  // Handle nested routes like /admin/events/:id/sectors
  const path = route.path
  if (path.includes('/sectors') && path.includes('/seats')) {
    return [
      { label: 'Eventos', to: '/admin/events' },
      { label: 'Sectores', to: path.split('/seats')[0] },
      { label: 'Asientos' },
    ]
  }
  if (path.includes('/sectors')) {
    return [{ label: 'Eventos', to: '/admin/events' }, { label: 'Sectores' }]
  }
  return map[path] ?? []
})

function handleLogout() {
  reservation.clear()
  auth.logout()
  router.push('/login')
}
</script>

<style scoped>
.admin-shell {
  display: flex; min-height: 100vh;
  background: var(--c-bg);
}

/* ── Sidebar ─────────────────────────────────────────────────────────────── */
.sidebar {
  width: 220px; flex-shrink: 0;
  background: var(--c-surface);
  border-right: 1px solid var(--c-border);
  display: flex; flex-direction: column;
  transition: width .25s ease;
  position: sticky; top: 0; height: 100vh; overflow: hidden;
}
.sidebar.collapsed { width: 56px; }

.sidebar-header {
  display: flex; align-items: center; justify-content: space-between;
  padding: 16px 14px;
  border-bottom: 1px solid var(--c-border);
  min-height: 60px;
  flex-shrink: 0;
}
.sidebar-logo { display: flex; align-items: center; gap: 10px; }
.logo-icon  { color: var(--c-gold); font-size: 1.2rem; flex-shrink: 0; }
.logo-text  { font-size: .72rem; font-weight: 800; letter-spacing: .18em; color: var(--c-text); white-space: nowrap; }
.collapse-btn {
  background: none; border: none; cursor: pointer;
  color: var(--c-text-3); font-size: 1.1rem;
  padding: 4px; border-radius: var(--r-sm);
  transition: color var(--t-fast);
  flex-shrink: 0;
}
.collapse-btn:hover { color: var(--c-gold); }

.sidebar-section-label {
  font-size: .62rem; font-weight: 700;
  letter-spacing: .14em; text-transform: uppercase;
  color: var(--c-text-3);
  padding: 16px 16px 8px;
}

.sidebar-nav { flex: 1; padding: 8px 8px; overflow-y: auto; }
.sidebar-footer { padding: 8px 8px 16px; border-top: 1px solid var(--c-border); }

.nav-item {
  display: flex; align-items: center; gap: 10px;
  padding: 9px 10px;
  border-radius: var(--r-sm);
  font-size: .82rem; font-weight: 600;
  letter-spacing: .04em;
  color: var(--c-text-3);
  transition: all var(--t-fast);
  white-space: nowrap;
  width: 100%; text-align: left;
  background: none; border: none; cursor: pointer;
  margin-bottom: 2px;
}
.nav-item:hover { background: var(--c-surface-2); color: var(--c-text); }
.nav-item--active { background: rgba(201,168,76,.1); color: var(--c-gold); }
.nav-item--logout:hover { color: var(--c-red); }
.nav-icon { font-size: .9rem; flex-shrink: 0; width: 18px; text-align: center; }
.nav-label { overflow: hidden; }

/* ── Main ────────────────────────────────────────────────────────────────── */
.admin-main { flex: 1; display: flex; flex-direction: column; min-width: 0; }

.admin-topbar {
  display: flex; align-items: center; justify-content: space-between;
  height: 56px; padding-inline: var(--space-lg);
  background: var(--c-surface);
  border-bottom: 1px solid var(--c-border);
  position: sticky; top: 0; z-index: 10;
  gap: var(--space-md);
  flex-shrink: 0;
}
.topbar-left  { display: flex; align-items: center; gap: var(--space-md); min-width: 0; }
.topbar-right { display: flex; align-items: center; gap: 12px; flex-shrink: 0; }

.breadcrumb { display: flex; align-items: center; gap: 6px; font-size: .78rem; color: var(--c-text-3); }
.bc-item { color: var(--c-text-3); transition: color var(--t-fast); }
.bc-item:hover { color: var(--c-gold); }
.bc-item--current { color: var(--c-text-2); }
.bc-sep { color: var(--c-border-2); }

.admin-badge {
  font-size: .65rem; font-weight: 700;
  letter-spacing: .1em; text-transform: uppercase;
  background: rgba(201,168,76,.12);
  color: var(--c-gold);
  padding: 3px 10px; border-radius: 99px;
  border: 1px solid var(--c-gold-dim);
}
.topbar-user { font-size: .82rem; color: var(--c-text-2); }

.mobile-menu-btn { display: none; background: none; border: none; cursor: pointer; color: var(--c-text-2); font-size: 1.2rem; padding: 4px; }

.admin-content { flex: 1; padding: var(--space-xl); overflow-y: auto; }

.mobile-overlay { position: fixed; inset: 0; z-index: 49; background: rgba(0,0,0,.5); }

.fade-enter-active, .fade-leave-active { transition: opacity .2s; }
.fade-enter-from, .fade-leave-to { opacity: 0; }

@media (max-width: 768px) {
  .sidebar { position: fixed; left: 0; top: 0; z-index: 50; height: 100vh; transform: translateX(-100%); transition: transform .25s ease; }
  .sidebar.mobile-open { transform: translateX(0); }
  .mobile-menu-btn { display: block; }
  .admin-content { padding: var(--space-md); }
}
</style>
