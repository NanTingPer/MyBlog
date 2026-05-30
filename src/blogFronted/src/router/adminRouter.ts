import { createRouter, createWebHashHistory } from 'vue-router';
import AdminFriendLink from '../components/AdminFriendLink.vue';
import AdminPosts from '../components/AdminPosts.vue';
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
    },
    {
        path: '/posts',
        name: 'adminPosts',
        component: AdminPosts,
        meta: { requiresAuth: true }
    }
];

const router = createRouter({
    history: createWebHashHistory(),
    routes
});

router.beforeEach((to) => {
    const requiresAuth = to.matched.some(record => record.meta.requiresAuth);
    const isLoggedIn = sessionStore.isLoggedIn();

    if (requiresAuth && !isLoggedIn) {
        return '/login';
    } else if (to.path === '/login' && isLoggedIn) {
        return '/friendlink';
    }
});

export default router;