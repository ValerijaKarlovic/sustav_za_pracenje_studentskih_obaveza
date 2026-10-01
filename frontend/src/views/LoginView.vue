<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const email = ref('')
const password = ref('')
const prikaziLozinku = ref(false)
const error = ref('')
const auth = useAuthStore()
const router = useRouter()

async function handleLogin() {
  error.value = ''
  try {
    const user = await auth.login(email.value, password.value)
    if (user.moraPromijenitiLozinku) {
      router.push('/promjena-lozinke')
    } else {
      router.push(`/${user.role}`)
    }
  } catch (e) {
    error.value = e.message.includes('Network Error')
      ? 'Backend nije pokrenut. Pokrenite: dotnet run --project backend'
      : e.message
  }
}
</script>

<template>
  <div class="login-screen">
    <div class="login-box">
      <h1>Prijava u sustav</h1>
      <p class="sub">Praćenje studentskih obaveza</p>

      <label for="email">Email</label>
      <input id="email" type="email" v-model="email" />

      <label for="pass">Lozinka</label>
      <div class="password-field">
        <input id="pass" :type="prikaziLozinku ? 'text' : 'password'" v-model="password" />
        <button
          type="button"
          class="password-toggle"
          :aria-label="prikaziLozinku ? 'Sakrij lozinku' : 'Prikaži lozinku'"
          :aria-pressed="prikaziLozinku"
          @click="prikaziLozinku = !prikaziLozinku"
        >
          <svg v-if="prikaziLozinku" class="password-toggle-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.75" aria-hidden="true">
            <path stroke-linecap="round" stroke-linejoin="round" d="M3.98 8.223A10.477 10.477 0 0 0 1.934 12c1.292 4.338 5.31 7.5 10.066 7.5 1.01 0 1.98-.144 2.888-.41M9.88 9.88a3 3 0 1 0 4.24 4.24" />
            <path stroke-linecap="round" stroke-linejoin="round" d="M6.228 6.228 3 3m3.228 3.228 3.65 3.65m7.894 7.894L21 21m-3.228-3.228-3.65-3.65" />
          </svg>
          <svg v-else class="password-toggle-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.75" aria-hidden="true">
            <path stroke-linecap="round" stroke-linejoin="round" d="M2.036 12.322a1.012 1.012 0 0 1 0-.639C3.423 7.51 7.36 4.5 12 4.5c4.638 0 8.573 3.007 9.963 7.178.07.207.07.431 0 .639C20.577 16.49 16.64 19.5 12 19.5c-4.638 0-8.573-3.007-9.963-7.178z" />
            <path stroke-linecap="round" stroke-linejoin="round" d="M15 12a3 3 0 1 1-6 0 3 3 0 0 1 6 0z" />
          </svg>
        </button>
      </div>

      <p v-if="error" style="color:#c62828; font-size:13px; margin:-8px 0 12px;">{{ error }}</p>

      <button class="primary" @click="handleLogin">Prijavi se</button>
      <p class="login-note">Pristupne podatke dodjeljuje administrator.</p>
    </div>
  </div>
</template>
