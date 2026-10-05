<script setup>
import { computed } from 'vue'
import { useRoute, RouterView } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const auth = useAuthStore()
const route = useRoute()

const showTopbar = computed(() =>
  auth.isAuthenticated && route.name !== 'login' && route.name !== 'promjena-lozinke',
)
</script>

<template>
  <header class="topbar" v-if="showTopbar">
    <div class="title">Praćenje studentskih aktivnosti</div>
    <div class="user">
      {{ auth.user.ime }} {{ auth.user.prezime }} - {{ auth.roleLabel }}
      <button class="logout-btn" @click="auth.logout(); $router.push('/login')">Odjava</button>
    </div>
  </header>
  <RouterView />
</template>
