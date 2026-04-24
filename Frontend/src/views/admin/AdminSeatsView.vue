<template>
  <div class="admin-seats">
    <div class="view-header">
      <div>
        <h1 class="page-title">Asientos</h1>
        <p class="page-desc" v-if="sectorName">Sector: <strong>{{ sectorName }}</strong></p>
      </div>
      <div class="header-btns">
        <button class="btn btn-outline" @click="openBulk">⊞ Carga masiva</button>
        <button class="btn btn-primary" @click="openCreate">+ Nuevo asiento</button>
      </div>
    </div>

    <div v-if="error" class="alert alert-error">
      <span>⚠</span> {{ error }}
    </div>

    <!-- Status summary -->
    <div class="status-bar" v-if="seats.length">
      <div class="status-item">
        <span class="dot dot-available"></span>
        Disponibles: <strong>{{ statusCount('Available') }}</strong>
      </div>
      <div class="status-item">
        <span class="dot dot-reserved"></span>
        Reservados: <strong>{{ statusCount('Reserved') }}</strong>
      </div>
      <div class="status-item">
        <span class="dot dot-sold"></span>
        Vendidos: <strong>{{ statusCount('Confirmed') }}</strong>
      </div>
      <div class="status-item">
        Total: <strong>{{ seats.length }}</strong>
      </div>
    </div>

    <div class="table-wrap card">
      <div class="table-toolbar">
        <input v-model.trim="search" class="form-input search-input" type="search" placeholder="Buscar asiento..." />
        <select v-model="filterStatus" class="form-input" style="max-width:160px;font-size:.85rem">
          <option value="">Todos los estados</option>
          <option value="Available">Disponible</option>
          <option value="Reserved">Reservado</option>
          <option value="Confirmed">Vendido</option>
        </select>
      </div>

      <div v-if="loading" class="table-loading">
        <LoadingSpinner label="Cargando asientos..." />
      </div>
      <table v-else class="data-table">
        <thead>
          <tr>
            <th>ID</th>
            <th>Número</th>
            <th>Precio</th>
            <th>Estado</th>
            <th>Acciones</th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="!filtered.length">
            <td colspan="5" class="empty-row">No hay asientos con ese criterio.</td>
          </tr>
          <tr v-for="seat in filtered" :key="seat.id" class="data-row">
            <td class="mono">{{ seat.id }}</td>
            <td class="fw mono">{{ seat.seatNumber }}</td>
            <td class="mono">${{ seat.price?.toLocaleString('es-AR') }}</td>
            <td>
              <span class="badge" :class="statusBadge(seat.status)">{{ statusLabel(seat.status) }}</span>
            </td>
            <td>
              <div class="row-actions">
                <button class="icon-btn" @click="openEdit(seat)">✎</button>
                <button class="icon-btn icon-btn--danger" @click="openDelete(seat)">✕</button>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Create / Edit modal -->
    <BaseModal v-model="showForm" :title="editTarget ? 'Editar asiento' : 'Nuevo asiento'" width="420px">
      <form class="modal-form" @submit.prevent="handleSave">
        <div class="form-group">
          <label class="form-label">Número de asiento *</label>
          <input v-model.trim="form.seatNumber" class="form-input" type="text" placeholder="Ej: VIP-1" required />
        </div>
        <div v-if="editTarget" class="form-group">
          <label class="form-label">Estado</label>
          <select v-model="form.status" class="form-input">
            <option value="Available">Disponible</option>
            <option value="Reserved">Reservado</option>
            <option value="Confirmed">Vendido</option>
          </select>
        </div>
        <div v-if="formError" class="alert alert-error"><span>⚠</span> {{ formError }}</div>
      </form>
      <template #footer>
        <button class="btn btn-ghost" @click="showForm = false">Cancelar</button>
        <button class="btn btn-primary" :disabled="saving" @click="handleSave">
          <LoadingSpinner v-if="saving" size="16px" />
          <span v-else>{{ editTarget ? 'Guardar' : 'Crear' }}</span>
        </button>
      </template>
    </BaseModal>

    <!-- Bulk create modal -->
    <BaseModal v-model="showBulk" title="Carga masiva de asientos" width="480px">
      <p class="bulk-desc">Generá automáticamente asientos en secuencia.</p>
      <div class="modal-form">
        <div class="form-row-2">
          <div class="form-group">
            <label class="form-label">Prefijo</label>
            <input v-model.trim="bulk.prefix" class="form-input" type="text" placeholder="VIP" />
          </div>
          <div class="form-group">
            <label class="form-label">Cantidad *</label>
            <input v-model.number="bulk.count" class="form-input" type="number" min="1" max="500" required />
          </div>
        </div>
        <div class="form-group">
          <label class="form-label">Número inicial</label>
          <input v-model.number="bulk.start" class="form-input" type="number" min="1" />
        </div>
        <div class="bulk-preview" v-if="bulk.count > 0">
          Vista previa: <span class="mono">{{ bulkPreview }}</span>
        </div>
        <div v-if="bulkError" class="alert alert-error"><span>⚠</span> {{ bulkError }}</div>
      </div>
      <template #footer>
        <button class="btn btn-ghost" @click="showBulk = false">Cancelar</button>
        <button class="btn btn-primary" :disabled="bulkSaving" @click="handleBulkCreate">
          <LoadingSpinner v-if="bulkSaving" size="16px" />
          <span v-else>Crear {{ bulk.count }} asientos</span>
        </button>
      </template>
    </BaseModal>

    <ConfirmDialog
      v-model="showDelete"
      title="Eliminar asiento"
      :message="`¿Eliminar el asiento &quot;${deleteTarget?.seatNumber}&quot;?`"
      :loading="deleting"
      @confirm="handleDelete"
    />
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { adminSeatsApi, adminSectorsApi } from '@/api/admin'
import BaseModal      from '@/components/BaseModal.vue'
import ConfirmDialog  from '@/components/ConfirmDialog.vue'
import LoadingSpinner from '@/components/LoadingSpinner.vue'

const props = defineProps({
  eventId:  { type: String, required: true },
  sectorId: { type: String, required: true },
})

const seats      = ref([])
const sectorName = ref('')
const loading    = ref(false)
const error      = ref(null)
const search     = ref('')
const filterStatus = ref('')

const showForm   = ref(false)
const editTarget = ref(null)
const form       = ref({ seatNumber: '', status: 'Available' })
const formError  = ref(null)
const saving     = ref(false)

const showBulk  = ref(false)
const bulk      = ref({ prefix: '', count: 10, start: 1 })
const bulkError = ref(null)
const bulkSaving = ref(false)

const showDelete   = ref(false)
const deleteTarget = ref(null)
const deleting     = ref(false)

const filtered = computed(() => {
  return seats.value.filter(s => {
    const matchQ = !search.value || s.seatNumber?.toLowerCase().includes(search.value.toLowerCase())
    const matchS = !filterStatus.value || s.status === filterStatus.value
    return matchQ && matchS
  })
})

const bulkPreview = computed(() => {
  const pre = bulk.value.prefix ? `${bulk.value.prefix}-` : ''
  const end = bulk.value.start + bulk.value.count - 1
  return `${pre}${bulk.value.start} → ${pre}${end}`
})

function statusCount(s) { return seats.value.filter(x => x.status === s).length }
function statusLabel(s) { return { Available:'Disponible', Reserved:'Reservado', Confirmed:'Vendido' }[s] ?? s }
function statusBadge(s) { return { Available:'badge-green', Reserved:'badge-amber', Confirmed:'badge-red' }[s] ?? '' }

async function load() {
  loading.value = true; error.value = null
  try {
    const [sec, seatList] = await Promise.all([
      adminSectorsApi.get(props.eventId, props.sectorId),
      // seatsApi re-exported via admin
      adminSeatsApi.listBySector(props.eventId, props.sectorId),
    ])
    sectorName.value = sec.name
    seats.value = Array.isArray(seatList) ? seatList : seatList?.items ?? []
  } catch (e) { error.value = e.message }
  finally { loading.value = false }
}

function openCreate() {
  editTarget.value = null
  form.value = { seatNumber: '', status: 'Available' }
  formError.value = null; showForm.value = true
}
function openEdit(s) {
  editTarget.value = s
  form.value = { seatNumber: s.seatNumber, status: s.status }
  formError.value = null; showForm.value = true
}
function openBulk() {
  bulk.value = { prefix: '', count: 10, start: 1 }
  bulkError.value = null; showBulk.value = true
}
function openDelete(s) { deleteTarget.value = s; showDelete.value = true }

async function handleSave() {
  saving.value = true; formError.value = null
  try {
    if (editTarget.value) {
      const u = await adminSeatsApi.update(editTarget.value.id, form.value)
      const i = seats.value.findIndex(s => s.id === editTarget.value.id)
      if (i !== -1) seats.value[i] = { ...seats.value[i], ...u }
    } else {
      const c = await adminSeatsApi.create({
        ...form.value,
        sectorId: props.sectorId,
      })
      seats.value.push(c)
    }
    showForm.value = false
  } catch (e) { formError.value = e.message }
  finally { saving.value = false }
}

async function handleBulkCreate() {
  bulkSaving.value = true; bulkError.value = null
  const pre = bulk.value.prefix ? `${bulk.value.prefix}-` : ''
  const promises = []
  for (let i = 0; i < bulk.value.count; i++) {
    const num = bulk.value.start + i
    promises.push(adminSeatsApi.create({ seatNumber: `${pre}${num}`, sectorId: props.sectorId, status: 'Available' }))
  }
  try {
    const results = await Promise.allSettled(promises)
    const created = results.filter(r => r.status === 'fulfilled').map(r => r.value)
    seats.value.push(...created)
    const failed = results.filter(r => r.status === 'rejected').length
    if (failed > 0) bulkError.value = `${failed} asientos no se pudieron crear.`
    else showBulk.value = false
  } catch (e) { bulkError.value = e.message }
  finally { bulkSaving.value = false }
}

async function handleDelete() {
  deleting.value = true
  try {
    await adminSeatsApi.delete(deleteTarget.value.id)
    seats.value = seats.value.filter(s => s.id !== deleteTarget.value.id)
    showDelete.value = false
  } catch (e) { error.value = e.message }
  finally { deleting.value = false }
}

onMounted(load)
</script>

<style scoped>
.admin-seats { max-width: 900px; }
.view-header { display: flex; align-items: flex-start; justify-content: space-between; gap: var(--space-lg); margin-bottom: var(--space-xl); flex-wrap: wrap; }
.header-btns { display: flex; gap: 10px; flex-wrap: wrap; }
.page-title { font-family: var(--f-display); font-size: 1.8rem; font-weight: 600; color: var(--c-text); margin-bottom: 4px; }
.page-desc  { font-size: .85rem; color: var(--c-text-3); }

.status-bar { display: flex; gap: var(--space-lg); flex-wrap: wrap; margin-bottom: var(--space-lg); padding: var(--space-md) var(--space-lg); background: var(--c-surface); border: 1px solid var(--c-border); border-radius: var(--r-md); font-size: .82rem; color: var(--c-text-2); }
.status-item { display: flex; align-items: center; gap: 8px; }
.dot { width: 10px; height: 10px; border-radius: 50%; flex-shrink: 0; }
.dot-available { background: var(--c-green); }
.dot-reserved  { background: var(--c-amber); }
.dot-sold      { background: var(--c-text-3); }

.table-wrap { overflow: hidden; }
.table-toolbar { display: flex; align-items: center; gap: var(--space-md); padding: var(--space-md) var(--space-lg); border-bottom: 1px solid var(--c-border); flex-wrap: wrap; }
.search-input { max-width: 240px; font-size: .85rem; }
.table-loading { padding: var(--space-2xl); display: flex; justify-content: center; }
.data-table { width: 100%; border-collapse: collapse; font-size: .85rem; }
.data-table th { padding: 10px 16px; text-align: left; font-size: .7rem; font-weight: 700; letter-spacing: .08em; text-transform: uppercase; color: var(--c-text-3); border-bottom: 1px solid var(--c-border); }
.data-table td { padding: 12px 16px; border-bottom: 1px solid var(--c-border); color: var(--c-text-2); vertical-align: middle; }
.data-row:last-child td { border-bottom: none; }
.data-row:hover td { background: var(--c-surface-2); }
.mono { font-family: var(--f-mono); font-size: .82rem; }
.fw   { font-weight: 600; color: var(--c-text); }
.empty-row { text-align: center; color: var(--c-text-3); padding: var(--space-xl) !important; }
.row-actions { display: flex; gap: 6px; }
.icon-btn { background: none; border: 1px solid var(--c-border); border-radius: var(--r-sm); padding: 5px 9px; cursor: pointer; color: var(--c-text-3); font-size: .85rem; transition: all var(--t-fast); }
.icon-btn:hover { border-color: var(--c-gold-dim); color: var(--c-gold); background: rgba(201,168,76,.06); }
.icon-btn--danger:hover { border-color: rgba(192,57,43,.4); color: var(--c-red); background: var(--c-red-bg); }
.modal-form { display: flex; flex-direction: column; gap: var(--space-md); }
.form-row-2 { display: grid; grid-template-columns: 1fr 1fr; gap: var(--space-md); }
.bulk-desc { font-size: .85rem; color: var(--c-text-3); margin-bottom: var(--space-md); }
.bulk-preview { font-size: .82rem; color: var(--c-text-3); background: var(--c-surface-2); padding: 10px 14px; border-radius: var(--r-sm); }
</style>
