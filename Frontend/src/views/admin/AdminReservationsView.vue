<template>
  <div class="admin-reservations">
    <div class="view-header">
      <div>
        <h1 class="page-title">Reservas</h1>
        <p class="page-desc">Gestión y consulta de reservas del sistema.</p>
      </div>
      <button class="btn btn-ghost" @click="load">↻ Actualizar</button>
    </div>

    <!-- Filters -->
    <div class="filters-card card">
      <div class="filters-grid">
        <div class="form-group">
          <label class="form-label">Asiento ID</label>
          <input v-model.number="filters.SeatId" class="form-input" type="number" placeholder="Filtrar por asiento..." />
        </div>
        <div class="form-group">
          <label class="form-label">Usuario ID</label>
          <input v-model.trim="filters.UserId" class="form-input" type="text" placeholder="Filtrar por usuario..." />
        </div>
        <div class="form-group">
          <label class="form-label">Estado</label>
          <select v-model="filters.IsActive" class="form-input">
            <option :value="null">Todos</option>
            <option :value="true">Activas</option>
            <option :value="false">Expiradas</option>
          </select>
        </div>
      </div>
      <div class="filters-actions">
        <button class="btn btn-ghost btn-sm" @click="resetFilters">Limpiar</button>
        <button class="btn btn-primary btn-sm" :disabled="loading" @click="load">Aplicar filtros</button>
      </div>
    </div>

    <!-- Error -->
    <div v-if="error" class="alert alert-error">
      <span>⚠</span> {{ error }}
    </div>

    <!-- Table -->
    <div class="table-wrap card">
      <div class="table-toolbar">
        <span class="total-badge">{{ reservations.length }} reservas</span>
        <div class="pagination-controls">
          <button class="btn btn-ghost btn-sm" :disabled="filters.Page <= 1" @click="prevPage">← Anterior</button>
          <span class="page-indicator">Pág. {{ filters.Page }}</span>
          <button class="btn btn-ghost btn-sm" :disabled="reservations.length < filters.Take" @click="nextPage">Siguiente →</button>
        </div>
      </div>

      <div v-if="loading" class="table-loading">
        <LoadingSpinner label="Cargando reservas..." />
      </div>

      <table v-else class="data-table">
        <thead>
          <tr>
            <th>ID Reserva</th>
            <th>Asiento</th>
            <th>Usuario</th>
            <th>Reservado</th>
            <th>Vencimiento</th>
            <th>Estado</th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="!reservations.length">
            <td colspan="6" class="empty-row">No se encontraron reservas.</td>
          </tr>
          <tr v-for="res in reservations" :key="res.id" class="data-row">
            <td class="mono id-cell">{{ res.id }}</td>
            <td class="mono">{{ res.seatId }}</td>
            <td class="mono user-cell">{{ res.userId }}</td>
            <td class="date-cell">{{ formatDate(res.reservedAt) }}</td>
            <td class="date-cell">{{ formatDate(res.expiresAt) }}</td>
            <td>
              <span class="badge" :class="res.isExpired ? 'badge-red' : 'badge-green'">
                {{ res.isExpired ? 'Expirada' : 'Activa' }}
              </span>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { adminReservationsApi } from '@/api/admin'
import LoadingSpinner from '@/components/LoadingSpinner.vue'

const reservations = ref([])
const loading      = ref(false)
const error        = ref(null)

const filters = ref({
  SeatId: null,
  UserId: '',
  IsActive: null,
  Page: 1,
  Take: 20
})

function formatDate(d) {
  if (!d) return '—'
  return new Date(d).toLocaleString('es-AR', {
    day:'2-digit', month:'short', year:'numeric', hour:'2-digit', minute:'2-digit',
  })
}

function resetFilters() {
  filters.value = { SeatId: null, UserId: '', IsActive: null, Page: 1, Take: 20 }
  load()
}

function prevPage() { if (filters.value.Page > 1) { filters.value.Page--; load() } }
function nextPage() { filters.value.Page++; load() }

async function load() {
  loading.value = true; error.value = null
  const params = { ...filters.value }
  
  // Clean empty params
  if (!params.SeatId) delete params.SeatId
  if (!params.UserId) delete params.UserId
  if (params.IsActive === null) delete params.IsActive

  try {
    const res = await adminReservationsApi.listAll(params)
    reservations.value = Array.isArray(res) ? res : res?.items ?? []
  } catch (e) {
    error.value = e.message
  } finally {
    loading.value = false
  }
}

onMounted(load)
</script>

<style scoped>
.admin-reservations { max-width: 1100px; }
.view-header { display: flex; align-items: flex-start; justify-content: space-between; gap: var(--space-lg); margin-bottom: var(--space-xl); flex-wrap: wrap; }
.page-title { font-family: var(--f-display); font-size: 1.8rem; font-weight: 600; color: var(--c-text); margin-bottom: 4px; }
.page-desc  { font-size: .85rem; color: var(--c-text-3); }

.filters-card { padding: var(--space-lg); margin-bottom: var(--space-lg); }
.filters-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(200px, 1fr)); gap: var(--space-md); margin-bottom: var(--space-md); }
.filters-actions { display: flex; justify-content: flex-end; gap: 10px; }

.table-wrap { overflow: hidden; }
.table-toolbar { display: flex; align-items: center; justify-content: space-between; padding: var(--space-md) var(--space-lg); border-bottom: 1px solid var(--c-border); gap: var(--space-md); flex-wrap: wrap; }
.total-badge { font-size: .75rem; color: var(--c-text-3); }
.pagination-controls { display: flex; align-items: center; gap: 10px; }
.page-indicator { font-size: .78rem; color: var(--c-text-3); font-family: var(--f-mono); }
.table-loading { padding: var(--space-2xl); display: flex; justify-content: center; }

.data-table { width: 100%; border-collapse: collapse; font-size: .82rem; }
.data-table th { padding: 10px 14px; text-align: left; font-size: .68rem; font-weight: 700; letter-spacing: .08em; text-transform: uppercase; color: var(--c-text-3); border-bottom: 1px solid var(--c-border); white-space: nowrap; }
.data-table td { padding: 10px 14px; border-bottom: 1px solid var(--c-border); color: var(--c-text-2); vertical-align: middle; }
.data-row:last-child td { border-bottom: none; }
.data-row:hover td { background: var(--c-surface-2); }

.mono { font-family: var(--f-mono); font-size: .78rem; }
.id-cell { color: var(--c-gold); font-size: .75rem; }
.user-cell { color: var(--c-text-3); }
.date-cell { white-space: nowrap; color: var(--c-text-3); }
.empty-row { text-align: center; color: var(--c-text-3); padding: var(--space-xl) !important; }
</style>
