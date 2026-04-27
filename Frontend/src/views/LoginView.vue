<template>
  <div class="auth-page">
    <div class="auth-bg">
      <div class="bg-glyph">◈</div>
    </div>

    <div class="auth-card card fade-in">
      <div class="auth-header">
        <span class="auth-logo-icon">◈</span>
        <h1 class="auth-title">STAGEFRONT</h1>
        <p class="auth-subtitle">Tu próxima experiencia te espera</p>
      </div>

      <div v-if="auth.error" class="alert alert-error">
        <span>⚠</span> 
          {{
          auth.error === 'Error 400'
            ? 'Correo o contraseña incorrectos'
            : 'Ha ocurrido un error. Por favor, intentá nuevamente.' 
        }}
      </div>

      <form class="auth-form" @submit.prevent="handleSubmit">
        <div class="form-group">
          <label class="form-label" for="email">Correo electrónico</label>
          <input
            id="email"
            v-model.trim="form.email"
            class="form-input"
            type="email"
            placeholder="usuario@ejemplo.com"
            autocomplete="username"
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
              placeholder="••••••••"
              autocomplete="current-password"
              required
              @focus="auth.clearError"
            />
            <button type="button" class="pwd-toggle" @click="showPwd = !showPwd">
              {{ showPwd ? '◑' : '◐' }}
            </button>
          </div>
        </div>

        <button
          type="submit"
          class="btn btn-primary btn-lg"
          style="width:100%;justify-content:center;margin-top:8px"
          :disabled="auth.loading"
        >
          <LoadingSpinner v-if="auth.loading" size="18px" />
          <span v-else>Ingresar</span>
        </button>
      </form>

      <p class="auth-footer">
        ¿No tenés cuenta?
        <router-link to="/register" class="auth-link">Registrate</router-link>
      </p>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import LoadingSpinner from '@/components/LoadingSpinner.vue'

const auth   = useAuthStore()
const router = useRouter()
const route  = useRoute()

const form   = ref({ email: '', password: '' })
const showPwd = ref(false)

async function handleSubmit() {
  const ok = await auth.login(form.value.email, form.value.password)
  if (ok) {
    const redirect = route.query.redirect ?? '/'
    router.push(redirect)
  }
}
</script>

<style scoped>
.auth-page {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: var(--space-lg);
  position: relative;
  overflow: hidden;
}
.auth-bg {
  position: absolute; inset: 0;
  display: flex; align-items: center; justify-content: center;
  pointer-events: none;
}
.bg-glyph {
  font-size: 40vw;
  color: rgba(201,168,76,.03);
  user-select: none;
  line-height: 1;
}
.auth-card {
  width: 100%;
  max-width: 440px;
  padding: var(--space-xl);
  position: relative;
  z-index: 1;
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
}
.auth-header { text-align: center; }
.auth-logo-icon { font-size: 2rem; color: var(--c-gold); }
.auth-title {
  font-family: var(--f-ui);
  font-size: .8rem;
  font-weight: 800;
  letter-spacing: .28em;
  color: var(--c-text);
  margin-top: 4px;
}
.auth-subtitle {
  margin-top: 8px;
  font-family: var(--f-display);
  font-size: 1.1rem;
  font-style: italic;
  color: var(--c-text-3);
}
.auth-form { display: flex; flex-direction: column; gap: var(--space-md); }
.input-row { position: relative; }
.input-row .form-input { padding-right: 44px; }
.pwd-toggle {
  position: absolute;
  right: 12px; top: 50%;
  transform: translateY(-50%);
  background: none; border: none;
  color: var(--c-text-3);
  cursor: pointer;
  font-size: 1rem;
  transition: color var(--t-fast);
}
.pwd-toggle:hover { color: var(--c-gold); }
.auth-footer {
  text-align: center;
  font-size: .85rem;
  color: var(--c-text-3);
}
.auth-link {
  color: var(--c-gold);
  font-weight: 600;
  margin-left: 4px;
  transition: color var(--t-fast);
}
.auth-link:hover { color: var(--c-gold-light); }
</style>
