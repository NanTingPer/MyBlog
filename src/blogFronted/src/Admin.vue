<template>
    <div v-if="isLoginPage" class="login-page">
        <router-view />
    </div>
    <div v-else class="admin-container">
        <aside class="sidebar">
            <div class="sidebar-header">
                <div class="logo">南亭</div>
                <span class="admin-label">Admin</span>
            </div>
            <nav class="sidebar-nav">
                <router-link to="/friendlink" class="nav-item"
                    :class="{ active: $route.name === 'adminFriendlink' }">
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
        <main class="main-content">
            <router-view />
        </main>
    </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';

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
}

.login-page {
    min-height: 100vh;
}
</style>