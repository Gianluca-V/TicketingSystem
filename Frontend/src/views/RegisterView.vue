<template>
  <div class="auth-page">
    <div class="auth-bg"><div class="bg-glyph">◈</div></div>

    <div class="auth-card card fade-in">
      <div class="auth-header">
        <span class="auth-logo-icon">◈</span>
        <h1 class="auth-title">STAGEFRONT</h1>
        <p class="auth-subtitle">Crear una cuenta</p>
      </div>

      <div v-if="auth.error" class="alert alert-error">
        <span>⚠</span> {{ auth.error }}
      </div>

      <form class="auth-form" @submit.prevent="handleSubmit">
        <div class="form-group">
          <label class="form-label" for="name">Nombre completo</label>
          <input
            id="name"
            v-model.trim="form.name"
            class="form-input"
            type="text"
            placeholder="Juan Pérez"
            autocomplete="name"
            required
            @focus="auth.clearError"
          />
        </div>

        <div class="form-group">
          <label class="form-label" for="email">Correo electrónico</label>
          <input
            id="email"
            v-model.trim="form.email"
            class="form-input"
            type="email"
            placeholder="usuario@ejemplo.com"
            autocomplete="email"
            required
            @focus="auth.clearError"
          />
        </div>

        <div class="form-group">
          <label class="form-label" for="password">Contraseña</label>
          <div class="input-row">
            <input
              id="password"
              v-model="form.password"
              class="form-input"
              :type="showPwd ? 'text' : 'password'"
              placeholder="Mínimo 8 caracteres"
              autocomplete="new-password"
              minlength="8"
              required
              @focus="auth.clearError"
            />
            <button type="button" class="pwd-toggle" @click="showPwd = !showPwd">
              {{ showPwd ? '◑' : '◐' }}
            </button>
          </div>
        </div>

        <div class="form-group">
          <label class="form-label" for="confirm">Confirmar contraseña</label>
          <input
            id="confirm"
            v-model="form.confirm"
            class="form-input"
            :type="showPwd ? 'text' : 'password'"
            placeholder="Repetí tu contraseña"
            autocomplete="new-password"
            required
          />
          <p v-if="mismatch" class="form-error">Las contraseñas no coinciden.</p>
        </div>

        <button
          type="submit"
          class="btn btn-primary btn-lg"
          style="width:100%;justify-content:center;margin-top:8px"
          :disabled="auth.loading || mismatch"
        >
          <LoadingSpinner v-if="auth.loading" size="18px" />
          <span v-else>Crear cuenta</span>
        </button>
      </form>

      <p class="auth-footer">
        ¿Ya tenés cuenta?
        <router-link to="/login" class="auth-link">Iniciar sesión</router-link>
      </p>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import LoadingSpinner from '@/components/LoadingSpinner.vue'

const auth   = useAuthStore()
const router = useRouter()

const form    = ref({ name: '', email: '', password: '', confirm: '' })
const showPwd = ref(false)

const mismatch = computed(
  () => form.value.confirm.length > 0 && form.value.password !== form.value.confirm,
)

async function handleSubmit() {
  if (mismatch.value) return
  const ok = await auth.register(form.value.name, form.value.email, form.value.password)
  if (ok) router.push('/')
}
</script>

<style scoped>
.auth-page {
  min-height: 100vh;
  display: flex; align-items: center; justify-content: center;
  padding: var(--space-lg);
  position: relative; overflow: hidden;
}
.auth-bg { position: absolute; inset: 0; display: flex; align-items: center; justify-content: center; pointer-events: none; }
.bg-glyph { font-size: 40vw; color: rgba(201,168,76,.03); user-select: none; line-height: 1; }
.auth-card {
  width: 100%; max-width: 440px;
  padding: var(--space-xl);
  position: relative; z-index: 1;
  display: flex; flex-direction: column; gap: var(--space-lg);
}
.auth-header { text-align: center; }
.auth-logo-icon { font-size: 2rem; color: var(--c-gold); }
.auth-title { font-family: var(--f-ui); font-size: .8rem; font-weight: 800; letter-spacing: .28em; color: var(--c-text); margin-top: 4px; }
.auth-subtitle { margin-top: 8px; font-family: var(--f-display); font-size: 1.1rem; font-style: italic; color: var(--c-text-3); }
.auth-form { display: flex; flex-direction: column; gap: var(--space-md); }
.input-row { position: relative; }
.input-row .form-input { padding-right: 44px; }
.pwd-toggle { position: absolute; right: 12px; top: 50%; transform: translateY(-50%); background: none; border: none; color: var(--c-text-3); cursor: pointer; font-size: 1rem; transition: color var(--t-fast); }
.pwd-toggle:hover { color: var(--c-gold); }
.auth-footer { text-align: center; font-size: .85rem; color: var(--c-text-3); }
.auth-link { color: var(--c-gold); font-weight: 600; margin-left: 4px; transition: color var(--t-fast); }
.auth-link:hover { color: var(--c-gold-light); }
</style>
