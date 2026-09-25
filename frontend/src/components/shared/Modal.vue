<script setup>
defineProps({
  title: String,
  subtitle: String,
})
defineEmits(['close'])
</script>

<template>
  <div class="modal-overlay" @click.self="$emit('close')">
    <div class="modal-box">
      <div class="modal-header">
        <div>
          <h3>{{ title }}</h3>
          <p v-if="subtitle">{{ subtitle }}</p>
        </div>
        <button class="modal-close" @click="$emit('close')">✕</button>
      </div>
      <slot />
    </div>
  </div>
</template>
