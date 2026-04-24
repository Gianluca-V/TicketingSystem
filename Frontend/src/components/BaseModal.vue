<template>
  <teleport to="body">
    <transition name="modal">
      <div v-if="modelValue" class="modal-backdrop" @mousedown.self="$emit('update:modelValue', false)">
        <div
          class="modal-panel card"
          :style="{ maxWidth: width }"
          role="dialog"
          :aria-label="title"
          aria-modal="true"
        >
          <!-- Header -->
          <div class="modal-header">
            <h2 class="modal-title">{{ title }}</h2>
            <button class="modal-close" aria-label="Cerrar" @click="$emit('update:modelValue', false)">✕</button>
          </div>

          <!-- Body -->
          <div class="modal-body">
            <slot />
          </div>

          <!-- Footer -->
          <div v-if="$slots.footer" class="modal-footer">
            <slot name="footer" />
          </div>
        </div>
      </div>
    </transition>
  </teleport>
</template>

<script setup>
import { onMounted, onUnmounted } from 'vue'

const props = defineProps({
  modelValue: { type: Boolean, required: true },
  title:      { type: String,  default: '' },
  width:      { type: String,  default: '520px' },
})
defineEmits(['update:modelValue'])

function onKeydown(e) {
  if (e.key === 'Escape' && props.modelValue) {
    // emits handled via keyboard — parent must react
  }
}
onMounted(()  => document.addEventListener('keydown', onKeydown))
onUnmounted(() => document.removeEventListener('keydown', onKeydown))
</script>

<style scoped>
.modal-backdrop {
  position: fixed; inset: 0; z-index: 500;
  background: rgba(0,0,0,.72);
  backdrop-filter: blur(6px);
  display: flex; align-items: center; justify-content: center;
  padding: var(--space-lg);
}
.modal-panel {
  width: 100%;
  max-height: 90vh;
  display: flex; flex-direction: column;
  overflow: hidden;
}
.modal-header {
  display: flex; align-items: center; justify-content: space-between;
  padding: var(--space-lg) var(--space-lg) var(--space-md);
  border-bottom: 1px solid var(--c-border);
  flex-shrink: 0;
}
.modal-title {
  font-family: var(--f-display);
  font-size: 1.3rem; font-weight: 600;
  color: var(--c-text);
}
.modal-close {
  background: none; border: none;
  color: var(--c-text-3); cursor: pointer;
  font-size: .9rem; padding: 4px 8px;
  border-radius: var(--r-sm);
  transition: all var(--t-fast);
}
.modal-close:hover { background: var(--c-surface-2); color: var(--c-text); }
.modal-body {
  padding: var(--space-lg);
  overflow-y: auto; flex: 1;
}
.modal-footer {
  padding: var(--space-md) var(--space-lg);
  border-top: 1px solid var(--c-border);
  display: flex; justify-content: flex-end; gap: 10px;
  flex-shrink: 0;
}

/* Transition */
.modal-enter-active, .modal-leave-active { transition: all .2s ease; }
.modal-enter-from, .modal-leave-to { opacity: 0; }
.modal-enter-from .modal-panel, .modal-leave-to .modal-panel {
  transform: scale(.95) translateY(8px);
}
</style>
