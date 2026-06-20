<template>
    <div v-if="isLoginPage" class="login-page">
        <router-view />
    </div>
    <div v-else class="admin-container">
        <aside class="sidebar" :class="{ 'sidebar-visible': sidebarOpen }">
            <div class="sidebar-header">
                <div class="logo">南亭</div>
                <span class="admin-label">Admin</span>
            </div>
            <nav class="sidebar-nav">
                <router-link to="/friendlink" class="nav-item"
                    :class="{ active: $route.name === 'adminFriendlink' }"
                    @click="sidebarOpen = false">
                    <img src="/友链图标.svg" class="nav-icon" alt="友链管理">
                    <span class="nav-text">友链管理</span>
                </router-link>
                <router-link v-if="isAdmin" to="/posts" class="nav-item"
                    :class="{ active: $route.name === 'adminPosts' }"
                    @click="sidebarOpen = false">
                    <img src="/文章图标.svg" class="nav-icon" alt="文章管理">
                    <span class="nav-text">文章管理</span>
                </router-link>
                <router-link v-if="isAdmin" to="/config" class="nav-item"
                    :class="{ active: $route.name === 'adminConfig' }"
                    @click="sidebarOpen = false">
                    <img src="/设置图标.svg" class="nav-icon" alt="系统设置">
                    <span class="nav-text">系统设置</span>
                </router-link>
            </nav>
            <div class="sidebar-footer">
                <div class="user-info">
                    <div class="user-avatar">{{ userInitial }}</div>
                    <div class="user-name">{{ userName }}</div>
                </div>
            </div>
        </aside>
        <div class="overlay" v-if="sidebarOpen" @click="sidebarOpen = false"></div>
        <main class="main-content">
            <header class="mobile-header">
                <button class="menu-btn" @click="sidebarOpen = !sidebarOpen">
                    <span></span>
                    <span></span>
                    <span></span>
                </button>
                <h1 class="page-title">南亭 Admin</h1>
            </header>
            <router-view />
        </main>
    </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue';
import { sessionStore } from './ts/utils/sessionStore';
import { AuthAPI } from './ts/utils/AuthAPI';

const sidebarOpen = ref(false);
const isLoginPage = ref(!sessionStore.isLoggedIn());
const isAdmin = computed(() => sessionStore.isAdmin());
const userName = computed(() => sessionStore.getUserName() || 'User');
const userInitial = computed(() => userName.value.charAt(0).toUpperCase());

/** 加载用户角色 */
const loadUserRoles = async () => {
    const roles = await AuthAPI.getUserRoles();
    if (roles) {
        sessionStore.setRoles(roles);
    }
};

const j = () => {
    isLoginPage.value = !sessionStore.isLoggedIn();
    if (!isLoginPage.value) {
        loadUserRoles();
    }
    sessionStore.removeJWTChangeCallback(j);
};
sessionStore.addJWTChangeCallback(j);

// 如果已登录，立即加载角色
if (sessionStore.isLoggedIn()) {
    loadUserRoles();
}
</script>

<style scoped>
.admin-container {
    display: flex;
    height: 100vh;
    overflow: hidden;
    background: var(--color-bg-light);
}

.sidebar {
    width: 200px;
    background: var(--color-bg-white);
    border-right: 1px solid var(--color-border);
    display: flex;
    flex-direction: column;
    position: fixed;
    left: -200px;
    top: 0;
    height: 100vh;
    z-index: 500;
    transition: left 0.3s ease;
}

.sidebar-visible {
    left: 0;
}

.sidebar-header {
    padding: 20px;
    border-bottom: 1px solid var(--color-border);
}

.logo {
    font-size: 24px;
    font-weight: 700;
    color: var(--color-text);
}

.admin-label {
    font-size: 12px;
    color: var(--color-text-muted);
    margin-left: 8px;
}

.sidebar-nav {
    flex: 1;
    padding: 10px 0;
}

.nav-item {
    display: flex;
    align-items: center;
    padding: 12px 20px;
    color: var(--color-text-secondary);
    text-decoration: none;
    transition: all 0.2s;
}

.nav-item:hover:not(.disabled) {
    background: var(--color-bg-light);
    color: var(--color-primary);
}

.nav-item.active {
    background: var(--color-primary-light-bg);
    color: var(--color-primary);
}

.nav-item.disabled {
    color: var(--color-text-light);
    cursor: not-allowed;
}

.nav-icon {
    width: 16px;
    height: 16px;
    margin-right: 10px;
}

.nav-text {
    font-size: 14px;
}

.sidebar-footer {
    padding: 16px;
    border-top: 1px solid var(--color-border);
}

.user-info {
    display: flex;
    align-items: center;
}

.user-avatar {
    width: 36px;
    height: 36px;
    border-radius: 50%;
    background: var(--color-primary);
    color: var(--color-text-white);
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 14px;
    font-weight: 500;
    margin-right: 10px;
}

.user-name {
    font-size: 13px;
    color: var(--color-text);
}

.main-content {
    flex: 1;
    overflow: auto;
    min-height: 100vh;
}

.login-page {
    min-height: 100vh;
}

.mobile-header {
    display: flex;
    align-items: center;
    padding: 12px 16px;
    background: var(--color-bg-white);
    border-bottom: 1px solid var(--color-border);
    box-shadow: var(--shadow-md);
}

.menu-btn {
    width: 40px;
    height: 40px;
    display: flex;
    flex-direction: column;
    justify-content: center;
    align-items: center;
    background: transparent;
    border: none;
    cursor: pointer;
    padding: 0;
    margin-right: 12px;
}

.menu-btn span {
    width: 24px;
    height: 2px;
    background: var(--color-text);
    margin: 3px 0;
    transition: all 0.3s;
}

.page-title {
    font-size: 16px;
    font-weight: 600;
    color: var(--color-text);
    margin: 0;
}

.overlay {
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background: rgba(0, 0, 0, 0.5);
    z-index: 90;
}

@media (min-width: 768px) {
    .sidebar {
        position: static;
        left: auto;
        flex-shrink: 0;
        overflow-y: auto;
    }
    
    .mobile-header {
        display: none;
    }
    
    .overlay {
        display: none;
    }
}
</style>