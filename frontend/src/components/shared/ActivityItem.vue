<script setup>
import Badge from './Badge.vue'

defineProps({
  naziv: String,
  kolegij: String,
  vrsta: String,
  datum: String,
  status: String,
  bodovi: [String, Number],
  opis: { type: String, default: '' },
})
</script>

<template>
  <div class="activity-item">
    <div>
      <div class="top-row title-line">
        <span class="aname">{{ naziv }}</span>
        <span class="title-sep" aria-hidden="true">-</span>
        <span class="course">{{ kolegij }}</span>
      </div>
      <div class="meta">{{ vrsta }} - {{ datum }}</div>
      <p v-if="opis?.trim()" class="activity-opis">{{ opis.trim() }}</p>
    </div>
    <div class="bottom-row" style="width:auto;">
      <Badge :status="status" />
      <span class="points" style="margin-left:10px;">{{ bodovi }}</span>
    </div>
  </div>
</template>
