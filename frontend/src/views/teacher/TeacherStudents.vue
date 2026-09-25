<script setup>
import { computed, onMounted, ref } from 'vue'
import api from '@/api/client'
import Modal from '@/components/shared/Modal.vue'
import CourseProgressModal from '@/components/shared/CourseProgressModal.vue'
import { statusLabelNastavnika } from '@/utils/aktivnostiPrikaz'

const kolegiji = ref([])
const odabraniKolegij = ref(null)
const studenti = ref([])
const statusFilter = ref('Svi studenti')
const sortKey = ref('ime')
const sortAsc = ref(true)
const selected = ref(null)
const slobodniStudenti = ref([])
const noviStudentId = ref('')
const showEnrollModal = ref(false)

const odabraniKolegijNaziv = computed(() => kolegiji.value.find(k => k.id === odabraniKolegij.value)?.naziv || '')

function statusStudenta(student) {
  return statusLabelNastavnika(student.bodovi, student.max, student.pragProlaza, student.zavrsenoOcjenjivanje)
}

const prikazaniStudenti = computed(() => {
  let rezultat = studenti.value
  if (statusFilter.value === 'Položeno') rezultat = rezultat.filter(s => statusStudenta(s) === 'Položeno')
  if (statusFilter.value === 'Nije položeno') rezultat = rezultat.filter(s => statusStudenta(s) === 'Nije položeno')
  if (statusFilter.value === 'U tijeku') rezultat = rezultat.filter(s => statusStudenta(s) === 'U tijeku')
  return [...rezultat].sort((a, b) => {
    const prvi = sortKey.value === 'bodovi' ? a.bodovi : sortKey.value === 'indeks' ? a.indeks : a.ime
    const drugi = sortKey.value === 'bodovi' ? b.bodovi : sortKey.value === 'indeks' ? b.indeks : b.ime
    const usporedba = typeof prvi === 'number' ? prvi - drugi : String(prvi).localeCompare(String(drugi), 'hr')
    return sortAsc.value ? usporedba : -usporedba
  })
})

const modalSubtitle = computed(() =>
  selected.value
    ? `Pregled obaveza i napretka - Student: ${selected.value.studentIme}`
    : '',
)

function sortiraj(key) {
  if (sortKey.value === key) sortAsc.value = !sortAsc.value
  else { sortKey.value = key; sortAsc.value = true }
}

async function ucitajStudente() {
  if (!odabraniKolegij.value) return
  const { data } = await api.get(`/nastavnik/studenti?kolegijId=${odabraniKolegij.value}`)
  const kolegij = kolegiji.value.find(k => k.id === odabraniKolegij.value)
  studenti.value = data.map(s => ({
    id: s.studentId,
    ime: `${s.ime} ${s.prezime}`,
    indeks: s.brojIndeksa,
    bodovi: Number(s.bodovi),
    max: Number(kolegij?.ukupnoBodova || 0),
    pragProlaza: Number(kolegij?.pragProlaza || 55),
    zavrsenoOcjenjivanje: Boolean(s.zavrsenoOcjenjivanje),
  }))
}

async function ucitajKolegije() {
  const { data } = await api.get('/nastavnik/kolegiji')
  kolegiji.value = data
  odabraniKolegij.value = data[0]?.id || null
  await ucitajStudente()
}

async function otvoriKarton(student) {
  if (!odabraniKolegij.value) return
  const { data } = await api.get(`/nastavnik/studenti/${student.id}?kolegijId=${odabraniKolegij.value}`)
  const aktivnostiKolegija = data.aktivnosti ?? []
  const bodovi = Number(data.bodovi ?? student.bodovi)
  const maxBodovi = Number(data.maxBodovi ?? student.max)
  const pragProlaza = Number(data.kolegij?.pragProlaza ?? student.pragProlaza)

  selected.value = {
    studentIme: `${data.student?.ime ?? ''} ${data.student?.prezime ?? ''}`.trim() || student.ime,
    courseName: data.kolegij?.naziv || odabraniKolegijNaziv.value,
    bodovi,
    maxBodovi,
    pragProlaza,
    statusLabel: statusLabelNastavnika(
      bodovi,
      maxBodovi,
      pragProlaza,
      data.zavrsenoOcjenjivanje,
    ),
    aktivnostiKolegija,
  }
}

async function otvoriUpis() {
  const { data } = await api.get(`/nastavnik/slobodni-studenti?kolegijId=${odabraniKolegij.value}`)
  slobodniStudenti.value = data
  noviStudentId.value = data[0]?.id || ''
  showEnrollModal.value = true
}

async function upisiStudenta() {
  if (!noviStudentId.value) return
  await api.post('/nastavnik/upisi', { studentId: Number(noviStudentId.value), kolegijId: odabraniKolegij.value })
  showEnrollModal.value = false
  await ucitajStudente()
}

async function ukloniStudenta(student) {
  if (!window.confirm('Jeste li sigurni da želite ukloniti studenta s ovog kolegija?')) return
  await api.delete(`/nastavnik/upisi/${odabraniKolegij.value}/${student.id}`)
  if (selected.value?.studentIme === student.ime) selected.value = null
  await ucitajStudente()
}

onMounted(ucitajKolegije)
</script>

<template>
  <nav class="tabs">
    <RouterLink to="/nastavnik">Dashboard</RouterLink>
    <RouterLink to="/nastavnik/aktivnosti">Aktivnosti</RouterLink>
    <RouterLink to="/nastavnik/studenti">Studenti</RouterLink>
    <RouterLink to="/nastavnik/profil">Profil</RouterLink>
  </nav>

  <main class="page-content">
    <div class="row-between">
      <h2 class="page-title" style="margin:0;">Studenti</h2>
      <button class="small-primary" @click="otvoriUpis">+ Upiši studenta</button>
    </div>

    <div class="filters">
      <select v-model="odabraniKolegij" @change="ucitajStudente" aria-label="Odaberi kolegij">
        <option v-for="k in kolegiji" :key="k.id" :value="k.id">{{ k.naziv }}</option>
      </select>
      <select v-model="statusFilter" aria-label="Filtriraj po statusu">
        <option>Svi studenti</option><option>Položeno</option><option>Nije položeno</option><option>U tijeku</option>
      </select>
    </div>

    <div class="card">
      <table class="roster">
        <thead><tr>
          <th><button class="table-sort" @click="sortiraj('ime')">Student</button></th>
          <th><button class="table-sort" @click="sortiraj('indeks')">Broj indeksa</button></th>
          <th><button class="table-sort" @click="sortiraj('bodovi')">Bodovi</button></th>
          <th>Status</th><th></th>
        </tr></thead>
        <tbody>
          <tr
            v-for="s in prikazaniStudenti"
            :key="s.id"
            class="roster-row-clickable"
            @click="otvoriKarton(s)"
          >
            <td><span class="student-name-link">{{ s.ime }}</span></td>
            <td>{{ s.indeks }}</td><td>{{ s.bodovi }} / {{ s.max }}</td>
            <td><span class="badge" :class="statusStudenta(s) === 'Položeno' ? 'done' : statusStudenta(s) === 'U tijeku' ? 'pending' : 'missed'">{{ statusStudenta(s) }}</span></td>
            <td><button @click.stop="ukloniStudenta(s)">Ukloni</button></td>
          </tr>
        </tbody>
      </table>
    </div>

    <Modal v-if="showEnrollModal" title="Upiši studenta" @close="showEnrollModal = false">
      <label>Student</label>
      <select v-model="noviStudentId" style="width:100%; margin-bottom:16px;">
        <option v-for="s in slobodniStudenti" :key="s.id" :value="s.id">{{ s.ime }} ({{ s.brojIndeksa }})</option>
      </select>
      <p v-if="!slobodniStudenti.length" class="page-sub">Svi studenti su već upisani na ovaj kolegij.</p>
      <div class="modal-actions"><button class="small-primary" :disabled="!noviStudentId" @click="upisiStudenta">Upiši</button></div>
    </Modal>

    <CourseProgressModal
      v-if="selected"
      :course-name="selected.courseName"
      :subtitle="modalSubtitle"
      :bodovi="selected.bodovi"
      :max-bodovi="selected.maxBodovi"
      :status-label="selected.statusLabel"
      :aktivnosti-kolegija="selected.aktivnostiKolegija"
      @close="selected = null"
    />
  </main>
</template>
