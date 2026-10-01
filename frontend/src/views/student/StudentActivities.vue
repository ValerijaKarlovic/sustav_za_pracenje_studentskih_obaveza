<script setup>
import { ref, computed, onMounted } from 'vue'
import api from '@/api/client'
import ActivityItem from '@/components/shared/ActivityItem.vue'
import ActivitySortSelect from '@/components/shared/ActivitySortSelect.vue'
import { sortirajAktivnostiPoDatumu } from '@/utils/sortiranjeAktivnosti'
import { formatBodovi } from '@/utils/formatBodovi'

const aktivnosti = ref([])
const kolegiji = ref([])

onMounted(async () => {
  const [aktivnostiOdgovor, kolegijiOdgovor] = await Promise.all([
    api.get('/student/aktivnosti'),
    api.get('/student/kolegiji'),
  ])
  const { data } = aktivnostiOdgovor
  kolegiji.value = kolegijiOdgovor.data.map(k => k.naziv ?? k.kolegij)
  aktivnosti.value = data.map(a => ({
    ...a,
    datumRaw: a.datum ?? null,
    datum: a.datum ? new Date(a.datum).toLocaleDateString('hr-HR') : '-',
    status: a.status === 'odradeno' ? 'Odrađeno' : a.status === 'ceka_se' ? 'Čeka se' : 'Nije odrađeno',
    bodovi: a.bodovi === null ? '-' : formatBodovi(a.bodovi),
  }))
})

const filterKolegij = ref('Svi kolegiji')
const filterStatus = ref('Svi statusi')
const poredajPo = ref('datum-asc')

const filtrirane = computed(() => {
  const lista = aktivnosti.value.filter(a =>
    (filterKolegij.value === 'Svi kolegiji' || a.kolegij === filterKolegij.value) &&
    (filterStatus.value === 'Svi statusi' || a.status === filterStatus.value),
  )
  return sortirajAktivnostiPoDatumu(lista, poredajPo.value)
})
</script>

<template>
  <nav class="tabs">
    <RouterLink to="/student">Dashboard</RouterLink>
    <RouterLink to="/student/aktivnosti">Moje aktivnosti</RouterLink>
    <RouterLink to="/student/profil">Profil</RouterLink>
  </nav>

  <main class="page-content">
    <h2 class="page-title">Moje aktivnosti</h2>

    <div class="filters">
      <select v-model="filterKolegij">
        <option>Svi kolegiji</option>
        <option v-for="k in kolegiji" :key="k">{{ k }}</option>
      </select>
      <select v-model="filterStatus">
        <option>Svi statusi</option>
        <option>Odrađeno</option>
        <option>Čeka se</option>
        <option>Nije odrađeno</option>
      </select>
      <ActivitySortSelect v-model="poredajPo" />
    </div>

    <ActivityItem v-for="a in filtrirane" :key="a.naziv + a.datum" v-bind="a" />
    <p v-if="!filtrirane.length" style="color:var(--muted); font-size:13px;">Nema aktivnosti za odabrane filtre.</p>
  </main>
</template>
