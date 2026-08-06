import { createRouter, createWebHistory, type RouteLocationNormalizedGeneric } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import AppLayout from '../layouts/AppLayout.vue'
import LoginView from '../views/LoginView.vue'
import UploadView from '../views/UploadView.vue'
import VideoRecordsView from '../views/VideoRecordsView.vue'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/login', name: 'login', component: LoginView },
    {
      path: '/',
      component: AppLayout,
      children: [
        { path: 'upload', name: 'upload', component: UploadView, meta: { requiresAuth: true } },
        { path: 'videos', name: 'videos', component: VideoRecordsView, meta: { requiresAuth: true } },
        { path: '', redirect: '/upload' },
      ],
    },
  ],
})

router.beforeEach((to: RouteLocationNormalizedGeneric) => {
  const authStore = useAuthStore()

  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    return { name: 'login' }
  }

  if (to.name === 'login' && authStore.isAuthenticated) {
    return { name: 'upload' }
  }

  return true
})

export default router
