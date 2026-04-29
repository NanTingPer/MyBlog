<template>
    <div class="app-container">
        <header class="header">
            <div class="header-content">
                <div class="logo">南亭</div>
                <nav class="nav" :class="{ 'nav-open': isNavOpen }">
                    <router-link to="/latest" class="nav-link" active-class="active" @click="isNavOpen = false">首页</router-link>
                    <router-link to="/latest" class="nav-link" active-class="active" @click="isNavOpen = false">文章</router-link>
                    <router-link to="/friendlink" class="nav-link" active-class="active" @click="isNavOpen = false">友链</router-link>
                    <a href="#" class="nav-link" @click="isNavOpen = false">关于</a>
                    <a href="/admin.html" class="nav-link" target="_blank">后台管理</a>
                </nav>
                <button class="menu-btn" @click="isNavOpen = !isNavOpen">
                    <span></span>
                    <span></span>
                    <span></span>
                </button>
            </div>
        </header>

        <main class="main-content">
            <router-view />
        </main>

        <footer class="footer">
            <p class="copyright">Vue3 + ASP.NET 10 强力驱动</p>
        </footer>
    </div>
</template>

<style scoped>
.app-container {
    min-height: 100vh;
    display: flex;
    flex-direction: column;
    background: #faf8f5;
}

.header {
    background: #f8f7f4;
    border-bottom: 1px solid #eee;
    padding: 0 16px;
    position: relative;
}

.header-content {
    max-width: 1100px;
    margin: 0 auto;
    display: flex;
    justify-content: space-between;
    align-items: center;
    height: 56px;
}

.logo {
    font-size: 18px;
    font-weight: 700;
    color: #333;
}

.nav {
    display: flex;
    gap: 16px;
}

.nav-link {
    font-size: 14px;
    color: #666;
    text-decoration: none;
    padding: 6px 10px;
    border-radius: 6px;
    transition: all 0.2s;
}

.nav-link:hover {
    color: #4caf50;
    background: rgba(76, 175, 80, 0.1);
}

.nav-link.active {
    color: #4caf50;
    font-weight: 500;
}

.menu-btn {
    display: none;
    flex-direction: column;
    gap: 5px;
    background: none;
    border: none;
    cursor: pointer;
    padding: 8px;
}

.menu-btn span {
    width: 24px;
    height: 2px;
    background: #333;
    transition: all 0.3s;
}

.main-content {
    flex: 1;
    padding: 16px;
}

.footer {
    padding: 24px 16px;
    border-top: 1px solid #eee;
    text-align: center;
}

.copyright {
    font-size: 12px;
    color: #999;
    margin: 0;
}

@media (max-width: 768px) {
    .header {
        padding: 0 12px;
    }

    .header-content {
        height: 52px;
    }

    .logo {
        font-size: 16px;
    }

    .nav {
        position: absolute;
        top: 52px;
        left: 0;
        right: 0;
        background: #f8f7f4;
        flex-direction: column;
        padding: 12px 0;
        box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
        transform: translateY(-150%);
        opacity: 0;
        visibility: hidden;
        transition: all 0.3s ease;
        z-index: 100;
    }

    .nav-open {
        transform: translateY(0);
        opacity: 1;
        visibility: visible;
    }

    .nav-link {
        padding: 10px 20px;
        font-size: 15px;
    }

    .menu-btn {
        display: flex;
    }

    .nav-open + .menu-btn span:nth-child(1) {
        transform: rotate(45deg) translate(5px, 5px);
    }

    .nav-open + .menu-btn span:nth-child(2) {
        opacity: 0;
    }

    .nav-open + .menu-btn span:nth-child(3) {
        transform: rotate(-45deg) translate(7px, -6px);
    }

    .main-content {
        padding: 12px;
    }

    .footer {
        padding: 20px 12px;
    }

    .copyright {
        font-size: 11px;
    }
}

@media (max-width: 480px) {
    .nav-link {
        font-size: 14px;
        padding: 8px 16px;
    }

    .main-content {
        padding: 8px;
    }
}
</style>

<script setup lang="ts">
import { ref } from 'vue'

const isNavOpen = ref(false)
</script>