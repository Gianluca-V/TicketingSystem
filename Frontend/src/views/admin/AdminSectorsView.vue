<template>
  <div class="admin-sectors">
    <div class="view-header">
      <div>
        <h1 class="page-title">Sectores</h1>
        <p class="page-desc" v-if="eventName">Evento: <strong>{{ eventName }}</strong></p>
      </div>
      <button class="btn btn-primary" @click="openCreate">+ Nuevo sector</button>
    </div>

    <div v-if="error" class="alert alert-error">
      <span>⚠</span> {{ error }}
    </div>

    <div class="table-wrap card">
      <div v-if="loading" class="table-loading">
        <LoadingSpinner label="Cargando sectores..." />
      </div>
      <table v-else class="data-table">
        <thead>
          <tr>
            <th>ID</th>
            <th>Nombre</th>
            <th>Precio</th>
            <th>Capacidad</th>
            <th>Asientos</th>
            <th>Acciones</th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="!sectors.length">
            <td colspan="6" class="empty-row">No hay sectores. Creá el primero.</td>
          </tr>
          <tr v-for="s in sectors" :key="s.id" class="data-row">
            <td class="mono">{{ s.id }}</td>
            <td class="fw">{{ s.name }}</td>
            <td class="mono">${{ s.price?.toLocaleString('es-AR') }}</td>
            <td class="mono">{{ s.capacity }}</td>
            <td>
              <router-link :to="`/admin/events/${eventId}/sectors/${s.id}/seats`" class="link-btn">
                Ver asientos →
              </router-link>
            </td>
            <td>
              <div class="row-actions">
                <button class="icon-btn" @click="openEdit(s)">✎</button>
                <button class="icon-btn icon-btn--danger" @click="openDelete(s)">✕</button>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Create / Edit modal -->
    <BaseModal v-model="showForm" :title="editTarget ? 'Editar sector' : 'Nuevo sector'" width="480px">
      <form class="modal-form" @submit.prevent="handleSave">
        <div class="form-group">
          <label class="form-label">Nombre *</label>
          <input v-model.trim="form.name" class="form-input" type="text" placeholder="Ej: VIP, Platea Alta" required />
        </div>
        <div class="form-row-2">
          <div class="form-group">
            <label class="form-label">Precio *</label>
            <input v-model.number="form.price" class="form-input" type="number" min="0" step="0.01" required />
          </div>
          <div class="form-group">
            <label class="form-label">Capacidad *</label>
            <input v-model.number="form.capacity" class="form-input" type="number" min="1" required />
          </div>
        </div>
        <div v-if="formError" class="alert alert-error">
          <span>⚠</span> {{ formError }}
        </div>
      </form>
      <template #footer>
        <button class="btn btn-ghost" @click="showForm = false">Cancelar</button>
        <button class="btn btn-primary" :disabled="saving" @click="handleSave">
          <LoadingSpinner v-if="saving" size="16px" />
          <span v-else>{{ editTarget ? 'Guardar' : 'Crear' }}</span>
        </button>
      </template>
    </BaseModal>

    <ConfirmDialog
      v-model="showDelete"
      title="Eliminar sector"
      :message="`¿Eliminar el sector &quot;${deleteTarget?.name}&quot;?`"
      :loading="deleting"
      @confirm="handleDelete"
    />
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { adminSectorsApi, adminEventsApi } from '@/api/admin'
import BaseModal      from '@/components/BaseModal.vue'
import ConfirmDialog  from '@/components/ConfirmDialog.vue'
import LoadingSpinner from '@/components/LoadingSpinner.vue'

const props = defineProps({ eventId: { type: String, required: true } })

const sectors   = ref([])
const eventName = ref('')
const loading   = ref(false)
const error     = ref(null)

const showForm   = ref(false)
const editTarget = ref(null)
const form       = ref({ name: '', price: 0, capacity: 100 })
const formError  = ref(null)
const saving     = ref(false)

const showDelete   = ref(false)
const deleteTarget = ref(null)
const deleting     = ref(false)

async function load() {
  loading.value = true; error.value = null
  try {
    const [ev, sec] = await Promise.all([
      adminEventsApi.get(props.eventId),
      adminSectorsApi.list(props.eventId),
    ])
    eventName.value = ev.name
    sectors.value   = Array.isArray(sec) ? sec : sec?.items ?? []
  } catch (e) { error.value = e.message }
  finally     { loading.value = false }
}

function openCreate() {
  editTarget.value = null
  form.value = { name: '', price: 0, capacity: 100 }
  formError.value = null; showForm.value = true
}
function openEdit(s) {
  editTarget.value = s
  form.value = { name: s.name, price: s.price, capacity: s.capacity }
  formError.value = null; showForm.value = true
}
function openDelete(s) { deleteTarget.value = s; showDelete.value = true }

async function handleSave() {
  saving.value = true; formError.value = null
  try {
    if (editTarget.value) {
      await adminSectorsApi.update(props.eventId, editTarget.value.id, {
        ...form.value,
        id: editTarget.value.id,
        eventId: parseInt(props.eventId)
      })
    } else {
      await adminSectorsApi.create(props.eventId, { ...form.value, eventId: props.eventId })
    }
    await load()
    showForm.value = false
  } catch (e) { formError.value = e.message }
  finally     { saving.value = false }
}

async function handleDelete() {
  deleting.value = true
  try {
    await adminSectorsApi.delete(props.eventId, deleteTarget.value.id)
    await load()
    showDelete.value = false
  } catch (e) { error.value = e.message }
  finally     { deleting.value = false }
}

onMounted(load)
</script>

<style scoped>
.admin-sectors { max-width: 900px; }
.view-header { display: flex; align-items: flex-start; justify-content: space-between; gap: var(--space-lg); margin-bottom: var(--space-xl); flex-wrap: wrap; }
.page-title { font-family: var(--f-display); font-size: 1.8rem; font-weight: 600; color: var(--c-text); margin-bottom: 4px; }
.page-desc  { font-size: .85rem; color: var(--c-text-3); }
.table-wrap { overflow: hidden; }
.table-loading { padding: var(--space-2xl); display: flex; justify-content: center; }
.data-table { width: 100%; border-collapse: collapse; font-size: .85rem; }
.data-table th { padding: 10px 16px; text-align: left; font-size: .7rem; font-weight: 700; letter-spacing: .08em; text-transform: uppercase; color: var(--c-text-3); border-bottom: 1px solid var(--c-border); }
.data-table td { padding: 12px 16px; border-bottom: 1px solid var(--c-border); color: var(--c-text-2); vertical-align: middle; }
.data-row:last-child td { border-bottom: none; }
.data-row:hover td { background: var(--c-surface-2); }
.mono { font-family: var(--f-mono); font-size: .8rem; }
.fw   { font-weight: 600; color: var(--c-text); }
.empty-row { text-align: center; color: var(--c-text-3); padding: var(--space-xl) !important; }
.link-btn { color: var(--c-gold); font-weight: 600; font-size: .82rem; }
.link-btn:hover { opacity: .7; }
.row-actions { display: flex; gap: 6px; }
.icon-btn { background: none; border: 1px solid var(--c-border); border-radius: var(--r-sm); padding: 5px 9px; cursor: pointer; color: var(--c-text-3); font-size: .85rem; transition: all var(--t-fast); }
.icon-btn:hover { border-color: var(--c-gold-dim); color: var(--c-gold); background: rgba(201,168,76,.06); }
.icon-btn--danger:hover { border-color: rgba(192,57,43,.4); color: var(--c-red); background: var(--c-red-bg); }
.modal-form { display: flex; flex-direction: column; gap: var(--space-md); }
.form-row-2 { display: grid; grid-template-columns: 1fr 1fr; gap: var(--space-md); }
</style>
