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
                <div class="nav-item disabled">
                    <img src="/文章图标.svg" class="nav-icon" alt="文章管理">
                    <span class="nav-text">文章管理</span>
                </div>
                <div class="nav-item disabled">
                    <img src="/设置图标.svg" class="nav-icon" alt="系统设置">
                    <span class="nav-text">系统设置</span>
                </div>
            </nav>
            <div class="sidebar-footer">
                <div class="user-info">
                    <div class="user-avatar">M</div>
                    <div class="user-name">Admin</div>
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
import { computed, ref } from 'vue';

const sidebarOpen = ref(false);

const isLoginPage = computed(() => {
    return window.location.hash === '#/login' || window.location.hash === '';
});
</script>

<style scoped>
.admin-container {
    display: flex;
    min-height: 100vh;
    background: #f5f5f5;
}

.sidebar {
    width: 200px;
    background: #fff;
    border-right: 1px solid #e8e8e8;
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
    border-bottom: 1px solid #e8e8e8;
}

.logo {
    font-size: 24px;
    font-weight: 700;
    color: #333;
}

.admin-label {
    font-size: 12px;
    color: #999;
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
    color: #666;
    text-decoration: none;
    transition: all 0.2s;
}

.nav-item:hover:not(.disabled) {
    background: #f5f7fa;
    color: #4caf50;
}

.nav-item.active {
    background: #e8f5e9;
    color: #4caf50;
}

.nav-item.disabled {
    color: #ccc;
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
    border-top: 1px solid #e8e8e8;
}

.user-info {
    display: flex;
    align-items: center;
}

.user-avatar {
    width: 36px;
    height: 36px;
    border-radius: 50%;
    background: #4caf50;
    color: #fff;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 14px;
    font-weight: 500;
    margin-right: 10px;
}

.user-name {
    font-size: 13px;
    color: #333;
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
    background: #fff;
    border-bottom: 1px solid #e8e8e8;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
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
    background: #333;
    margin: 3px 0;
    transition: all 0.3s;
}

.page-title {
    font-size: 16px;
    font-weight: 600;
    color: #333;
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
        height: auto;
    }
    
    .mobile-header {
        display: none;
    }
    
    .overlay {
        display: none;
    }
}
</style>