<script setup>
defineProps({
  status: String, // 'Odrađeno' | 'Čeka se' | 'Nije odrađeno'
})

const statusClass = {
  'Odrađeno': 'done',
  'Čeka se': 'pending',
  'Nije odrađeno': 'missed',
}
</script>

<template>
  <span class="badge" :class="statusClass[status] || ''">{{ status }}</span>
</template>
