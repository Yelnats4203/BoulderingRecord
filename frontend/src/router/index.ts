import { createRouter, createWebHistory, type RouteLocationNormalizedGeneric } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import AppLayout from '../layouts/AppLayout.vue'
import LoginView from '../views/LoginView.vue'
import UploadView from '../views/UploadView.vue'
import VideoRecordsView from '../views/VideoRecordsView.vue'
import CreateUserView from '../views/CreateUserView.vue'
import UserListView from '../views/UserListView.vue'
import ChangePasswordView from '../views/ChangePasswordView.vue'
import DashboardView from '../views/DashboardView.vue'
import CreateSessionView from '../views/CreateSessionView.vue'
import FriendListView from '../views/FriendListView.vue'
import FriendProfileView from '../views/FriendProfileView.vue'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/login', name: 'login', component: LoginView },
    {
      path: '/',
      component: AppLayout,
      children: [
        { path: 'dashboard', name: 'dashboard', component: DashboardView, meta: { requiresAuth: true } },
        {
          path: 'sessions/create',
          name: 'createSession',
          component: CreateSessionView,
          meta: { requiresAuth: true },
        },
        { path: 'upload', name: 'upload', component: UploadView, meta: { requiresAuth: true } },
        { path: 'videos', name: 'videos', component: VideoRecordsView, meta: { requiresAuth: true } },
        { path: 'friends', name: 'friends', component: FriendListView, meta: { requiresAuth: true } },
        {
          path: 'friends/:userId',
          name: 'friendProfile',
          component: FriendProfileView,
          meta: { requiresAuth: true },
        },
        {
          path: 'change-password',
          name: 'changePassword',
          component: ChangePasswordView,
          meta: { requiresAuth: true },
        },
        {
          path: 'users',
          name: 'users',
          component: CreateUserView,
          meta: { requiresAuth: true, requiresEditPermission: true },
        },
        {
          path: 'user-list',
          name: 'userList',
          component: UserListView,
          meta: { requiresAuth: true, requiresEditPermission: true },
        },
        { path: '', redirect: '/dashboard' },
      ],
    },
  ],
})

router.beforeEach((to: RouteLocationNormalizedGeneric) => {
  const authStore = useAuthStore()

  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    return { name: 'login' }
  }

  if (to.meta.requiresEditPermission && !authStore.hasEditPermission) {
    return { name: 'dashboard' }
  }

  if (to.name === 'login' && authStore.isAuthenticated) {
    return { name: 'dashboard' }
  }

  return true
})

export default router
