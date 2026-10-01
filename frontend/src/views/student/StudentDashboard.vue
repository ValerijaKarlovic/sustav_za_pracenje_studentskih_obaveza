<script setup>
import { computed, onMounted, onUnmounted, ref, watch } from 'vue'
import { onBeforeRouteUpdate } from 'vue-router'
import api from '@/api/client'
import { useAuthStore } from '@/stores/auth'
import StatCard from '@/components/shared/StatCard.vue'
import LineChart from '@/components/shared/LineChart.vue'
import CourseProgressModal from '@/components/shared/CourseProgressModal.vue'
import {
  donutGradient,
  segmentiDonutaPoVrstama,
  sortirajVrste,
} from '@/utils/bodoviPoVrsti'
import { mapObvezeZaPrikaz } from '@/utils/aktivnostiPrikaz'

const auth = useAuthStore()
const kolegiji = ref([])
const aktivnosti = ref([])
const napredak = ref([])
const kolegijZaGraf = ref('')

const pozdrav = computed(() => {
  const ime = auth.user?.ime || ''
  return ime ? `Dobrodošli, ${ime}` : 'Dashboard'
})

const napredakFiltriran = computed(() => {
  if (!kolegijZaGraf.value) return []
  const kid = Number(kolegijZaGraf.value)
  return napredak.value.filter(x => x.kolegijId === kid)
})

const grafBodovi = computed(() =>
  napredakFiltriran.value.map(x => Number(x.kumulativniBodovi)),
)

const grafLabele = computed(() =>
  napredakFiltriran.value.map(x =>
    x.datum ? new Date(x.datum).toLocaleDateString('hr-HR', { day: 'numeric', month: 'short' }) : '',
  ),
)

const pragProlazaZaGraf = computed(() => {
  const k = kolegiji.value.find(x => String(x.kolegijId) === String(kolegijZaGraf.value))
  if (!k) return null
  return Math.round(Number(k.maxBodovi) * (k.pragProlaza / 100))
})

const ukupnoBodova = computed(() => kolegiji.value.reduce((s, k) => s + Number(k.bodovi), 0))
const ukupnoMax = computed(() => kolegiji.value.reduce((s, k) => s + Number(k.maxBodovi), 0))
function aktivnostiZaKolegij(nazivKolegija) {
  return aktivnosti.value.filter(a => a.kolegij === nazivKolegija)
}

const ukupnoEcts = computed(() =>
  kolegiji.value.reduce((s, k) => s + Number(k.ectsOstvareno ?? 0), 0).toFixed(1),
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
    naziv: k.naziv ?? k.kolegij,
    ects: k.ects,
    ectsOstvareno: Number(k.ectsOstvareno ?? 0),
    bodovi: Number(k.bodovi),
    maxBodovi: Number(k.ukupnoBodova),
    pragProlaza: Number(k.pragProlaza ?? 60),
    polozen: Boolean(k.polozen),
    statusPrikaz: k.statusPrikaz ?? 'U tijeku',
    obveze: [],
  }))
}

async function ucitajStatistiku() {
  const { data } = await api.get('/student/statistika')
  napredak.value = data
}

async function ucitajDashboard() {
  await Promise.all([ucitajKolegije(), ucitajAktivnosti(), ucitajStatistiku()])
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
    statusPrikaz: data.statusPrikaz ?? osvjezeni.statusPrikaz ?? 'U tijeku',
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

watch(kolegiji, (list) => {
  if (list.length && !kolegijZaGraf.value) {
    kolegijZaGraf.value = String(list[0].kolegijId)
  }
}, { immediate: true })

watch(aktivnosti, () => {
  if (!selectedCourse.value) return
  const k = kolegiji.value.find(x => x.kolegijId === selectedCourse.value.kolegijId)
  const zaKolegij = aktivnosti.value.filter(a => a.kolegij === selectedCourse.value.naziv)
  selectedCourse.value = {
    ...selectedCourse.value,
    ...(k ? { bodovi: k.bodovi, maxBodovi: k.maxBodovi, statusPrikaz: k.statusPrikaz } : {}),
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
    <h2 class="page-title">{{ pozdrav }}</h2>

    <div class="stats-row">
      <StatCard label="Ukupno bodova">{{ ukupnoBodova }} <span class="value-of">/ {{ ukupnoMax }}</span></StatCard>
      <StatCard label="Ukupno ECTS">{{ ukupnoEcts }} <span class="value-of">/ {{ ukupnoEctsMax }}</span></StatCard>
      <StatCard label="Odrađeno ovaj mjesec" :value="odradjenoOvajMjesec" />
    </div>

    <div class="section-title row-between" style="display: flex; align-items: center; gap: 12px; flex-wrap: wrap;">
      <span>Napredak bodova kroz vrijeme</span>
      <select v-model="kolegijZaGraf" style="max-width: 240px;">
        <option v-for="k in kolegiji" :key="k.kolegijId" :value="k.kolegijId">{{ k.naziv }}</option>
      </select>
    </div>
    <div class="card">
      <p v-if="pragProlazaZaGraf != null" class="chart-hint">
        Prag prolaza: {{ pragProlazaZaGraf }} bodova
      </p>
      <LineChart v-if="grafBodovi.length" :points="grafBodovi" :labels="grafLabele" />
      <p v-else class="chart-empty">Nema ocijenjenih aktivnosti za prikaz napretka.</p>
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
      :status-label="selectedCourse.statusPrikaz"
      :aktivnosti-kolegija="selectedCourse.aktivnostiKolegija"
      @close="selectedCourse = null"
    />
  </main>
</template>

<style scoped>
.chart-empty,
.chart-hint {
  margin: 0 0 8px;
  color: var(--muted);
  font-size: 13px;
}
</style>
