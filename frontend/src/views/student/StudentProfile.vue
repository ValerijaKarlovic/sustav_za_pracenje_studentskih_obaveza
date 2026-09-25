<script setup>
import { onMounted, ref } from 'vue'
import api from '@/api/client'
import { useAuthStore } from '@/stores/auth'
import Modal from '@/components/shared/Modal.vue'

const auth = useAuthStore()

const kolegiji = ref([])
const brojIndeksa = ref('-')
const showPasswordModal = ref(false)
const lozinkaForma = ref({ trenutna: '', nova: '', potvrda: '' })
const lozinkaPoruka = ref('')
const lozinkaUspjeh = ref(false)
const spremanje = ref(false)

onMounted(async () => {
  const [profil, kolegijiOdgovor] = await Promise.all([
    api.get('/student/profil'),
    api.get('/student/kolegiji'),
  ])
  brojIndeksa.value = profil.data.brojIndeksa
  kolegiji.value = kolegijiOdgovor.data.map(k => k.kolegij)
})

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

  spremanje.value = true
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
    spremanje.value = false
  }
}
</script>

<template>
  <nav class="tabs">
    <RouterLink to="/student">Dashboard</RouterLink>
    <RouterLink to="/student/aktivnosti">Moje aktivnosti</RouterLink>
    <RouterLink to="/student/profil">Profil</RouterLink>
  </nav>

  <main class="page-content">
    <h2 class="page-title">Moj profil</h2>

    <div class="card">
      <div class="profile-row"><span class="k">Ime i prezime</span><span>{{ auth.user.ime }} {{ auth.user.prezime }}</span></div>
      <div class="profile-row"><span class="k">Email</span><span>{{ auth.user.email }}</span></div>
      <div class="profile-row"><span class="k">Broj indeksa</span><span>{{ brojIndeksa }}</span></div>
      <div class="profile-password-action">
        <button type="button" class="secondary" @click="otvoriPromjenuLozinke">Promijeni lozinku</button>
      </div>
    </div>

    <div class="section-title">Upisani kolegiji</div>
    <div class="card">
      <ul class="course-list">
        <li v-for="k in kolegiji" :key="k">{{ k }}</li>
      </ul>
    </div>

    <Modal v-if="showPasswordModal" title="Promjena lozinke" @close="zatvoriPromjenuLozinke">
      <label for="trenutna-lozinka">Trenutna lozinka</label>
      <input id="trenutna-lozinka" v-model="lozinkaForma.trenutna" type="password" autocomplete="current-password" />

      <label for="nova-lozinka">Nova lozinka</label>
      <input id="nova-lozinka" v-model="lozinkaForma.nova" type="password" autocomplete="new-password" />

      <label for="potvrda-lozinka">Potvrdi novu lozinku</label>
      <input id="potvrda-lozinka" v-model="lozinkaForma.potvrda" type="password" autocomplete="new-password" />

      <p
        v-if="lozinkaPoruka"
        class="password-feedback"
        :class="{ success: lozinkaUspjeh, error: !lozinkaUspjeh }"
      >
        {{ lozinkaPoruka }}
      </p>

      <div class="modal-actions modal-actions-split">
        <button type="button" class="secondary" @click="zatvoriPromjenuLozinke">Odustani</button>
        <button type="button" class="small-primary" :disabled="spremanje" @click="spremiLozinku">Spremi</button>
      </div>
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
