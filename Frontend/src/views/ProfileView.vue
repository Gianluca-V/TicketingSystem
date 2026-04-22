<template>
  <div class="profile-page">
    <div class="container narrow">

      <p class="eyebrow">Mi cuenta</p>
      <h1 class="page-title">Perfil</h1>

      <div class="avatar-row">
        <div class="avatar">{{ initials }}</div>
        <div>
          <p class="avatar-name">{{ auth.user?.name }}</p>
          <p class="avatar-email">{{ auth.user?.email }}</p>
        </div>
      </div>

      <!-- Edit form -->
      <div class="card profile-card">
        <p class="card-section-title">Información personal</p>

        <div v-if="successMsg" class="alert alert-success" style="margin-bottom:var(--space-md)">
          <span>✓</span> {{ successMsg }}
        </div>
        <div v-if="formError" class="alert alert-error" style="margin-bottom:var(--space-md)">
          <span>⚠</span> {{ formError }}
        </div>

        <form class="profile-form" @submit.prevent="handleSave">
          <div class="form-group">
            <label class="form-label">Nombre</label>
            <input v-model.trim="form.name" class="form-input" type="text" required />
          </div>
          <div class="form-group">
            <label class="form-label">Correo electrónico</label>
            <input v-model.trim="form.email" class="form-input" type="email" required />
          </div>
          <div class="form-group">
            <label class="form-label">Nueva contraseña <span class="optional">(opcional)</span></label>
            <input v-model="form.password" class="form-input" type="password" placeholder="Dejar vacío para no cambiar" minlength="8" />
          </div>
          <div class="form-actions">
            <button type="button" class="btn btn-ghost" @click="resetForm">Cancelar</button>
            <button type="submit" class="btn btn-primary" :disabled="saving">
              <LoadingSpinner v-if="saving" size="16px" />
              <span v-else>Guardar cambios</span>
            </button>
          </div>
        </form>
      </div>

      <!-- Danger zone -->
      <div class="danger-zone card">
        <p class="card-section-title danger">Zona de peligro</p>
        <div class="danger-row">
          <div>
            <p class="danger-label">Cerrar sesión</p>
            <p class="danger-desc">Salís de tu cuenta en este dispositivo.</p>
          </div>
          <button class="btn btn-danger" @click="handleLogout">Cerrar sesión</button>
        </div>
      </div>

    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { authApi } from '@/api/auth'
import { useAuthStore } from '@/stores/auth'
import { useReservationStore } from '@/stores/reservation'
import LoadingSpinner from '@/components/LoadingSpinner.vue'

const auth        = useAuthStore()
const reservation = useReservationStore()
const router      = useRouter()

const saving    = ref(false)
const formError = ref(null)
const successMsg = ref(null)

const form = ref({
  name:     auth.user?.name  ?? '',
  email:    auth.user?.email ?? '',
  password: '',
})

const initials = computed(() => {
  const n = auth.user?.name ?? ''
  return n.split(' ').map(w => w[0]).slice(0, 2).join('').toUpperCase() || '?'
})

function resetForm() {
  form.value = { name: auth.user?.name ?? '', email: auth.user?.email ?? '', password: '' }
  formError.value  = null
  successMsg.value = null
}

async function handleSave() {
  if (!auth.userId) return
  saving.value    = true
  formError.value = null
  successMsg.value = null
  try {
    const payload = { name: form.value.name, email: form.value.email }
    if (form.value.password) payload.password = form.value.password
    await authApi.updateProfile(auth.userId, payload)
    // Patch local store
    auth.user.name  = form.value.name
    auth.user.email = form.value.email
    sessionStorage.setItem('_usr', JSON.stringify(auth.user))
    successMsg.value = 'Perfil actualizado correctamente.'
    form.value.password = ''
  } catch (e) {
    formError.value = e.message
  } finally {
    saving.value = false
  }
}

function handleLogout() {
  reservation.clear()
  auth.logout()
  router.push('/login')
}
</script>

<style scoped>
.profile-page { padding-block: var(--space-xl) var(--space-2xl); }
.narrow { max-width: 560px; }

.eyebrow { font-size: .72rem; font-weight: 700; letter-spacing: .14em; text-transform: uppercase; color: var(--c-gold); margin-bottom: 8px; }
.page-title { font-family: var(--f-display); font-size: clamp(1.8rem, 3vw, 2.5rem); font-weight: 600; color: var(--c-text); margin-bottom: var(--space-xl); }

.avatar-row { display: flex; align-items: center; gap: var(--space-md); margin-bottom: var(--space-xl); }
.avatar {
  width: 60px; height: 60px;
  border-radius: 50%;
  background: rgba(201,168,76,.15);
  border: 1px solid var(--c-gold-dim);
  display: flex; align-items: center; justify-content: center;
  font-family: var(--f-ui);
  font-size: 1.1rem; font-weight: 700;
  color: var(--c-gold);
  flex-shrink: 0;
}
.avatar-name  { font-size: 1rem; font-weight: 600; color: var(--c-text); }
.avatar-email { font-size: .85rem; color: var(--c-text-3); margin-top: 2px; }

.profile-card, .danger-zone { padding: var(--space-lg); margin-bottom: var(--space-lg); }
.card-section-title {
  font-size: .75rem; font-weight: 700;
  letter-spacing: .1em; text-transform: uppercase;
  color: var(--c-text-3);
  margin-bottom: var(--space-md);
  padding-bottom: var(--space-sm);
  border-bottom: 1px solid var(--c-border);
}
.card-section-title.danger { color: var(--c-red); border-bottom-color: rgba(192,57,43,.2); }

.profile-form { display: flex; flex-direction: column; gap: var(--space-md); }
.optional { font-weight: 400; color: var(--c-text-3); text-transform: none; letter-spacing: 0; font-size: .9em; }
.form-actions { display: flex; gap: 10px; justify-content: flex-end; padding-top: var(--space-sm); }

.danger-row { display: flex; align-items: center; justify-content: space-between; gap: var(--space-md); }
.danger-label { font-size: .9rem; font-weight: 600; color: var(--c-text); }
.danger-desc  { font-size: .82rem; color: var(--c-text-3); margin-top: 2px; }
</style>
