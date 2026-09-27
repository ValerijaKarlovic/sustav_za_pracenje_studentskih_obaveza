<script setup>
import { computed, onMounted, onUnmounted, ref, watch } from 'vue'
import { onBeforeRouteUpdate } from 'vue-router'
import api from '@/api/client'
import StatCard from '@/components/shared/StatCard.vue'
import CourseProgressModal from '@/components/shared/CourseProgressModal.vue'
import {
  donutGradient,
  segmentiDonutaPoVrstama,
  sortirajVrste,
} from '@/utils/bodoviPoVrsti'
import { ectsOstvarenoZaKolegij, mapObvezeZaPrikaz, statusLabelStudenta } from '@/utils/aktivnostiPrikaz'

const kolegiji = ref([])
const aktivnosti = ref([])

const ukupnoBodova = computed(() => kolegiji.value.reduce((s, k) => s + Number(k.bodovi), 0))
const ukupnoMax = computed(() => kolegiji.value.reduce((s, k) => s + Number(k.maxBodovi), 0))
function aktivnostiZaKolegij(nazivKolegija) {
  return aktivnosti.value.filter(a => a.kolegij === nazivKolegija)
}

const ukupnoEcts = computed(() =>
  kolegiji.value
    .reduce(
      (s, k) =>
        s + ectsOstvarenoZaKolegij(k.prolazi, k.ects, aktivnostiZaKolegij(k.naziv), k.maxBodovi),
      0,
    )
    .toFixed(1),
)
const ukupnoEctsMax = computed(() => kolegiji.value.reduce((s, k) => s + k.ects, 0))

const upisaniKolegiji = computed(() => new Set(kolegiji.value.map(k => k.naziv)))

const vrsteNaUpisanimKolegijima = computed(() =>
  sortirajVrste(
    aktivnosti.value
      .filter(a => upisaniKolegiji.value.has(a.kolegij))
      .map(a => a.vrsta)
      .filter(Boolean),
  ),
)

const angažmanPoVrsti = computed(() =>
  segmentiDonutaPoVrstama(
    aktivnosti.value.filter(a => upisaniKolegiji.value.has(a.kolegij)),
    vrsteNaUpisanimKolegijima.value,
  ),
)

function aktivnostJeOdradena(a) {
  return a.status === 'odradeno' || (a.bodovi != null && Number(a.bodovi) > 0)
}

function aktivnostJeUTekucemMjesecu(datum) {
  if (!datum) return false
  const tekuci = new Date()
  const d = new Date(datum)
  if (Number.isNaN(d.getTime())) return false
  return d.getMonth() === tekuci.getMonth() && d.getFullYear() === tekuci.getFullYear()
}

const odradjenoOvajMjesec = computed(() =>
  aktivnosti.value.filter(a => aktivnostJeOdradena(a) && aktivnostJeUTekucemMjesecu(a.datum)).length,
)

const selectedCourse = ref(null)

async function ucitajAktivnosti() {
  const { data } = await api.get('/student/aktivnosti')
  aktivnosti.value = data
}

async function ucitajKolegije() {
  const { data } = await api.get('/student/kolegiji')
  kolegiji.value = data.map(k => ({
    kolegijId: k.kolegijId,
    naziv: k.kolegij,
    ects: k.ects,
    ectsOstvareno: 0,
    bodovi: Number(k.bodovi),
    maxBodovi: Number(k.ukupnoBodova),
    prolazi: k.prolazi,
    obveze: [],
  }))
}

async function ucitajDashboard() {
  await Promise.all([ucitajKolegije(), ucitajAktivnosti()])
}

async function openCourse(k) {
  const [, { data }] = await Promise.all([
    ucitajAktivnosti(),
    api.get(`/student/kolegiji/${k.kolegijId}`),
  ])
  const osvjezeni = kolegiji.value.find(x => x.kolegijId === k.kolegijId) ?? k
  const obvezeKolegija = data.aktivnosti ?? []
  selectedCourse.value = {
    ...osvjezeni,
    bodovi: Number(data.bodovi ?? osvjezeni.bodovi),
    maxBodovi: Number(data.maxBodovi ?? osvjezeni.maxBodovi),
    prolazi: data.prolazi ?? osvjezeni.prolazi,
    nastavnik: data.nastavnik ?? '',
    aktivnostiKolegija: obvezeKolegija,
    obveze: mapObvezeZaPrikaz(obvezeKolegija),
  }
}

async function osvjeziPodatke() {
  const otvoreniKolegijId = selectedCourse.value?.kolegijId
  await ucitajDashboard()
  if (otvoreniKolegijId) {
    const k = kolegiji.value.find(x => x.kolegijId === otvoreniKolegijId)
    if (k) await openCourse(k)
  }
}

function onVisibilityChange() {
  if (document.visibilityState === 'visible') osvjeziPodatke()
}

onMounted(() => {
  ucitajDashboard()
  window.addEventListener('focus', osvjeziPodatke)
  document.addEventListener('visibilitychange', onVisibilityChange)
})

onUnmounted(() => {
  window.removeEventListener('focus', osvjeziPodatke)
  document.removeEventListener('visibilitychange', onVisibilityChange)
})

onBeforeRouteUpdate(() => {
  osvjeziPodatke()
})

watch(aktivnosti, () => {
  if (!selectedCourse.value) return
  const k = kolegiji.value.find(x => x.kolegijId === selectedCourse.value.kolegijId)
  const zaKolegij = aktivnosti.value.filter(a => a.kolegij === selectedCourse.value.naziv)
  selectedCourse.value = {
    ...selectedCourse.value,
    ...(k ? { bodovi: k.bodovi, maxBodovi: k.maxBodovi, prolazi: k.prolazi } : {}),
    aktivnostiKolegija: zaKolegij,
    obveze: mapObvezeZaPrikaz(zaKolegij),
  }
})
</script>

<template>
  <nav class="tabs">
    <RouterLink to="/student">Dashboard</RouterLink>
    <RouterLink to="/student/aktivnosti">Moje aktivnosti</RouterLink>
    <RouterLink to="/student/profil">Profil</RouterLink>
  </nav>

  <main class="page-content">
    <h2 class="page-title">Dobrodošla, Ana</h2>

    <div class="stats-row">
      <StatCard label="Ukupno bodova">{{ ukupnoBodova }} <span class="value-of">/ {{ ukupnoMax }}</span></StatCard>
      <StatCard label="Ukupno ECTS">{{ ukupnoEcts }} <span class="value-of">/ {{ ukupnoEctsMax }}</span></StatCard>
      <StatCard label="Odrađeno ovaj mjesec" :value="odradjenoOvajMjesec" />
    </div>

    <div class="section-title">Bodovi po kolegiju</div>
    <div class="card">
      <div class="bar-row" v-for="k in kolegiji" :key="k.naziv" @click="openCourse(k)">
        <div class="name">
          <span class="student-name-link">{{ k.naziv }} ({{ k.ects }} ECTS)</span>
        </div>
        <div class="bar-track">
          <div class="bar-fill" :style="{ width: (k.bodovi / k.maxBodovi * 100) + '%' }"></div>
        </div>
        <div class="num">{{ k.bodovi }} / {{ k.maxBodovi }}</div>
      </div>
    </div>

    <div class="section-title">Angažman po vrsti aktivnosti</div>
    <div class="card">
      <div v-if="angažmanPoVrsti.length" class="pie-wrap">
        <div class="pie-chart donut" :style="{ background: donutGradient(angažmanPoVrsti) }"></div>
        <div class="pie-legend">
          <div class="legend-item" v-for="v in angažmanPoVrsti" :key="v.vrsta">
            <span class="legend-dot" :style="{ background: v.boja }"></span>{{ v.naziv }}
            <span class="legend-pct">{{ v.postotak }}%</span>
          </div>
        </div>
      </div>
      <p v-else class="chart-empty">Nema definiranih obaveza na upisanim kolegijima.</p>
    </div>

    <CourseProgressModal
      v-if="selectedCourse"
      :course-name="selectedCourse.naziv"
      :subtitle="selectedCourse.nastavnik ? `Nastavnik: ${selectedCourse.nastavnik}` : 'Pregled obaveza i napretka'"
      :bodovi="selectedCourse.bodovi"
      :max-bodovi="selectedCourse.maxBodovi"
      :status-label="statusLabelStudenta(
        selectedCourse.prolazi,
        selectedCourse.aktivnostiKolegija,
        selectedCourse.maxBodovi,
      )"
      :aktivnosti-kolegija="selectedCourse.aktivnostiKolegija"
      @close="selectedCourse = null"
    />
  </main>
</template>

<style scoped>
.chart-empty {
  margin: 0;
  color: var(--muted);
  font-size: 13px;
}
</style>
