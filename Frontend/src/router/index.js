import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const routes = [
  // ── Public ─────────────────────────────────────────────────────────────────
  {
    path: '/login',
    name: 'Login',
    component: () => import('@/views/LoginView.vue'),
    meta: { public: true, hideNav: true },
  },
  {
    path: '/register',
    name: 'Register',
    component: () => import('@/views/RegisterView.vue'),
    meta: { public: true, hideNav: true },
  },

  // ── Protected ──────────────────────────────────────────────────────────────
  {
    path: '/',
    name: 'Events',
    component: () => import('@/views/EventsView.vue'),
    meta: { requiresAuth: true },
  },
  {
    path: '/events/:eventId',
    name: 'EventDetail',
    component: () => import('@/views/EventDetailView.vue'),
    meta: { requiresAuth: true },
    props: true,
  },
  {
    path: '/events/:eventId/sectors/:sectorId/seats',
    name: 'Seats',
    component: () => import('@/views/SeatsView.vue'),
    meta: { requiresAuth: true },
    props: true,
  },
  {
    path: '/reservation',
    name: 'Reservation',
    component: () => import('@/views/ReservationView.vue'),
    meta: { requiresAuth: true },
  },
  {
    path: '/payment',
    name: 'Payment',
    component: () => import('@/views/PaymentView.vue'),
    meta: { requiresAuth: true },
  },
  {
    path: '/confirmation',
    name: 'Confirmation',
    component: () => import('@/views/ConfirmationView.vue'),
    meta: { requiresAuth: true },
  },
  {
    path: '/profile',
    name: 'Profile',
    component: () => import('@/views/ProfileView.vue'),
    meta: { requiresAuth: true },
  },

  // ── Fallback ────────────────────────────────────────────────────────────────
  { path: '/:pathMatch(.*)*', redirect: '/' },
]

const router = createRouter({
  history: createWebHistory(),
  routes,
  scrollBehavior: () => ({ top: 0 }),
})

// ── Global navigation guard ──────────────────────────────────────────────────
router.beforeEach((to) => {
  const auth = useAuthStore()

  if (to.meta.requiresAuth && !auth.isAuthenticated) {
    return { name: 'Login', query: { redirect: to.fullPath } }
  }

  if (to.meta.public && auth.isAuthenticated) {
    return { name: 'Events' }
  }
})

export default router
