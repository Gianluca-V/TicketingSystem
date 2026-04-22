<template>
  <header class="header">
    <div class="container header-inner">
      <!-- Logo -->
      <router-link to="/" class="logo">
        <span class="logo-icon">◈</span>
        <span class="logo-text">STAGEFRONT</span>
      </router-link>

      <!-- Desktop nav -->
      <nav class="nav" role="navigation" aria-label="Main">
        <router-link to="/" class="nav-link" exact-active-class="nav-link--active">
          Eventos
        </router-link>
        <router-link to="/profile" class="nav-link" active-class="nav-link--active">
          Mi Perfil
        </router-link>
      </nav>

      <!-- User menu -->
      <div class="header-actions">
        <div v-if="auth.isAuthenticated" class="user-area">
          <span class="user-name">{{ auth.user?.name }}</span>
          <button class="btn btn-ghost btn-sm" @click="handleLogout">
            Salir
          </button>
        </div>
      </div>

      <!-- Mobile menu toggle -->
      <button class="mobile-toggle" @click="mobileOpen = !mobileOpen" aria-label="Menú">
        <span :class="['bar', { open: mobileOpen }]"></span>
        <span :class="['bar', { open: mobileOpen }]"></span>
        <span :class="['bar', { open: mobileOpen }]"></span>
      </button>
    </div>

    <!-- Mobile nav -->
    <transition name="mobile-nav">
      <div v-if="mobileOpen" class="mobile-menu">
        <router-link to="/" class="mobile-link" @click="mobileOpen = false">Eventos</router-link>
        <router-link to="/profile" class="mobile-link" @click="mobileOpen = false">Mi Perfil</router-link>
        <button class="btn btn-ghost btn-sm" style="margin-top:8px" @click="handleLogout">
          Cerrar sesión
        </button>
      </div>
    </transition>
  </header>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useReservationStore } from '@/stores/reservation'

const auth        = useAuthStore()
const reservation = useReservationStore()
const router      = useRouter()
const mobileOpen  = ref(false)

async function handleLogout() {
  mobileOpen.value = false
  reservation.clear()
  auth.logout()
  router.push('/login')
}
</script>

<style scoped>
.header {
  position: sticky;
  top: 0;
  z-index: 100;
  background: rgba(8,8,8,.92);
  backdrop-filter: blur(12px);
  border-bottom: 1px solid var(--c-border);
}
.header-inner {
  display: flex;
  align-items: center;
  height: 64px;
  gap: var(--space-xl);
}

.logo {
  display: flex;
  align-items: center;
  gap: 10px;
  flex-shrink: 0;
}
.logo-icon {
  color: var(--c-gold);
  font-size: 1.3rem;
  line-height: 1;
}
.logo-text {
  font-family: var(--f-ui);
  font-size: .78rem;
  font-weight: 800;
  letter-spacing: .2em;
  color: var(--c-text);
}

.nav {
  display: flex;
  gap: var(--space-lg);
  flex: 1;
}
.nav-link {
  font-size: .8rem;
  font-weight: 600;
  letter-spacing: .08em;
  text-transform: uppercase;
  color: var(--c-text-3);
  transition: color var(--t-fast);
  padding-block: 4px;
  border-bottom: 2px solid transparent;
}
.nav-link:hover { color: var(--c-text-2); }
.nav-link--active {
  color: var(--c-gold) !important;
  border-bottom-color: var(--c-gold);
}

.header-actions { margin-left: auto; }
.user-area { display: flex; align-items: center; gap: 12px; }
.user-name {
  font-size: .85rem;
  color: var(--c-text-2);
  max-width: 160px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.mobile-toggle {
  display: none;
  flex-direction: column;
  justify-content: center;
  gap: 5px;
  background: none;
  border: none;
  cursor: pointer;
  padding: 4px;
  margin-left: auto;
}
.bar {
  display: block;
  width: 22px;
  height: 2px;
  background: var(--c-text-2);
  border-radius: 2px;
  transition: all var(--t-base);
}

.mobile-menu {
  display: flex;
  flex-direction: column;
  padding: var(--space-md) var(--space-lg);
  border-top: 1px solid var(--c-border);
  background: var(--c-surface);
  gap: var(--space-sm);
}
.mobile-link {
  font-size: .85rem;
  font-weight: 600;
  letter-spacing: .06em;
  text-transform: uppercase;
  color: var(--c-text-2);
  padding: 10px 0;
  border-bottom: 1px solid var(--c-border);
}

/* Transitions */
.mobile-nav-enter-active, .mobile-nav-leave-active { transition: all .2s ease; }
.mobile-nav-enter-from, .mobile-nav-leave-to { opacity: 0; transform: translateY(-8px); }

@media (max-width: 640px) {
  .nav, .header-actions { display: none; }
  .mobile-toggle { display: flex; }
}
</style>
