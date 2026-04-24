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

  // ── User routes ────────────────────────────────────────────────────────────
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

  // ── Admin routes (requiresAuth + requiresAdmin) ────────────────────────────
  {
    path: '/admin',
    component: () => import('@/views/admin/AdminLayout.vue'),
    meta: { requiresAuth: true, requiresAdmin: true, hideNav: true },
    children: [
      {
        path: '',
        name: 'AdminDashboard',
        component: () => import('@/views/admin/AdminDashboard.vue'),
      },
      // Events
      {
        path: 'events',
        name: 'AdminEvents',
        component: () => import('@/views/admin/AdminEventsView.vue'),
      },
      {
        path: 'events/:eventId/sectors',
        name: 'AdminSectors',
        component: () => import('@/views/admin/AdminSectorsView.vue'),
        props: true,
      },
      {
        path: 'events/:eventId/sectors/:sectorId/seats',
        name: 'AdminSeats',
        component: () => import('@/views/admin/AdminSeatsView.vue'),
        props: true,
      },
      // Users
      {
        path: 'users',
        name: 'AdminUsers',
        component: () => import('@/views/admin/AdminUsersView.vue'),
      },
      // Reservations
      {
        path: 'reservations',
        name: 'AdminReservations',
        component: () => import('@/views/admin/AdminReservationsView.vue'),
      },
      // Audit
      {
        path: 'audit',
        name: 'AdminAudit',
        component: () => import('@/views/admin/AdminAuditView.vue'),
      },
    ],
  },

  // ── Fallback ────────────────────────────────────────────────────────────────
  { path: '/:pathMatch(.*)*', redirect: '/' },
]

const router = createRouter({
  history: createWebHistory(),
  routes,
  scrollBehavior: () => ({ top: 0 }),
})

router.beforeEach((to) => {
  const auth = useAuthStore()

  if (to.meta.requiresAuth && !auth.isAuthenticated) {
    return { name: 'Login', query: { redirect: to.fullPath } }
  }

  if (to.meta.requiresAdmin && !auth.isAdmin) {
    return { name: 'Events' }  // silently redirect non-admins
  }

  if (to.meta.public && auth.isAuthenticated) {
    return { name: 'Events' }
  }
})

export default router
