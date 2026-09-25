<script setup>
import { onMounted, ref } from 'vue'
import api from '@/api/client'
import Modal from '@/components/shared/Modal.vue'
import { useAuthStore } from '@/stores/auth'

const auth = useAuthStore()
const kolegiji = ref([])
const showModal = ref(false)
const active = ref(null)
const greska = ref('')
const showPasswordModal = ref(false)
const lozinkaForma = ref({ trenutna: '', nova: '', potvrda: '' })
const lozinkaPoruka = ref('')
const lozinkaUspjeh = ref(false)
const spremanjeLozinke = ref(false)

async function ucitaj() {
  const { data } = await api.get('/nastavnik/kolegiji')
  kolegiji.value = data.map(k => ({ ...k, bodovi: Number(k.ukupnoBodova), prag: k.pragProlaza }))
}

function otvoriPostavke(kolegij) {
  active.value = { ...kolegij }
  greska.value = ''
  showModal.value = true
}

function otvoriPromjenuLozinke() {
  lozinkaForma.value = { trenutna: '', nova: '', potvrda: '' }
  lozinkaPoruka.value = ''
  lozinkaUspjeh.value = false
  showPasswordModal.value = true
}

function zatvoriPromjenuLozinke() {
  showPasswordModal.value = false
}

async function spremiLozinku() {
  lozinkaPoruka.value = ''
  lozinkaUspjeh.value = false

  if (lozinkaForma.value.nova !== lozinkaForma.value.potvrda) {
    lozinkaPoruka.value = 'Nova lozinka i potvrda se ne podudaraju.'
    return
  }

  spremanjeLozinke.value = true
  try {
    const { data } = await api.post('/auth/promijeni-lozinku', {
      trenutnaLozinka: lozinkaForma.value.trenutna,
      novaLozinka: lozinkaForma.value.nova,
    })
    lozinkaPoruka.value = data.poruka || 'Lozinka je uspješno promijenjena.'
    lozinkaUspjeh.value = true
    setTimeout(() => zatvoriPromjenuLozinke(), 1200)
  } catch (error) {
    lozinkaPoruka.value = error.response?.data?.poruka || 'Promjena lozinke nije uspjela.'
  } finally {
    spremanjeLozinke.value = false
  }
}

async function spremiPostavke() {
  try {
    await api.put(`/nastavnik/kolegiji/${active.value.id}/postavke`, {
      ukupnoBodova: Number(active.value.bodovi),
      pragProlaza: Number(active.value.prag),
    })
    showModal.value = false
    await ucitaj()
  } catch (error) {
    greska.value = error.response?.data?.poruka || 'Postavke nije moguće spremiti.'
  }
}

onMounted(ucitaj)
</script>

<template>
  <nav class="tabs"><RouterLink to="/nastavnik">Dashboard</RouterLink><RouterLink to="/nastavnik/aktivnosti">Aktivnosti</RouterLink><RouterLink to="/nastavnik/studenti">Studenti</RouterLink><RouterLink to="/nastavnik/profil">Profil</RouterLink></nav>
  <main class="page-content">
    <h2 class="page-title">Moj profil</h2>
    <div class="card">
      <div class="profile-row"><span class="k">Ime i prezime</span><span>{{ auth.user.ime }} {{ auth.user.prezime }}</span></div>
      <div class="profile-row"><span class="k">Email</span><span>{{ auth.user.email }}</span></div>
      <div class="profile-password-action">
        <button type="button" class="secondary" @click="otvoriPromjenuLozinke">Promijeni lozinku</button>
      </div>
    </div>
    <div class="section-title">Kolegiji koje predajem</div>
    <div class="card"><table class="roster"><thead><tr><th>Kolegij</th><th>Ukupno bodova</th><th>Prag za prolaz</th><th></th></tr></thead><tbody>
      <tr v-for="k in kolegiji" :key="k.id"><td>{{ k.naziv }}</td><td>{{ k.bodovi }}</td><td>{{ k.prag }}%</td><td><button @click="otvoriPostavke(k)">Uredi postavke</button></td></tr>
    </tbody></table></div>
    <Modal v-if="showPasswordModal" title="Promjena lozinke" @close="zatvoriPromjenuLozinke">
      <label for="trenutna-lozinka-nastavnik">Trenutna lozinka</label>
      <input id="trenutna-lozinka-nastavnik" v-model="lozinkaForma.trenutna" type="password" autocomplete="current-password" />

      <label for="nova-lozinka-nastavnik">Nova lozinka</label>
      <input id="nova-lozinka-nastavnik" v-model="lozinkaForma.nova" type="password" autocomplete="new-password" />

      <label for="potvrda-lozinka-nastavnik">Potvrdi novu lozinku</label>
      <input id="potvrda-lozinka-nastavnik" v-model="lozinkaForma.potvrda" type="password" autocomplete="new-password" />

      <p
        v-if="lozinkaPoruka"
        class="password-feedback"
        :class="{ success: lozinkaUspjeh, error: !lozinkaUspjeh }"
      >
        {{ lozinkaPoruka }}
      </p>

      <div class="modal-actions modal-actions-split">
        <button type="button" class="secondary" @click="zatvoriPromjenuLozinke">Odustani</button>
        <button type="button" class="small-primary" :disabled="spremanjeLozinke" @click="spremiLozinku">Spremi</button>
      </div>
    </Modal>

    <Modal v-if="showModal" :title="`Postavke - ${active.naziv}`" subtitle="Ukupni bodovi i prag za prolaz" @close="showModal = false">
      <label>Ukupno bodova kolegija</label><input type="number" v-model="active.bodovi" min="1" />
      <label>Prag za prolaz (%)</label><input type="number" v-model="active.prag" min="0" max="100" />
      <p v-if="greska" style="color:var(--red); font-size:13px;">{{ greska }}</p>
      <div class="modal-actions"><button class="small-primary" @click="spremiPostavke">Spremi</button></div>
    </Modal>
  </main>
</template>

<style scoped>
.profile-password-action {
  margin-top: 18px;
  padding-top: 4px;
}
.password-feedback {
  font-size: 13px;
  margin: 12px 0 0;
}
.password-feedback.success {
  color: var(--green);
}
.password-feedback.error {
  color: var(--red);
}
.modal-actions-split {
  display: flex;
  justify-content: flex-end;
  gap: 8px;
}
</style>
