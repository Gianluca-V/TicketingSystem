<template>
  <div class="spinner-wrap" :class="{ overlay }">
    <div class="spinner" :style="{ width: size, height: size }">
      <svg viewBox="0 0 50 50">
        <circle class="track" cx="25" cy="25" r="20" fill="none" stroke-width="3" />
        <circle class="arc"   cx="25" cy="25" r="20" fill="none" stroke-width="3"
          stroke-linecap="round"
          stroke-dasharray="80 120"
          stroke-dashoffset="0"
        />
      </svg>
    </div>
    <p v-if="label" class="spinner-label">{{ label }}</p>
  </div>
</template>

<script setup>
defineProps({
  size:    { type: String, default: '40px' },
  label:   { type: String, default: '' },
  overlay: { type: Boolean, default: false },
})
</script>

<style scoped>
.spinner-wrap {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 12px;
  padding: var(--space-xl);
}
.spinner-wrap.overlay {
  position: fixed; inset: 0;
  background: rgba(8,8,8,.7);
  backdrop-filter: blur(4px);
  z-index: 999;
}
.spinner svg { animation: rotate 1.2s linear infinite; }
.track { stroke: var(--c-border); }
.arc   { stroke: var(--c-gold); }
.spinner-label {
  font-size: .8rem;
  color: var(--c-text-3);
  letter-spacing: .06em;
  text-transform: uppercase;
}

@keyframes rotate { from { transform: rotate(0); } to { transform: rotate(360deg); } }
</style>
