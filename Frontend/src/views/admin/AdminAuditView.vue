<template>
  <div class="admin-audit">
    <div class="view-header">
      <div>
        <h1 class="page-title">Auditoría</h1>
        <p class="page-desc">Registro completo de acciones del sistema.</p>
      </div>
      <button class="btn btn-ghost" @click="load">↻ Actualizar</button>
    </div>

    <!-- Filters -->
    <div class="filters-card card">
      <div class="filters-grid">
        <div class="form-group">
          <label class="form-label">Usuario ID</label>
          <input v-model.trim="filters.userId" class="form-input" type="text" placeholder="Filtrar por usuario..." />
        </div>
        <div class="form-group">
          <label class="form-label">Acción</label>

          <select v-model="filters.action" class="form-input">
            <option value="">Todas</option>
            <option value="Reserved">Reserved</option>
            <option value="PaymentConfirmed">PaymentConfirmed</option>
            <option value="Released">Released</option>
            <option value="ConflictAttempt">ConflictAttempt</option>
            <option value="ExpiredLock">ExpiredLock</option>
            <option value="Created">Created</option>
            <option value="Deleted">Deleted</option>
            <option value="Updated">Updated</option>
            <option value="Login">Login</option>
          </select>
        </div>
        <div class="form-group">
          <label class="form-label">Desde</label>
          <input v-model="filters.from" class="form-input" type="datetime-local" />
        </div>
        <div class="form-group">
          <label class="form-label">Hasta</label>
          <input v-model="filters.to" class="form-input" type="datetime-local" />
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
        <span class="total-badge">{{ logs.length }} registros</span>
        <div class="pagination-controls">
          <button class="btn btn-ghost btn-sm" :disabled="filters.page <= 1" @click="prevPage">← Anterior</button>
          <span class="page-indicator">Pág. {{ filters.page }}</span>
          <button class="btn btn-ghost btn-sm" :disabled="logs.length < filters.take" @click="nextPage">Siguiente →</button>
        </div>
      </div>

      <div v-if="loading" class="table-loading">
        <LoadingSpinner label="Cargando registros..." />
      </div>

      <table v-else class="data-table">
        <thead>
          <tr>
            <th>Fecha / Hora</th>
            <th>Usuario</th>
            <th>Acción</th>
            <th>Tipo recurso</th>
            <th>ID recurso</th>
            <th>Detalles</th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="!logs.length">
            <td colspan="6" class="empty-row">No hay registros con los filtros aplicados.</td>
          </tr>
          <tr v-for="log in logs" :key="log.id" class="data-row">
            <td class="mono date-cell">{{ formatDate(log.occurredAt) }}</td>
            <td class="mono">{{ log.userId }}</td>
            <td>
              <span class="badge action-badge" :class="actionBadge(log.action)">{{ log.action }}</span>
            </td>
            <td class="mono resource-type">{{ log.resourceType }}</td>
            <td class="mono">{{ log.resourceId }}</td>
            <td class="details-cell" :title="log.details">{{ log.details }}</td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { adminAuditApi } from '@/api/admin'
import LoadingSpinner from '@/components/LoadingSpinner.vue'

const logs    = ref([])
const loading = ref(false)
const error   = ref(null)

const filters = ref({
  userId: '', action: '', from: '', to: '',
  page: 1, take: 50,
})

function formatDate(d) {
  if (!d) return '—'
  return new Date(d).toLocaleString('es-AR', {
    day: '2-digit', month: 'short', year: 'numeric',
    hour: '2-digit', minute: '2-digit', second: '2-digit',
  })
}

function actionBadge(action) {
  if (!action) return ''
  const a = action.toLowerCase()
  if (a.includes('login') || a.includes('register')) return 'badge-green'
  if (a.includes('delete') || a.includes('cancel'))   return 'badge-red'
  if (a.includes('pay') || a.includes('confirm'))     return 'badge-gold'
  return 'badge-amber'
}

function resetFilters() {
  filters.value = { userId: '', action: '', from: '', to: '', page: 1, take: 50 }
  load()
}

function prevPage() { if (filters.value.page > 1) { filters.value.page--; load() } }
function nextPage() { filters.value.page++; load() }

async function load() {
  loading.value = true; error.value = null
  const params = {}
  if (filters.value.userId) params.UserId = filters.value.userId
  if (filters.value.action) params.Action = filters.value.action
  if (filters.value.from)   params.From   = filters.value.from
  if (filters.value.to)     params.To     = filters.value.to
  params.Page = filters.value.page
  params.Take = filters.value.take

  try {
    const res = await adminAuditApi.list(params)
    logs.value = Array.isArray(res) ? res : res?.items ?? []
  } catch (e) {
    error.value = e.message
  } finally {
    loading.value = false
  }
}

onMounted(load)
</script>

<style scoped>
.admin-audit { max-width: 1100px; }
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
.date-cell { white-space: nowrap; color: var(--c-text-3); }
.resource-type { color: var(--c-text-2); }
.details-cell { max-width: 220px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; font-size: .8rem; color: var(--c-text-3); }
.empty-row { text-align: center; color: var(--c-text-3); padding: var(--space-xl) !important; }
.action-badge { font-size: .65rem; }
</style>
