<template>
  <div class="admin-users">
    <div class="view-header">
      <div>
        <h1 class="page-title">Usuarios</h1>
        <p class="page-desc">Administrá cuentas de usuario.</p>
      </div>
    </div>

    <div v-if="error" class="alert alert-error">
      <span>⚠</span> {{ error }}
      <button class="btn btn-ghost btn-sm" style="margin-left:auto" @click="load">Reintentar</button>
    </div>

    <div class="table-wrap card">
      <div class="table-toolbar">
        <input v-model.trim="search" class="form-input search-input" type="search" placeholder="Buscar por nombre o email..." />
        <span class="total-badge">{{ filtered.length }} usuarios</span>
      </div>

      <div v-if="loading" class="table-loading">
        <LoadingSpinner label="Cargando usuarios..." />
      </div>

      <table v-else class="data-table">
        <thead>
          <tr>
            <th>ID</th>
            <th>Nombre</th>
            <th>Email</th>
            <th>Acciones</th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="!filtered.length">
            <td colspan="4" class="empty-row">No hay usuarios registrados.</td>
          </tr>
          <tr v-for="u in filtered" :key="u.id" class="data-row" :class="{ 'row-self': u.id == auth.userId }">
            <td class="mono">{{ u.id }}</td>
            <td>
              <div class="user-cell">
                <div class="user-avatar">{{ initials(u.name) }}</div>
                <span class="fw">{{ u.name }}</span>
                <span v-if="u.id == auth.userId" class="badge badge-gold" style="font-size:.62rem">Tú</span>
              </div>
            </td>
            <td class="mono email-cell">{{ u.email }}</td>
            <td>
              <div class="row-actions">
                <button class="icon-btn" title="Editar" @click="openEdit(u)">✎</button>
                <button
                  class="icon-btn icon-btn--danger"
                  title="Eliminar"
                  :disabled="u.id == auth.userId"
                  @click="openDelete(u)"
                >✕</button>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Edit modal -->
    <BaseModal v-model="showForm" title="Editar usuario" width="460px">
      <form class="modal-form" @submit.prevent="handleSave">
        <div class="form-group">
          <label class="form-label">Nombre *</label>
          <input v-model.trim="form.name" class="form-input" type="text" required />
        </div>
        <div class="form-group">
          <label class="form-label">Email *</label>
          <input v-model.trim="form.email" class="form-input" type="email" required />
        </div>
        <div class="form-group">
          <label class="form-label">Nueva contraseña <span class="optional">(dejar vacío para no cambiar)</span></label>
          <input v-model="form.password" class="form-input" type="password" minlength="8" />
        </div>
        <div v-if="formError" class="alert alert-error"><span>⚠</span> {{ formError }}</div>
      </form>
      <template #footer>
        <button class="btn btn-ghost" @click="showForm = false">Cancelar</button>
        <button class="btn btn-primary" :disabled="saving" @click="handleSave">
          <LoadingSpinner v-if="saving" size="16px" />
          <span v-else>Guardar cambios</span>
        </button>
      </template>
    </BaseModal>

    <ConfirmDialog
      v-model="showDelete"
      title="Eliminar usuario"
      :message="`¿Eliminar la cuenta de &quot;${deleteTarget?.name}&quot;? Esta acción no se puede deshacer.`"
      :loading="deleting"
      @confirm="handleDelete"
    />
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { adminUsersApi } from '@/api/admin'
import { useAuthStore } from '@/stores/auth'
import BaseModal      from '@/components/BaseModal.vue'
import ConfirmDialog  from '@/components/ConfirmDialog.vue'
import LoadingSpinner from '@/components/LoadingSpinner.vue'

const auth    = useAuthStore()
const users   = ref([])
const loading = ref(false)
const error   = ref(null)
const search  = ref('')

const showForm   = ref(false)
const editTarget = ref(null)
const form       = ref({ name: '', email: '', password: '' })
const formError  = ref(null)
const saving     = ref(false)

const showDelete   = ref(false)
const deleteTarget = ref(null)
const deleting     = ref(false)

const filtered = computed(() => {
  const q = search.value.toLowerCase()
  if (!q) return users.value
  return users.value.filter(u =>
    u.name?.toLowerCase().includes(q) || u.email?.toLowerCase().includes(q),
  )
})

function initials(name) {
  return (name ?? '?').split(' ').map(w => w[0]).slice(0, 2).join('').toUpperCase()
}

async function load() {
  loading.value = true; error.value = null
  try {
    const res = await adminUsersApi.list()
    users.value = Array.isArray(res) ? res : res?.items ?? []
  } catch (e) { error.value = e.message }
  finally { loading.value = false }
}

function openEdit(u) {
  editTarget.value = u
  form.value = { name: u.name, email: u.email, password: '' }
  formError.value = null; showForm.value = true
}
function openDelete(u) { deleteTarget.value = u; showDelete.value = true }

async function handleSave() {
  saving.value = true; formError.value = null
  try {
    const payload = { name: form.value.name, email: form.value.email }
    if (form.value.password) payload.password = form.value.password
    await adminUsersApi.update(editTarget.value.id, payload)
    
    // Update local state manually since server returns 204
    const idx = users.value.findIndex(u => u.id === editTarget.value.id)
    if (idx !== -1) {
      users.value[idx] = { ...users.value[idx], name: payload.name, email: payload.email }
    }
    
    showForm.value = false
  } catch (e) { formError.value = e.message }
  finally { saving.value = false }
}

async function handleDelete() {
  deleting.value = true
  try {
    await adminUsersApi.delete(deleteTarget.value.id)
    users.value = users.value.filter(u => u.id !== deleteTarget.value.id)
    showDelete.value = false
  } catch (e) { error.value = e.message }
  finally { deleting.value = false }
}

onMounted(load)
</script>

<style scoped>
.admin-users { max-width: 900px; }
.view-header { margin-bottom: var(--space-xl); }
.page-title { font-family: var(--f-display); font-size: 1.8rem; font-weight: 600; color: var(--c-text); margin-bottom: 4px; }
.page-desc  { font-size: .85rem; color: var(--c-text-3); }

.table-wrap { overflow: hidden; }
.table-toolbar { display: flex; align-items: center; justify-content: space-between; padding: var(--space-md) var(--space-lg); border-bottom: 1px solid var(--c-border); gap: var(--space-md); flex-wrap: wrap; }
.search-input { max-width: 320px; font-size: .85rem; }
.total-badge { font-size: .75rem; color: var(--c-text-3); white-space: nowrap; }
.table-loading { padding: var(--space-2xl); display: flex; justify-content: center; }

.data-table { width: 100%; border-collapse: collapse; font-size: .85rem; }
.data-table th { padding: 10px 16px; text-align: left; font-size: .7rem; font-weight: 700; letter-spacing: .08em; text-transform: uppercase; color: var(--c-text-3); border-bottom: 1px solid var(--c-border); }
.data-table td { padding: 11px 16px; border-bottom: 1px solid var(--c-border); color: var(--c-text-2); vertical-align: middle; }
.data-row:last-child td { border-bottom: none; }
.data-row:hover td { background: var(--c-surface-2); }
.row-self td { background: rgba(201,168,76,.03); }
.mono { font-family: var(--f-mono); font-size: .8rem; }
.fw   { font-weight: 600; color: var(--c-text); }
.email-cell { color: var(--c-text-3); font-size: .8rem; }
.empty-row { text-align: center; color: var(--c-text-3); padding: var(--space-xl) !important; }

.user-cell { display: flex; align-items: center; gap: 10px; }
.user-avatar {
  width: 32px; height: 32px; border-radius: 50%;
  background: rgba(201,168,76,.12); border: 1px solid var(--c-gold-dim);
  display: flex; align-items: center; justify-content: center;
  font-size: .7rem; font-weight: 700; color: var(--c-gold);
  flex-shrink: 0;
}
.row-actions { display: flex; gap: 6px; }
.icon-btn { background: none; border: 1px solid var(--c-border); border-radius: var(--r-sm); padding: 5px 9px; cursor: pointer; color: var(--c-text-3); font-size: .85rem; transition: all var(--t-fast); }
.icon-btn:disabled { opacity: .3; cursor: not-allowed; }
.icon-btn:not(:disabled):hover { border-color: var(--c-gold-dim); color: var(--c-gold); background: rgba(201,168,76,.06); }
.icon-btn--danger:not(:disabled):hover { border-color: rgba(192,57,43,.4); color: var(--c-red); background: var(--c-red-bg); }
.modal-form { display: flex; flex-direction: column; gap: var(--space-md); }
.optional { font-weight: 400; color: var(--c-text-3); font-size: .85em; text-transform: none; letter-spacing: 0; }
</style>
