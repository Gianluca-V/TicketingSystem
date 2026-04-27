<template>
  <div class="admin-reservations">
    <div class="view-header">
      <div>
        <h1 class="page-title">Reservas</h1>
        <p class="page-desc">Consultá el estado de una reserva por ID.</p>
      </div>
    </div>

    <!-- Lookup by reservation ID -->
    <div class="lookup-card card">
      <p class="card-section-title">Buscar reserva por ID</p>
      <div class="lookup-row">
        <input
          v-model.trim="lookupId"
          class="form-input mono-input"
          type="text"
          placeholder="GUID de la reserva..."
          @keydown.enter="lookupReservation"
        />
        <button class="btn btn-primary" :disabled="lookupLoading || !lookupId" @click="lookupReservation">
          <LoadingSpinner v-if="lookupLoading" size="16px" />
          <span v-else>Buscar</span>
        </button>
      </div>
      <div v-if="lookupError" class="alert alert-error" style="margin-top:var(--space-md)">
        <span>⚠</span> {{ lookupError }}
      </div>
      <div v-if="lookedUp" class="lookup-result">
        <div class="result-grid">
          <div class="result-item">
            <span class="rl">ID</span>
            <span class="rv mono">{{ lookedUp.id }}</span>
          </div>
          <div class="result-item">
            <span class="rl">Usuario ID</span>
            <span class="rv mono">{{ lookedUp.userId }}</span>
          </div>
          <div class="result-item">
            <span class="rl">Asiento ID</span>
            <span class="rv mono">{{ lookedUp.seatId }}</span>
          </div>
          <div class="result-item">
            <span class="rl">Reservado el</span>
            <span class="rv">{{ formatDate(lookedUp.reservedAt) }}</span>
          </div>
          <div class="result-item">
            <span class="rl">Vence el</span>
            <span class="rv">{{ formatDate(lookedUp.expiresAt) }}</span>
          </div>
          <div class="result-item">
            <span class="rl">Expirada</span>
            <span class="rv">
              <span class="badge" :class="lookedUp.isExpired ? 'badge-red' : 'badge-green'">
                {{ lookedUp.isExpired ? 'Sí' : 'No' }}
              </span>
            </span>
          </div>
        </div>
      </div>
    </div>

    <!-- Audit-based activity log (last actions) -->
    <div class="activity-section">
      <h2 class="section-title">Actividad reciente de reservas</h2>
      <div class="table-wrap card">
        <div v-if="auditLoading" class="table-loading">
          <LoadingSpinner label="Cargando registros..." />
        </div>
        <table v-else class="data-table">
          <thead>
            <tr>
              <th>Fecha</th>
              <th>Usuario</th>
              <th>Acción</th>
              <th>Recurso</th>
              <th>Detalles</th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="!auditLogs.length">
              <td colspan="5" class="empty-row">No hay registros disponibles.</td>
            </tr>
            <tr v-for="log in auditLogs" :key="log.id" class="data-row">
              <td class="mono date-cell">{{ formatDate(log.occurredAt) }}</td>
              <td class="mono">{{ log.userId }}</td>
              <td><span class="badge badge-gold action-badge">{{ log.action }}</span></td>
              <td class="mono">{{ log.resourceType }} #{{ log.resourceId }}</td>
              <td class="details-cell">{{ log.details }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { adminReservationsApi, adminAuditApi } from '@/api/admin'
import LoadingSpinner from '@/components/LoadingSpinner.vue'

const lookupId      = ref('')
const lookupLoading = ref(false)
const lookupError   = ref(null)
const lookedUp      = ref(null)

const auditLogs    = ref([])
const auditLoading = ref(false)

function formatDate(d) {
  if (!d) return '—'
  return new Date(d).toLocaleString('es-AR', {
    day:'2-digit', month:'short', year:'numeric', hour:'2-digit', minute:'2-digit',
  })
}

async function lookupReservation() {
  if (!lookupId.value) return
  lookupLoading.value = true; lookupError.value = null; lookedUp.value = null
  try {
    lookedUp.value = await adminReservationsApi.get(lookupId.value)
  } catch (e) {
    lookupError.value = e.message
  } finally {
    lookupLoading.value = false
  }
}

async function loadAudit() {
  auditLoading.value = true
  try {
    const res = await adminReservationsApi.listAll({ Take: 30 })
    auditLogs.value = Array.isArray(res) ? res : res?.items ?? []
  } catch {
    // Audit endpoint may not be available in all environments
    auditLogs.value = []
  } finally {
    auditLoading.value = false
  }
}

onMounted(loadAudit)
</script>

<style scoped>
.admin-reservations { max-width: 960px; }
.view-header { margin-bottom: var(--space-xl); }
.page-title { font-family: var(--f-display); font-size: 1.8rem; font-weight: 600; color: var(--c-text); margin-bottom: 4px; }
.page-desc  { font-size: .85rem; color: var(--c-text-3); }

.lookup-card { padding: var(--space-lg); margin-bottom: var(--space-xl); }
.card-section-title { font-size: .72rem; font-weight: 700; letter-spacing: .1em; text-transform: uppercase; color: var(--c-text-3); margin-bottom: var(--space-md); }
.lookup-row { display: flex; gap: 10px; flex-wrap: wrap; }
.mono-input { font-family: var(--f-mono); letter-spacing: .04em; max-width: 400px; }

.lookup-result { margin-top: var(--space-lg); padding-top: var(--space-lg); border-top: 1px solid var(--c-border); }
.result-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: var(--space-md); }
.result-item { display: flex; flex-direction: column; gap: 4px; }
.rl { font-size: .7rem; font-weight: 700; letter-spacing: .08em; text-transform: uppercase; color: var(--c-text-3); }
.rv { font-size: .875rem; color: var(--c-text); }
.rv.mono { font-family: var(--f-mono); font-size: .8rem; color: var(--c-gold); word-break: break-all; }

.section-title { font-size: .75rem; font-weight: 700; letter-spacing: .1em; text-transform: uppercase; color: var(--c-text-3); margin-bottom: var(--space-md); }
.activity-section { }
.table-wrap { overflow: hidden; }
.table-loading { padding: var(--space-2xl); display: flex; justify-content: center; }
.data-table { width: 100%; border-collapse: collapse; font-size: .82rem; }
.data-table th { padding: 10px 14px; text-align: left; font-size: .68rem; font-weight: 700; letter-spacing: .08em; text-transform: uppercase; color: var(--c-text-3); border-bottom: 1px solid var(--c-border); }
.data-table td { padding: 10px 14px; border-bottom: 1px solid var(--c-border); color: var(--c-text-2); vertical-align: middle; }
.data-row:last-child td { border-bottom: none; }
.data-row:hover td { background: var(--c-surface-2); }
.mono { font-family: var(--f-mono); font-size: .78rem; }
.date-cell { color: var(--c-text-3); white-space: nowrap; }
.empty-row { text-align: center; color: var(--c-text-3); padding: var(--space-xl) !important; }
.action-badge { font-size: .65rem; }
.details-cell { max-width: 200px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; font-size: .8rem; color: var(--c-text-3); }
</style>
