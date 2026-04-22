<template>
  <div id="app">
    <AppHeader v-if="!route.meta.hideNav" />

    <main class="app-main">
      <router-view v-slot="{ Component, route: r }">
        <transition name="page" mode="out-in">
          <component :is="Component" :key="r.fullPath" />
        </transition>
      </router-view>
    </main>

    <!-- Global reservation countdown banner -->
    <ReservationBanner v-if="reservation.hasActiveReservation && !isPaymentRoute" />
  </div>
</template>

<script setup>
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import AppHeader       from '@/components/AppHeader.vue'
import ReservationBanner from '@/components/ReservationBanner.vue'
import { useReservationStore } from '@/stores/reservation'

const route       = useRoute()
const reservation = useReservationStore()

const isPaymentRoute = computed(() =>
  ['Payment', 'Reservation', 'Confirmation'].includes(route.name),
)
</script>

<style>
#app    { display: flex; flex-direction: column; min-height: 100vh; }
.app-main { flex: 1; }
</style>
