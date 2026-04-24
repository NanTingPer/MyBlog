import { createRouter, createWebHashHistory } from 'vue-router';
import AdminFriendLink from '../components/AdminFriendLink.vue';

const routes = [
  {
    path: '/',
    name: 'admin',
    redirect: '/friendlink'
  },
  {
    path: '/friendlink',
    name: 'adminFriendlink',
    component: AdminFriendLink
  }
];

const router = createRouter({
  history: createWebHashHistory(),
  routes
});

export default router;