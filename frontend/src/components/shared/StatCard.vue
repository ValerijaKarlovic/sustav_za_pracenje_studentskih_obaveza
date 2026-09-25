<script setup>
defineProps({
  label: String,
  value: [String, Number],
})
</script>

<template>
  <div class="stat-card">
    <div class="label">{{ label }}</div>
    <div class="value"><slot>{{ value }}</slot></div>
  </div>
</template>
