import { defineStore } from 'pinia'
import api from '@/api/client'

const roleLabels = {
  student: 'Student',
  nastavnik: 'Nastavnik',
  admin: 'Administrator',
}

export const useAuthStore = defineStore('auth', {
  state: () => ({
    user: JSON.parse(localStorage.getItem('user')) || null,
  }),
  getters: {
    roleLabel: (state) => (state.user ? roleLabels[state.user.role] : ''),
    isAuthenticated: (state) => !!state.user && !!localStorage.getItem('token'),
  },
  actions: {
    async login(email, password) {
      try {
        const { data } = await api.post('/auth/login', {
          email: email.trim().toLowerCase(),
          lozinka: password.trim(),
        })
        this.user = {
          id: data.id,
          ime: data.ime,
          prezime: data.prezime,
          email: data.email,
          role: data.uloga,
          moraPromijenitiLozinku: data.moraPromijenitiLozinku,
        }
        localStorage.setItem('token', data.token)
      } catch (error) {
        throw new Error(error.response?.data?.poruka || 'Neispravan email ili lozinka.')
      }
      localStorage.setItem('user', JSON.stringify(this.user))
      return this.user
    },
    logout() {
      this.user = null
      localStorage.removeItem('user')
      localStorage.removeItem('token')
    },
  },
})
