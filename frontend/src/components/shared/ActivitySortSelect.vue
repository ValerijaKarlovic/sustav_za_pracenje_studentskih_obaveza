<script setup>
defineProps({
  modelValue: { type: String, default: 'datum-asc' },
})
defineEmits(['update:modelValue'])
</script>

<template>
  <select
    :value="modelValue"
    aria-label="Poredaj aktivnosti po datumu"
    @change="$emit('update:modelValue', $event.target.value)"
  >
    <option value="datum-asc">Datum: najstarije prvo</option>
    <option value="datum-desc">Datum: najnovije prvo</option>
  </select>
</template>
