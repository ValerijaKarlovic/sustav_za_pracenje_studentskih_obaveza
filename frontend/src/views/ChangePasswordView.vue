<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import api from '@/api/client'
import { useAuthStore } from '@/stores/auth'

const auth = useAuthStore()
const router = useRouter()
const forma = ref({ trenutna: '', nova: '', potvrda: '' })
const poruka = ref('')
const greska = ref('')
const spremanje = ref(false)

async function spremi() {
  greska.value = ''
  poruka.value = ''
  if (forma.value.nova !== forma.value.potvrda) {
    greska.value = 'Nova lozinka i potvrda se ne podudaraju.'
    return
  }
  if (forma.value.trenutna === forma.value.nova) {
    greska.value = 'Nova lozinka mora biti različita od trenutne.'
    return
  }
  spremanje.value = true
  try {
    const { data } = await api.post('/auth/promijeni-lozinku', {
      trenutnaLozinka: forma.value.trenutna,
      novaLozinka: forma.value.nova,
    })
    auth.oznaciLozinkuPromijenjenu()
    poruka.value = data.poruka || 'Lozinka je promijenjena.'
    setTimeout(() => router.push(`/${auth.user.role}`), 800)
  } catch (error) {
    greska.value = error.response?.data?.poruka || 'Promjena lozinke nije uspjela.'
  } finally {
    spremanje.value = false
  }
}
</script>

<template>
  <main class="page-content" style="max-width: 420px; margin: 48px auto;">
    <h2 class="page-title">Obavezna promjena lozinke</h2>
    <p class="sub" style="margin-bottom: 20px;">
      Prije korištenja sustava morate postaviti novu lozinku (najmanje 6 znakova).
    </p>
    <div class="card">
      <label>Trenutna lozinka</label>
      <input v-model="forma.trenutna" type="password" autocomplete="current-password" />
      <label>Nova lozinka</label>
      <input v-model="forma.nova" type="password" autocomplete="new-password" />
      <label>Potvrda nove lozinke</label>
      <input v-model="forma.potvrda" type="password" autocomplete="new-password" />
      <p v-if="greska" style="color: #c62828; font-size: 13px;">{{ greska }}</p>
      <p v-if="poruka" style="color: var(--green); font-size: 13px;">{{ poruka }}</p>
      <button class="primary" style="margin-top: 12px;" :disabled="spremanje" @click="spremi">
        Spremi novu lozinku
      </button>
    </div>
  </main>
</template>
