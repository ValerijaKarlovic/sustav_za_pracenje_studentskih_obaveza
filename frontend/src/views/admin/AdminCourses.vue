<script setup>
import { computed, onMounted, ref } from 'vue'
import api from '@/api/client'
import Modal from '@/components/shared/Modal.vue'
import CourseProgressModal from '@/components/shared/CourseProgressModal.vue'
import { statusLabelNastavnika } from '@/utils/aktivnostiPrikaz'

const kolegiji = ref([])
const nastavnici = ref([])
const showFormModal = ref(false)
const editingId = ref(null)
const forma = ref({ sifra: '', naziv: '', ects: 5, ukupnoBodova: 100, pragProlaza: 55, nastavnikId: '' })

const rosterKolegij = ref(null)
const studenti = ref([])
const slobodniStudenti = ref([])
const showEnrollModal = ref(false)
const noviStudentId = ref('')
const selectedStudent = ref(null)

const rosterModalSubtitle = computed(() =>
  rosterKolegij.value ? `Nastavnik: ${rosterKolegij.value.nastavnik}` : '',
)

function statusStudenta(student) {
  const k = rosterKolegij.value
  if (!k) return ''
  return statusLabelNastavnika(
    student.bodovi,
    Number(k.ukupnoBodova),
    Number(k.pragProlaza),
    student.zavrsenoOcjenjivanje,
  )
}

async function ucitaj() {
  const [kolegijiOdgovor, korisniciOdgovor] = await Promise.all([api.get('/admin/kolegiji'), api.get('/admin/korisnici')])
  kolegiji.value = kolegijiOdgovor.data
  nastavnici.value = korisniciOdgovor.data.filter(k => k.uloga === 'nastavnik')
}

async function ucitajRoster() {
  if (!rosterKolegij.value) return
  const { data } = await api.get(`/admin/kolegiji/${rosterKolegij.value.id}/studenti`)
  studenti.value = data.map(s => ({
    id: s.studentId,
    ime: `${s.ime} ${s.prezime}`,
    indeks: s.brojIndeksa,
    bodovi: Number(s.bodovi),
    max: Number(rosterKolegij.value.ukupnoBodova),
    pragProlaza: Number(rosterKolegij.value.pragProlaza),
    zavrsenoOcjenjivanje: Boolean(s.zavrsenoOcjenjivanje),
  }))
}

function otvoriRoster(kolegij) {
  rosterKolegij.value = kolegij
  selectedStudent.value = null
  ucitajRoster()
}

function otvoriDodavanje() {
  editingId.value = null
  forma.value = { sifra: '', naziv: '', ects: 5, ukupnoBodova: 100, pragProlaza: 55, nastavnikId: nastavnici.value[0]?.id || '' }
  showFormModal.value = true
}

function otvoriUredivanje(kolegij) {
  editingId.value = kolegij.id
  forma.value = { sifra: kolegij.sifra, naziv: kolegij.naziv, ects: kolegij.ects, ukupnoBodova: kolegij.ukupnoBodova, pragProlaza: kolegij.pragProlaza, nastavnikId: kolegij.nastavnikId }
  showFormModal.value = true
}

async function spremi() {
  if (editingId.value) await api.put(`/admin/kolegiji/${editingId.value}`, forma.value)
  else await api.post('/admin/kolegiji', forma.value)
  showFormModal.value = false
  await ucitaj()
}

async function obrisi(kolegij) {
  if (!window.confirm(`Jeste li sigurni da želite obrisati kolegij ${kolegij.naziv}?`)) return
  await api.delete(`/admin/kolegiji/${kolegij.id}`)
  if (rosterKolegij.value?.id === kolegij.id) rosterKolegij.value = null
  await ucitaj()
}

async function otvoriUpis() {
  const { data } = await api.get(`/admin/kolegiji/${rosterKolegij.value.id}/slobodni-studenti`)
  slobodniStudenti.value = data
  noviStudentId.value = data[0]?.id || ''
  showEnrollModal.value = true
}

async function upisiStudenta() {
  if (!noviStudentId.value) return
  await api.post('/admin/upisi', { studentId: Number(noviStudentId.value), kolegijId: rosterKolegij.value.id })
  showEnrollModal.value = false
  await ucitajRoster()
  await ucitaj()
}

async function ukloniStudenta(student) {
  if (!window.confirm('Jeste li sigurni da želite ukloniti studenta s ovog kolegija?')) return
  await api.delete(`/admin/upisi/${rosterKolegij.value.id}/${student.id}`)
  if (selectedStudent.value?.studentIme === student.ime) selectedStudent.value = null
  await ucitajRoster()
  await ucitaj()
}

async function otvoriKarton(student) {
  const { data } = await api.get(`/admin/kolegiji/${rosterKolegij.value.id}/studenti/${student.id}`)
  const aktivnostiKolegija = data.aktivnosti ?? []
  const bodovi = Number(data.bodovi ?? student.bodovi)
  const maxBodovi = Number(data.maxBodovi ?? student.max)
  const pragProlaza = Number(data.kolegij?.pragProlaza ?? student.pragProlaza)
  selectedStudent.value = {
    studentIme: `${data.student?.ime ?? ''} ${data.student?.prezime ?? ''}`.trim() || student.ime,
    courseName: data.kolegij?.naziv || rosterKolegij.value.naziv,
    bodovi,
    maxBodovi,
    statusLabel: statusLabelNastavnika(
      bodovi,
      maxBodovi,
      pragProlaza,
      data.zavrsenoOcjenjivanje,
    ),
    aktivnostiKolegija,
  }
}

const kartonSubtitle = computed(() =>
  selectedStudent.value ? `Pregled obaveza i napretka - Student: ${selectedStudent.value.studentIme}` : '',
)

onMounted(ucitaj)
</script>

<template>
  <nav class="tabs"><RouterLink to="/admin">Dashboard</RouterLink><RouterLink to="/admin/korisnici">Korisnici</RouterLink><RouterLink to="/admin/kolegiji">Kolegiji</RouterLink><RouterLink to="/admin/evidencije">Aktivnosti i evidencije</RouterLink></nav>
  <main class="page-content">
    <div class="row-between"><h2 class="page-title" style="margin:0;">Kolegiji</h2><button class="small-primary" @click="otvoriDodavanje">+ Dodaj kolegij</button></div>
    <div class="card"><table class="roster"><thead><tr><th>Naziv</th><th>Nastavnik</th><th>Studenata</th><th></th></tr></thead><tbody>
      <tr v-for="k in kolegiji" :key="k.id" class="roster-row-clickable" @click="otvoriRoster(k)">
        <td><span class="student-name-link">{{ k.naziv }}</span></td><td>{{ k.nastavnik }}</td><td>{{ k.studenata }}</td>
        <td>
          <button @click.stop="otvoriUredivanje(k)">Uredi</button>
          <button @click.stop="obrisi(k)">Obriši</button>
        </td>
      </tr>
    </tbody></table></div>

    <Modal v-if="showFormModal" :title="editingId ? 'Uredi kolegij' : 'Dodaj kolegij'" @close="showFormModal = false">
      <label>Šifra</label><input type="text" v-model="forma.sifra" />
      <label>Naziv</label><input type="text" v-model="forma.naziv" />
      <label>ECTS</label><input type="number" v-model="forma.ects" min="1" />
      <label>Ukupno bodova</label><input type="number" v-model="forma.ukupnoBodova" min="1" />
      <label>Prag prolaza (%)</label><input type="number" v-model="forma.pragProlaza" min="0" max="100" />
      <label>Nastavnik</label><select v-model="forma.nastavnikId" style="width:100%; margin-bottom:16px;"><option v-for="n in nastavnici" :key="n.id" :value="n.id">{{ n.ime }} {{ n.prezime }}</option></select>
      <div class="modal-actions"><button class="small-primary" @click="spremi">Spremi</button></div>
    </Modal>

    <Modal v-if="rosterKolegij" :title="rosterKolegij.naziv" :subtitle="rosterModalSubtitle" @close="rosterKolegij = null">
      <div style="margin-bottom:12px; text-align:right;">
        <button class="small-primary" @click="otvoriUpis">+ Upiši studenta</button>
      </div>
      <table class="roster">
        <thead><tr><th>Student</th><th>Broj indeksa</th><th>Bodovi</th><th></th></tr></thead>
        <tbody>
          <tr v-for="s in studenti" :key="s.id">
            <td><span class="student-name-link" @click="otvoriKarton(s)">{{ s.ime }}</span></td>
            <td>{{ s.indeks }}</td>
            <td>{{ s.bodovi }} / {{ s.max }}</td>
            <td><button @click="ukloniStudenta(s)">Ukloni</button></td>
          </tr>
        </tbody>
      </table>
    </Modal>

    <Modal v-if="showEnrollModal" title="Upiši studenta" @close="showEnrollModal = false">
      <label>Student</label>
      <select v-model="noviStudentId" style="width:100%; margin-bottom:16px;">
        <option v-for="s in slobodniStudenti" :key="s.id" :value="s.id">{{ s.ime }} ({{ s.brojIndeksa }})</option>
      </select>
      <p v-if="!slobodniStudenti.length" class="page-sub">Svi studenti su već upisani na ovaj kolegij.</p>
      <div class="modal-actions"><button class="small-primary" :disabled="!noviStudentId" @click="upisiStudenta">Upiši</button></div>
    </Modal>

    <CourseProgressModal
      v-if="selectedStudent"
      :course-name="selectedStudent.courseName"
      :subtitle="kartonSubtitle"
      :bodovi="selectedStudent.bodovi"
      :max-bodovi="selectedStudent.maxBodovi"
      :status-label="selectedStudent.statusLabel"
      :aktivnosti-kolegija="selectedStudent.aktivnostiKolegija"
      @close="selectedStudent = null"
    />
  </main>
</template>
