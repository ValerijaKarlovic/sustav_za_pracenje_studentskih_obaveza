<script setup>
defineProps({
  message: { type: String, required: true },
  type: { type: String, default: 'alert' },
})
defineEmits(['confirm', 'cancel'])
</script>

<template>
  <div class="modal-overlay" @click.self="$emit('cancel')">
    <div class="modal-box dialog-box" role="dialog" aria-modal="true">
      <div class="modal-header">
        <h3>{{ type === 'confirm' ? 'Potvrda' : 'Obavijest' }}</h3>
        <button class="modal-close" aria-label="Zatvori" @click="$emit('cancel')">✕</button>
      </div>
      <p class="dialog-message">{{ message }}</p>
      <div class="modal-actions">
        <button v-if="type === 'confirm'" class="secondary" @click="$emit('cancel')">Odustani</button>
        <button class="small-primary" @click="$emit('confirm')">{{ type === 'confirm' ? 'Potvrdi' : 'U redu' }}</button>
      </div>
    </div>
  </div>
</template>
