import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const routes = [
  { path: '/', redirect: '/login' },
  { path: '/login', name: 'login', component: () => import('@/views/LoginView.vue') },

  { path: '/student', component: () => import('@/views/student/StudentDashboard.vue'), meta: { role: 'student' } },
  { path: '/student/aktivnosti', component: () => import('@/views/student/StudentActivities.vue'), meta: { role: 'student' } },
  { path: '/student/profil', component: () => import('@/views/student/StudentProfile.vue'), meta: { role: 'student' } },

  { path: '/nastavnik', component: () => import('@/views/teacher/TeacherDashboard.vue'), meta: { role: 'nastavnik' } },
  { path: '/nastavnik/aktivnosti', component: () => import('@/views/teacher/TeacherActivities.vue'), meta: { role: 'nastavnik' } },
  { path: '/nastavnik/studenti', component: () => import('@/views/teacher/TeacherStudents.vue'), meta: { role: 'nastavnik' } },
  { path: '/nastavnik/profil', component: () => import('@/views/teacher/TeacherProfile.vue'), meta: { role: 'nastavnik' } },

  { path: '/admin', component: () => import('@/views/admin/AdminDashboard.vue'), meta: { role: 'admin' } },
  { path: '/admin/korisnici', component: () => import('@/views/admin/AdminUsers.vue'), meta: { role: 'admin' } },
  { path: '/admin/kolegiji', component: () => import('@/views/admin/AdminCourses.vue'), meta: { role: 'admin' } },
  { path: '/admin/evidencije', component: () => import('@/views/admin/AdminRecords.vue'), meta: { role: 'admin' } },
]

const router = createRouter({
  history: createWebHistory(),
  routes,
})

router.beforeEach((to, from, next) => {
  const auth = useAuthStore()
  if (to.meta.role && !auth.user) return next('/login')
  if (to.meta.role && auth.user.role !== to.meta.role) return next('/login')
  next()
})

export default router
