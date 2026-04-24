<template>
  <div class="admin-events">
    <!-- Header -->
    <div class="view-header">
      <div>
        <h1 class="page-title">Eventos</h1>
        <p class="page-desc">Creá, editá y eliminá eventos.</p>
      </div>
      <button class="btn btn-primary" @click="openCreate">+ Nuevo evento</button>
    </div>

    <!-- Error -->
    <div v-if="error" class="alert alert-error">
      <span>⚠</span> {{ error }}
      <button class="btn btn-ghost btn-sm" style="margin-left:auto" @click="load">Reintentar</button>
    </div>

    <!-- Table -->
    <div class="table-wrap card">
      <div class="table-toolbar">
        <input
          v-model.trim="search"
          class="form-input search-input"
          type="search"
          placeholder="Buscar eventos..."
        />
        <span class="total-badge">{{ filtered.length }} eventos</span>
      </div>

      <div v-if="loading" class="table-loading">
        <LoadingSpinner label="Cargando eventos..." />
      </div>

      <table v-else class="data-table">
        <thead>
          <tr>
            <th>ID</th>
            <th>Nombre</th>
            <th>Fecha</th>
            <th>Venue</th>
            <th>Sectores</th>
            <th>Acciones</th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="!filtered.length">
            <td colspan="6" class="empty-row">No hay eventos.</td>
          </tr>
          <tr v-for="ev in filtered" :key="ev.id" class="data-row">
            <td class="mono">{{ ev.id }}</td>
            <td class="fw">{{ ev.name }}</td>
            <td>{{ formatDate(ev.date) }}</td>
            <td>{{ ev.venue }}</td>
            <td>
              <router-link :to="`/admin/events/${ev.id}/sectors`" class="link-btn">
                {{ ev.sectorCount }} →
              </router-link>
            </td>
            <td>
              <div class="row-actions">
                <button class="icon-btn" title="Editar" @click="openEdit(ev)">✎</button>
                <button class="icon-btn icon-btn--danger" title="Eliminar" @click="openDelete(ev)">✕</button>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Create / Edit modal -->
    <BaseModal v-model="showForm" :title="editTarget ? 'Editar evento' : 'Nuevo evento'" width="560px">
      <form class="modal-form" @submit.prevent="handleSave">
        <div class="form-group">
          <label class="form-label">Nombre *</label>
          <input v-model.trim="form.name" class="form-input" type="text" required />
        </div>
        <div class="form-group">
          <label class="form-label">Fecha *</label>
          <input v-model="form.date" class="form-input" type="datetime-local" required />
        </div>
        <div class="form-group">
          <label class="form-label">Venue *</label>
          <input v-model.trim="form.venue" class="form-input" type="text" required />
        </div>
        <div v-if="formError" class="alert alert-error" style="margin-top:var(--space-sm)">
          <span>⚠</span> {{ formError }}
        </div>
      </form>
      <template #footer>
        <button class="btn btn-ghost" @click="showForm = false">Cancelar</button>
        <button class="btn btn-primary" :disabled="saving" @click="handleSave">
          <LoadingSpinner v-if="saving" size="16px" />
          <span v-else>{{ editTarget ? 'Guardar cambios' : 'Crear evento' }}</span>
        </button>
      </template>
    </BaseModal>

    <!-- Delete confirm -->
    <ConfirmDialog
      v-model="showDelete"
      title="Eliminar evento"
      :message="`¿Eliminar el evento &quot;${deleteTarget?.name}&quot;? Se eliminarán también sus sectores y asientos.`"
      :loading="deleting"
      @confirm="handleDelete"
    />
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { adminEventsApi } from '@/api/admin'
import BaseModal     from '@/components/BaseModal.vue'
import ConfirmDialog from '@/components/ConfirmDialog.vue'
import LoadingSpinner from '@/components/LoadingSpinner.vue'

const events  = ref([])
const loading = ref(false)
const error   = ref(null)
const search  = ref('')

// Form state
const showForm   = ref(false)
const editTarget = ref(null)
const form       = ref({ name: '', date: '', venue: '' })
const formError  = ref(null)
const saving     = ref(false)

// Delete state
const showDelete   = ref(false)
const deleteTarget = ref(null)
const deleting     = ref(false)

const filtered = computed(() => {
  const q = search.value.toLowerCase()
  if (!q) return events.value
  return events.value.filter(e =>
    e.name?.toLowerCase().includes(q) || e.venue?.toLowerCase().includes(q),
  )
})

function formatDate(d) {
  if (!d) return '—'
  return new Date(d).toLocaleDateString('es-AR', { day:'2-digit', month:'short', year:'numeric' })
}

async function load() {
  loading.value = true; error.value = null
  try {
    const res = await adminEventsApi.list()
    events.value = Array.isArray(res) ? res : res?.items ?? []
  } catch (e) { error.value = e.message }
  finally     { loading.value = false }
}

function openCreate() {
  editTarget.value = null
  form.value = { name: '', date: '', venue: '' }
  formError.value = null
  showForm.value  = true
}
function openEdit(ev) {
  editTarget.value = ev
  // Convert ISO date to datetime-local format
  const d = ev.date ? new Date(ev.date).toISOString().slice(0, 16) : ''
  form.value = { name: ev.name, date: d, venue: ev.venue }
  formError.value = null
  showForm.value  = true
}
function openDelete(ev) {
  deleteTarget.value = ev
  showDelete.value   = true
}

async function handleSave() {
  saving.value = true; formError.value = null
  try {
    if (editTarget.value) {
      const updated = await adminEventsApi.update(editTarget.value.id, form.value)
      const idx = events.value.findIndex(e => e.id === editTarget.value.id)
      if (idx !== -1) events.value[idx] = { ...events.value[idx], ...updated }
    } else {
      const created = await adminEventsApi.create(form.value)
      events.value.unshift(created)
    }
    showForm.value = false
  } catch (e) { formError.value = e.message }
  finally     { saving.value = false }
}

async function handleDelete() {
  if (!deleteTarget.value) return
  deleting.value = true
  try {
    await adminEventsApi.delete(deleteTarget.value.id)
    events.value = events.value.filter(e => e.id !== deleteTarget.value.id)
    showDelete.value = false
  } catch (e) { error.value = e.message }
  finally     { deleting.value = false }
}

onMounted(load)
</script>

<style scoped>
.admin-events { max-width: 1000px; }
.view-header { display: flex; align-items: flex-start; justify-content: space-between; gap: var(--space-lg); margin-bottom: var(--space-xl); flex-wrap: wrap; }
.page-title { font-family: var(--f-display); font-size: 1.8rem; font-weight: 600; color: var(--c-text); margin-bottom: 4px; }
.page-desc  { font-size: .85rem; color: var(--c-text-3); }

.table-wrap { overflow: hidden; }
.table-toolbar { display: flex; align-items: center; justify-content: space-between; padding: var(--space-md) var(--space-lg); border-bottom: 1px solid var(--c-border); gap: var(--space-md); flex-wrap: wrap; }
.search-input { max-width: 300px; font-size: .85rem; }
.total-badge { font-size: .75rem; color: var(--c-text-3); white-space: nowrap; }
.table-loading { padding: var(--space-2xl); display: flex; justify-content: center; }

.data-table { width: 100%; border-collapse: collapse; font-size: .85rem; }
.data-table th { padding: 10px 16px; text-align: left; font-size: .7rem; font-weight: 700; letter-spacing: .08em; text-transform: uppercase; color: var(--c-text-3); border-bottom: 1px solid var(--c-border); }
.data-table td { padding: 12px 16px; border-bottom: 1px solid var(--c-border); color: var(--c-text-2); vertical-align: middle; }
.data-row:last-child td { border-bottom: none; }
.data-row:hover td { background: var(--c-surface-2); }
.mono { font-family: var(--f-mono); font-size: .8rem; color: var(--c-text-3); }
.fw   { font-weight: 600; color: var(--c-text); }
.empty-row { text-align: center; color: var(--c-text-3); padding: var(--space-xl) !important; }

.link-btn { color: var(--c-gold); font-weight: 600; font-size: .82rem; transition: opacity var(--t-fast); }
.link-btn:hover { opacity: .7; }

.row-actions { display: flex; gap: 6px; }
.icon-btn { background: none; border: 1px solid var(--c-border); border-radius: var(--r-sm); padding: 5px 9px; cursor: pointer; color: var(--c-text-3); font-size: .85rem; transition: all var(--t-fast); }
.icon-btn:hover { border-color: var(--c-gold-dim); color: var(--c-gold); background: rgba(201,168,76,.06); }
.icon-btn--danger:hover { border-color: rgba(192,57,43,.4); color: var(--c-red); background: var(--c-red-bg); }

.modal-form { display: flex; flex-direction: column; gap: var(--space-md); }
</style>
