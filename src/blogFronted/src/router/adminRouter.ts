import { createRouter, createWebHistory } from 'vue-router';
import AdminFriendLink from '../components/AdminFriendLink.vue';
import Login from '../components/Login.vue';
import { sessionStore } from '../ts/utils/sessionStore';

const routes = [
  {
    path: '/login',
    name: 'login',
    component: Login
  },
  {
    path: '/',
    name: 'admin',
    redirect: '/login',
    meta: { requiresAuth: true }
  },
  {
    path: '/friendlink',
    name: 'adminFriendlink',
    component: AdminFriendLink,
    meta: { requiresAuth: true }
  }
];

const router = createRouter({
  history: createWebHistory(),
  routes
});

router.beforeEach((to, _, next) => {
  const requiresAuth = to.matched.some(record => record.meta.requiresAuth);
  const isLoggedIn = sessionStore.isLoggedIn();

  if (requiresAuth && !isLoggedIn) {
    next('/login');
  } else if (to.path === '/login' && isLoggedIn) {
    next('/friendlink');
  } else {
    next();
  }
});

export default router;