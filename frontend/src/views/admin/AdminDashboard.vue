<script setup>
import { computed, onMounted, ref, watch } from 'vue'
import api from '@/api/client'
import StatCard from '@/components/shared/StatCard.vue'
import LineChart from '@/components/shared/LineChart.vue'
import { formatBodovi } from '@/utils/formatBodovi'

const kolegiji = ref([])
const odabraniKolegij = ref('')
const razdobljeDana = ref('')
const statistika = ref({ studenata: 0, nastavnika: 0, kolegija: 0, aktivnosti: 0, evidencija: 0, aktivnostiOvajMjesec: 0, novihAktivnosti: 0, novihKorisnika: 0 })
const nastavnikKolegija = ref('')
const dashboard = ref({ statistika: {}, najaktivniji: [], usporedba: [], poVrsti: [], bodoviPoKolegiju: [], angažman: [] })
const boje = ['#2f5fd1', '#1a7f37', '#d99a1f', '#c62828', '#7c3aed']

const prikazKolegija = computed(() => Boolean(odabraniKolegij.value))

const najaktivniji = computed(() => dashboard.value.najaktivniji || [])
const usporedba = computed(() => dashboard.value.usporedba || [])

const angažmanGraf = computed(() => {
  const lista = dashboard.value.angažman || []
  return {
    labels: lista.map(x => x.label),
    points: lista.map(x => Number(x.bodovi) || 0),
  }
})

const prosjekBodovaPoKolegiju = computed(() =>
  (dashboard.value.bodoviPoKolegiju || []).map(k => {
    const meta = kolegiji.value.find(c => c.id === k.id)
    const ukupno = Number(k.ukupnoBodova) || 0
    const broj = Number(k.brojStudenata) || 0
    const prosjek = broj > 0 ? Number((ukupno / broj).toFixed(1)) : 0
    const maxBodovi = meta ? Number(meta.ukupnoBodova) : 1
    return {
      id: k.id,
      naziv: k.naziv,
      ects: meta?.ects ?? 0,
      prosjek,
      maxBodovi: maxBodovi > 0 ? maxBodovi : 1,
    }
  }),
)

const vrsteAktivnosti = computed(() => {
  const ukupno = (dashboard.value.poVrsti || []).reduce((sum, item) => sum + item.broj, 0) || 1
  return (dashboard.value.poVrsti || []).map((item, index) => ({
    naziv: item.naziv,
    postotak: Math.round(item.broj / ukupno * 100),
    boja: boje[index % boje.length],
  }))
})

function pieGradient() {
  let acc = 0
  const stops = vrsteAktivnosti.value.map(v => {
    const start = acc
    acc += v.postotak
    return `${v.boja} ${start}% ${acc}%`
  })
  return `conic-gradient(${stops.join(', ')})`
}

async function ucitajKolegije() {
  const { data } = await api.get('/admin/kolegiji')
  kolegiji.value = data
}

async function ucitajPregled() {
  if (!odabraniKolegij.value) {
    const { data } = await api.get('/admin/dashboard')
    statistika.value = data
    return
  }
  const params = new URLSearchParams({ kolegijId: String(odabraniKolegij.value) })
  if (razdobljeDana.value) params.set('dana', razdobljeDana.value)
  const { data } = await api.get(`/admin/dashboard?${params}`)
  nastavnikKolegija.value = data.nastavnik || ''
  dashboard.value = data.dashboard || {}
}

onMounted(async () => {
  await ucitajKolegije()
  await ucitajPregled()
})

watch(odabraniKolegij, () => ucitajPregled())
watch(razdobljeDana, () => {
  if (odabraniKolegij.value) ucitajPregled()
})
</script>

<template>
  <nav class="tabs">
    <RouterLink to="/admin">Dashboard</RouterLink>
    <RouterLink to="/admin/korisnici">Korisnici</RouterLink>
    <RouterLink to="/admin/kolegiji">Kolegiji</RouterLink>
    <RouterLink to="/admin/evidencije">Aktivnosti i evidencije</RouterLink>
  </nav>

  <main class="page-content">
    <h2 class="page-title">Pregled sustava</h2>

    <div class="filters">
      <select v-model="odabraniKolegij" aria-label="Kolegij">
        <option value="">Svi kolegiji (sistem)</option>
        <option v-for="k in kolegiji" :key="k.id" :value="k.id">{{ k.naziv }}</option>
      </select>
      <select v-if="prikazKolegija" v-model="razdobljeDana" aria-label="Razdoblje">
        <option value="">Cijelo razdoblje</option>
        <option value="7">Zadnjih 7 dana</option>
        <option value="30">Zadnjih 30 dana</option>
      </select>
    </div>

    <template v-if="!prikazKolegija">
      <div class="stats-row">
        <StatCard label="Studenata" :value="statistika.studenata" />
        <StatCard label="Nastavnika" :value="statistika.nastavnika" />
        <StatCard label="Kolegija" :value="statistika.kolegija" />
        <StatCard label="Aktivnosti ukupno" :value="statistika.aktivnosti" />
      </div>

      <div class="section-title">Aktivnost sustava ovaj mjesec</div>
      <div class="card">
        <table class="roster">
          <thead><tr><th>Podatak</th><th>Broj</th></tr></thead>
          <tbody>
            <tr><td>Aktivnosti sustava ovaj mjesec</td><td>{{ statistika.aktivnostiOvajMjesec }}</td></tr>
            <tr><td>Novih aktivnosti kreirano</td><td>{{ statistika.novihAktivnosti }}</td></tr>
            <tr><td>Novih korisnika dodano</td><td>{{ statistika.novihKorisnika }}</td></tr>
          </tbody>
        </table>
      </div>
    </template>

    <template v-else>
      <p v-if="nastavnikKolegija" class="page-sub">Nastavnik: {{ nastavnikKolegija }}</p>

      <div class="stats-row">
        <StatCard label="Kolegiji" :value="dashboard.statistika.kolegiji || 0" />
        <StatCard label="Studenata ukupno" :value="dashboard.statistika.studenata || 0" />
        <StatCard label="Aktivnosti ovaj mjesec" :value="dashboard.statistika.aktivnosti || 0" />
      </div>

      <div class="section-title">Najaktivniji studenti</div>
      <div class="card">
        <div class="rank-row" v-for="(s, i) in najaktivniji" :key="s.ime">
          <div class="rank-num">{{ i + 1 }}</div>
          <div class="rank-name">{{ s.ime }}</div>
          <div class="rank-points">{{ formatBodovi(s.bodovi) }}</div>
        </div>
      </div>

      <div class="section-title">Usporedba studenata</div>
      <div class="card">
        <table class="roster">
          <thead><tr><th>Student</th><th>Bodovi</th><th>ECTS</th><th>Broj aktivnosti</th></tr></thead>
          <tbody>
            <tr v-for="s in usporedba" :key="s.ime">
              <td>{{ s.ime }}</td><td>{{ s.bodovi }}</td><td>{{ s.ects }}</td><td>{{ s.aktivnosti }}</td>
            </tr>
          </tbody>
        </table>
      </div>

      <div class="section-title">Prosječni bodovi po kolegiju</div>
      <div class="card">
        <div class="bar-row" v-for="k in prosjekBodovaPoKolegiju" :key="k.id">
          <div class="name">{{ k.naziv }} <span class="ects">({{ k.ects }} ECTS)</span></div>
          <div class="bar-track">
            <div class="bar-fill" :style="{ width: (k.prosjek / k.maxBodovi * 100) + '%' }"></div>
          </div>
          <div class="num">{{ k.prosjek }} / {{ k.maxBodovi }}</div>
        </div>
      </div>

      <div class="section-title">Prosjek bodova po mjesecu (angažman)</div>
      <div class="card">
        <LineChart
          v-if="angažmanGraf.labels.length"
          :points="angažmanGraf.points"
          :labels="angažmanGraf.labels"
        />
        <p v-else class="chart-empty">Nema aktivnosti s datumom u odabranom razdoblju.</p>
      </div>

      <div class="section-title">Raspodjela aktivnosti po vrsti</div>
      <div class="card">
        <div class="pie-wrap">
          <div class="pie-chart" :style="{ background: pieGradient() }"></div>
          <div class="pie-legend">
            <div class="legend-item" v-for="v in vrsteAktivnosti" :key="v.naziv">
              <span class="legend-dot" :style="{ background: v.boja }"></span>{{ v.naziv }}
              <span class="legend-pct">{{ v.postotak }}%</span>
            </div>
          </div>
        </div>
      </div>
    </template>
  </main>
</template>

<style scoped>
.chart-empty {
  margin: 0;
  color: var(--muted);
  font-size: 13px;
}
</style>
