<script setup>
import { computed, onMounted, ref } from 'vue'
import api from '@/api/client'
import Modal from '@/components/shared/Modal.vue'

const korisnici = ref([])
const filterUloga = ref('Sve uloge')
const showModal = ref(false)
const editingId = ref(null)
const forma = ref({ ime: '', prezime: '', email: '', uloga: 'student', brojIndeksa: '', lozinka: '' })
const roleBadgeClass = { student: 'role-student', nastavnik: 'role-nastavnik', admin: 'role-admin' }
const roleLabel = { student: 'Student', nastavnik: 'Nastavnik', admin: 'Administrator' }

const filtrirani = computed(() => korisnici.value.filter(k => filterUloga.value === 'Sve uloge' || k.uloga === filterUloga.value))

async function ucitaj() {
  const { data } = await api.get('/admin/korisnici')
  korisnici.value = data
}

function otvoriDodavanje() {
  editingId.value = null
  forma.value = { ime: '', prezime: '', email: '', uloga: 'student', brojIndeksa: '', lozinka: 'test123' }
  showModal.value = true
}

function otvoriUredivanje(korisnik) {
  editingId.value = korisnik.id
  forma.value = {
    ime: korisnik.ime,
    prezime: korisnik.prezime,
    email: korisnik.email,
    uloga: korisnik.uloga,
    brojIndeksa: korisnik.brojIndeksa || '',
    lozinka: '',
  }
  showModal.value = true
}

function payloadZaSpremanje() {
  const osnova = {
    ime: forma.value.ime,
    prezime: forma.value.prezime,
    email: forma.value.email,
    lozinka: forma.value.lozinka || undefined,
  }
  if (forma.value.uloga === 'student') {
    return { ...osnova, brojIndeksa: forma.value.brojIndeksa.trim() }
  }
  return osnova
}

async function spremi() {
  if (!forma.value.ime.trim() || !forma.value.prezime.trim() || !forma.value.email.trim()) return
  if (forma.value.uloga === 'student' && !forma.value.brojIndeksa.trim()) {
    window.alert('Unesite broj indeksa za studenta.')
    return
  }
  try {
    if (editingId.value) {
      const body = payloadZaSpremanje()
      if (!body.lozinka) delete body.lozinka
      await api.put(`/admin/korisnici/${editingId.value}`, body)
    } else {
      await api.post('/admin/korisnici', {
        ...payloadZaSpremanje(),
        uloga: forma.value.uloga,
        lozinka: forma.value.lozinka || 'test123',
      })
    }
    showModal.value = false
    await ucitaj()
  } catch (error) {
    window.alert(error.response?.data?.poruka || 'Spremanje nije uspjelo.')
  }
}

async function obrisi(korisnik) {
  if (!window.confirm(`Jeste li sigurni da želite obrisati korisnika ${korisnik.ime} ${korisnik.prezime}?`)) return
  await api.delete(`/admin/korisnici/${korisnik.id}`)
  await ucitaj()
}

onMounted(ucitaj)
</script>

<template>
  <nav class="tabs"><RouterLink to="/admin">Dashboard</RouterLink><RouterLink to="/admin/korisnici">Korisnici</RouterLink><RouterLink to="/admin/kolegiji">Kolegiji</RouterLink><RouterLink to="/admin/evidencije">Aktivnosti i evidencije</RouterLink></nav>
  <main class="page-content">
    <div class="row-between"><h2 class="page-title" style="margin:0;">Korisnici</h2><button class="small-primary" @click="otvoriDodavanje">+ Dodaj korisnika</button></div>
    <div class="filters"><select v-model="filterUloga"><option>Sve uloge</option><option value="student">Student</option><option value="nastavnik">Nastavnik</option><option value="admin">Administrator</option></select></div>
    <div class="card"><table class="roster"><thead><tr><th>Ime i prezime</th><th>Broj indeksa</th><th>Email</th><th>Uloga</th><th></th></tr></thead><tbody>
      <tr v-for="k in filtrirani" :key="k.id">
        <td>{{ k.ime }} {{ k.prezime }}</td>
        <td>{{ k.uloga === 'student' ? (k.brojIndeksa || '-') : '-' }}</td>
        <td>{{ k.email }}</td>
        <td><span class="badge" :class="roleBadgeClass[k.uloga]">{{ roleLabel[k.uloga] }}</span></td>
        <td><button @click="otvoriUredivanje(k)">Uredi</button><button @click="obrisi(k)">Obriši</button></td>
      </tr>
    </tbody></table></div>
    <Modal v-if="showModal" :title="editingId ? 'Uredi korisnika' : 'Dodaj korisnika'" @close="showModal = false">
      <label>Ime</label><input type="text" v-model="forma.ime" />
      <label>Prezime</label><input type="text" v-model="forma.prezime" />
      <label>Email</label><input type="email" v-model="forma.email" />
      <label>Uloga</label><select v-model="forma.uloga" style="width:100%; margin-bottom:16px;"><option value="student">Student</option><option value="nastavnik">Nastavnik</option><option value="admin">Administrator</option></select>
      <template v-if="forma.uloga === 'student'">
        <label>Broj indeksa</label>
        <input type="text" v-model="forma.brojIndeksa" placeholder="npr. 0123456789" style="margin-bottom:16px;" />
      </template>
      <label>Lozinka {{ editingId ? '(ostavi prazno za nepromijenjenu)' : '' }}</label><input type="text" v-model="forma.lozinka" />
      <div class="modal-actions"><button class="small-primary" @click="spremi">Spremi</button></div>
    </Modal>
  </main>
</template>
