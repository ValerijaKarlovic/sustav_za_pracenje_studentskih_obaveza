<script setup>
import { computed, onMounted, ref } from 'vue'
import api from '@/api/client'
import Modal from '@/components/shared/Modal.vue'
import ActivitySortSelect from '@/components/shared/ActivitySortSelect.vue'
import { sortirajAktivnostiPoDatumu } from '@/utils/sortiranjeAktivnosti'
import { metaLinijaAktivnosti, naslovModalaBodova } from '@/utils/aktivnostiMeta'

const aktivnosti = ref([])
const kolegiji = ref([])
const vrste = ref([])
const filterKolegij = ref('')
const filterVrsta = ref('')
const poredajPo = ref('datum-asc')
const showActivityModal = ref(false)
const showPointsModal = ref(false)
const editingId = ref(null)
const activeActivity = ref(null)
const studenti = ref([])
const novaVrstaCustom = ref('')
const forma = ref({ kolegijId: '', vrstaId: '', naziv: '', opis: '', datum: '', maxBodovi: 0 })

const filtrirane = computed(() => {
  const lista = aktivnosti.value.filter(a =>
    (!filterKolegij.value || String(a.kolegijId) === String(filterKolegij.value)) &&
    (!filterVrsta.value || String(a.vrstaId) === String(filterVrsta.value)),
  )
  return sortirajAktivnostiPoDatumu(lista, poredajPo.value)
})

const metaAktivnostiZaBodove = computed(() => metaLinijaAktivnosti(activeActivity.value))
const naslovBodovaModala = computed(() => naslovModalaBodova(activeActivity.value))

async function ucitaj() {
  const [aktivnostiOdgovor, kolegijiOdgovor, vrsteOdgovor] = await Promise.all([
    api.get('/nastavnik/aktivnosti'),
    api.get('/nastavnik/kolegiji'),
    api.get('/nastavnik/vrste-aktivnosti'),
  ])
  aktivnosti.value = aktivnostiOdgovor.data.map(a => ({
    ...a,
    datumRaw: a.datum ?? null,
  }))
  kolegiji.value = kolegijiOdgovor.data
  vrste.value = vrsteOdgovor.data
}

function otvoriDodavanje() {
  editingId.value = null
  forma.value = { kolegijId: kolegiji.value[0]?.id || '', vrstaId: vrste.value[0]?.id || '', naziv: '', opis: '', datum: '', maxBodovi: 0 }
  novaVrstaCustom.value = ''
  showActivityModal.value = true
}

function otvoriUredivanje(aktivnost) {
  editingId.value = aktivnost.id
  forma.value = {
    kolegijId: aktivnost.kolegijId,
    vrstaId: aktivnost.vrstaId,
    naziv: aktivnost.naziv,
    opis: aktivnost.opis || '',
    datum: aktivnost.datum ? aktivnost.datum.substring(0, 10) : '',
    maxBodovi: aktivnost.bodovi,
  }
  novaVrstaCustom.value = ''
  showActivityModal.value = true
}

async function spremiAktivnost() {
  if (!forma.value.naziv.trim()) return
  let vrstaId = forma.value.vrstaId
  if (vrstaId === 'ostalo') {
    if (!novaVrstaCustom.value.trim()) return
    const { data } = await api.post('/nastavnik/vrste-aktivnosti', { naziv: novaVrstaCustom.value })
    vrstaId = data.id
  }
  const zahtjev = {
    ...forma.value,
    vrstaId,
    opis: forma.value.opis?.trim() || null,
  }
  if (editingId.value) await api.put(`/nastavnik/aktivnosti/${editingId.value}`, zahtjev)
  else await api.post('/nastavnik/aktivnosti', zahtjev)
  showActivityModal.value = false
  await ucitaj()
}

async function obrisiAktivnost(aktivnost) {
  if (!window.confirm('Jeste li sigurni da želite izbrisati ovu aktivnost?')) return
  await api.delete(`/nastavnik/aktivnosti/${aktivnost.id}`)
  await ucitaj()
}

async function otvoriBodove(aktivnost) {
  activeActivity.value = aktivnost
  const { data } = await api.get(`/nastavnik/aktivnosti/${aktivnost.id}/studenti`)
  studenti.value = data
  showPointsModal.value = true
}

async function spremiBodove() {
  await Promise.all(studenti.value.map(student => api.put(
    `/nastavnik/evidencije/${activeActivity.value.id}/${student.studentId}`,
    { status: student.status, bodovi: student.bodovi === '' ? null : Number(student.bodovi) },
  )))
  showPointsModal.value = false
  await ucitaj()
}

onMounted(ucitaj)
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
      <h2 class="page-title" style="margin:0;">Aktivnosti</h2>
      <button class="small-primary" @click="otvoriDodavanje">+ Dodaj aktivnost</button>
    </div>

    <div class="filters">
      <select v-model="filterKolegij">
        <option value="">Svi kolegiji</option>
        <option v-for="k in kolegiji" :key="k.id" :value="k.id">{{ k.naziv }}</option>
      </select>
      <select v-model="filterVrsta">
        <option value="">Sve vrste</option>
        <option v-for="v in vrste" :key="v.id" :value="v.id">{{ v.naziv }}</option>
      </select>
      <ActivitySortSelect v-model="poredajPo" />
    </div>

    <div class="activity-item" v-for="a in filtrirane" :key="a.id">
      <div>
        <div class="aname">{{ a.naziv }}</div>
        <div class="meta">{{ metaLinijaAktivnosti(a) }}</div>
        <p v-if="a.opis?.trim()" class="activity-opis">{{ a.opis.trim() }}</p>
      </div>
      <div class="actions">
        <button v-if="!a.imaUneseneBodove" type="button" class="enter-points" @click="otvoriBodove(a)">Unesi bodove</button>
        <button v-else type="button" class="view-points-link" @click="otvoriBodove(a)">Pogledaj bodove</button>
        <button @click="otvoriUredivanje(a)">Uredi</button>
        <button @click="obrisiAktivnost(a)">Obriši</button>
      </div>
    </div>

    <Modal v-if="showActivityModal" :title="editingId ? 'Uredi aktivnost' : 'Dodaj aktivnost'" @close="showActivityModal = false">
      <label>Naziv aktivnosti</label>
      <input type="text" v-model="forma.naziv" placeholder="npr. Lab vježba 4" />
      <label>Kolegij</label>
      <select v-model="forma.kolegijId" style="width:100%; margin-bottom:16px;">
        <option v-for="k in kolegiji" :key="k.id" :value="k.id">{{ k.naziv }}</option>
      </select>
      <label>Vrsta aktivnosti</label>
      <select v-model="forma.vrstaId" style="width:100%; margin-bottom:16px;">
        <option v-for="v in vrste" :key="v.id" :value="v.id">{{ v.naziv }}</option>
        <option value="ostalo">Ostalo</option>
      </select>
      <div v-if="forma.vrstaId === 'ostalo'">
        <label>Naziv nove vrste aktivnosti</label>
        <input type="text" v-model="novaVrstaCustom" placeholder="npr. Radionica" />
      </div>
      <label>Datum</label>
      <input type="date" v-model="forma.datum" />
      <label>Opis (opcionalno)</label>
      <textarea v-model="forma.opis" rows="3" placeholder="Kratki opis aktivnosti" style="width:100%; margin-bottom:12px; resize:vertical;" />
      <label>Bodovi</label>
      <input type="number" v-model="forma.maxBodovi" min="0" />
      <div class="modal-actions"><button class="small-primary" @click="spremiAktivnost">Spremi</button></div>
    </Modal>

    <Modal
      v-if="showPointsModal"
      :title="naslovBodovaModala"
      :subtitle="metaAktivnostiZaBodove"
      @close="showPointsModal = false"
    >
      <p v-if="activeActivity?.opis?.trim()" class="activity-opis points-modal-opis">{{ activeActivity.opis.trim() }}</p>
      <table class="roster">
        <thead><tr><th>Student</th><th>Status</th><th>Bodovi</th></tr></thead>
        <tbody>
          <tr v-for="s in studenti" :key="s.studentId">
            <td>{{ s.ime }}</td>
            <td><select class="status-select" v-model="s.status">
              <option value="odradeno">Odrađeno</option><option value="ceka_se">Čeka se</option><option value="nije_odradeno">Nije odrađeno</option>
            </select></td>
            <td><input class="points-input" type="number" v-model="s.bodovi" min="0" /></td>
          </tr>
        </tbody>
      </table>
      <div class="modal-actions"><button class="small-primary" @click="spremiBodove">Spremi sve</button></div>
    </Modal>
  </main>
</template>
