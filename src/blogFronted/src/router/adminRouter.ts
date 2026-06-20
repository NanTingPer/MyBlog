import { createRouter, createWebHashHistory } from 'vue-router';
import AdminFriendLink from '../views/admin/AdminFriendLink.vue';
import AdminPosts from '../views/admin/AdminPosts.vue';
import AdminConfig from '../views/admin/AdminConfig.vue';
import Login from '../views/admin/Login.vue';
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
        meta: { requiresAuth: true, requiresAdmin: true }
    },
    {
        path: '/config',
        name: 'adminConfig',
        component: AdminConfig,
        meta: { requiresAuth: true, requiresAdmin: true }
    }
];

const router = createRouter({
    history: createWebHashHistory(),
    routes
});

router.beforeEach((to) => {
    const requiresAuth = to.matched.some(record => record.meta.requiresAuth);
    const requiresAdmin = to.matched.some(record => record.meta.requiresAdmin);
    const isLoggedIn = sessionStore.isLoggedIn();
    const isAdmin = sessionStore.isAdmin();

    if (requiresAuth && !isLoggedIn) {
        return '/login';
    } else if (to.path === '/login' && isLoggedIn) {
        return '/friendlink';
    } else if (requiresAdmin && !isAdmin) {
        return '/friendlink';
    }
});

export default router;