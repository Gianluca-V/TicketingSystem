<template>
  <BaseModal :model-value="modelValue" :title="title" width="400px" @update:model-value="$emit('update:modelValue', $event)">
    <p class="confirm-msg">{{ message }}</p>
    <template #footer>
      <button class="btn btn-ghost" @click="$emit('update:modelValue', false)">Cancelar</button>
      <button class="btn btn-danger" :disabled="loading" @click="$emit('confirm')">
        <LoadingSpinner v-if="loading" size="16px" />
        <span v-else>{{ confirmLabel }}</span>
      </button>
    </template>
  </BaseModal>
</template>

<script setup>
import BaseModal from './BaseModal.vue'
import LoadingSpinner from './LoadingSpinner.vue'

defineProps({
  modelValue:   { type: Boolean, required: true },
  title:        { type: String,  default: 'Confirmar acción' },
  message:      { type: String,  default: '¿Estás seguro? Esta acción no se puede deshacer.' },
  confirmLabel: { type: String,  default: 'Eliminar' },
  loading:      { type: Boolean, default: false },
})
defineEmits(['update:modelValue', 'confirm'])
</script>

<style scoped>
.confirm-msg { font-size: .9rem; color: var(--c-text-2); line-height: 1.6; }
</style>
