<template>
  <div class="app-container">
    <header class="header">
      <div class="header-content">
        <div class="logo">Mellow</div>
        <nav class="nav">
          <router-link to="/" class="nav-link" active-class="active">首页</router-link>
          <router-link to="/" class="nav-link" active-class="active">文章</router-link>
          <a href="#" class="nav-link">关于</a>
        </nav>
      </div>
    </header>

    <main class="main-content">
      <router-view />
    </main>

    <footer class="footer">
      <p class="copyright">© 2025 Mellow · 用心生活，温柔设计</p>
    </footer>
  </div>
</template>

<script setup lang="ts">
</script>

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
  padding: 0 20px;
}

.header-content {
  max-width: 1100px;
  margin: 0 auto;
  display: flex;
  justify-content: space-between;
  align-items: center;
  height: 64px;
}

.logo {
  font-size: 20px;
  font-weight: 700;
  color: #333;
}

.nav {
  display: flex;
  gap: 24px;
}

.nav-link {
  font-size: 14px;
  color: #666;
  text-decoration: none;
  padding: 8px 12px;
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

.main-content {
  flex: 1;
  padding: 20px 0;
}

.footer {
  padding: 32px 20px;
  border-top: 1px solid #eee;
  text-align: center;
}

.copyright {
  font-size: 13px;
  color: #999;
  margin: 0;
}
</style>
