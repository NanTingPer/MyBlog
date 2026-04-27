<template>
    <div class="login-container">
        <div class="login-card">
            <div class="login-header">
                <h1 class="logo">Mellow</h1>
                <p class="subtitle">欢迎回来，请输入密码登录</p>
            </div>
            <form class="login-form" @submit.prevent="handleLogin">
                <div class="form-group">
                    <div class="input-wrapper">
                        <img src="data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='16' height='16' viewBox='0 0 24 24' fill='none' stroke='%23999' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'%3E%3Crect x='3' y='11' width='18' height='11' rx='2' ry='2'%3E%3C/rect%3E%3Cpath d='M7 11V7a5 5 0 0 1 10 0v4'%3E%3C/path%3E%3C/svg%3E" class="input-icon" alt="密码">
                        <input 
                            type="password" 
                            v-model="password" 
                            class="password-input" 
                            placeholder="输入密码" 
                            required
                            @keyup.enter="handleLogin"
                        />
                    </div>
                </div>
                <button type="submit" class="login-btn" :disabled="isLoading">
                    <span v-if="isLoading" class="loading-spinner"></span>
                    {{ isLoading ? '登录中...' : '登录' }}
                </button>
                <div v-if="errorMessage" class="error-message">{{ errorMessage }}</div>
            </form>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { AuthAPI } from '../ts/utils/AuthAPI';
import { useRouter } from 'vue-router';

const router = useRouter();
const password = ref('');
const isLoading = ref(false);
const errorMessage = ref('');

const handleLogin = async () => {
    if (!password.value.trim()) {
        errorMessage.value = '请输入密码';
        return;
    }

    isLoading.value = true;
    errorMessage.value = '';

    try {
        const response = await AuthAPI.getToken(password.value);
        
        if (response && response.code === 200) {
            router.push('/friendlink');
        } else {
            errorMessage.value = '密码错误，请重试';
        }
    } catch (error) {
        errorMessage.value = '登录失败，请稍后重试';
        console.error('登录错误:', error);
    } finally {
        isLoading.value = false;
    }
};
</script>

<style scoped>
.login-container {
    min-height: 100vh;
    display: flex;
    align-items: center;
    justify-content: center;
    background: linear-gradient(135deg, #f5f7fa 0%, #e4e8eb 100%);
}

.login-card {
    background: #fff;
    border-radius: 16px;
    padding: 48px 40px;
    box-shadow: 0 4px 24px rgba(0, 0, 0, 0.08);
    width: 100%;
    max-width: 360px;
}

.login-header {
    text-align: center;
    margin-bottom: 32px;
}

.logo {
    font-size: 32px;
    font-weight: 700;
    color: #333;
    margin: 0 0 8px 0;
}

.subtitle {
    font-size: 14px;
    color: #999;
    margin: 0;
}

.login-form {
    display: flex;
    flex-direction: column;
}

.form-group {
    margin-bottom: 20px;
}

.input-wrapper {
    position: relative;
    display: flex;
    align-items: center;
}

.input-icon {
    position: absolute;
    left: 12px;
    width: 16px;
    height: 16px;
    color: #999;
}

.password-input {
    width: 100%;
    padding: 12px 12px 12px 40px;
    border: 1px solid #e8e8e8;
    border-radius: 8px;
    font-size: 14px;
    outline: none;
    transition: border-color 0.2s, box-shadow 0.2s;
    box-sizing: border-box;
}

.password-input:focus {
    border-color: #4caf50;
    box-shadow: 0 0 0 3px rgba(76, 175, 80, 0.1);
}

.password-input::placeholder {
    color: #ccc;
}

.login-btn {
    background: #4caf50;
    color: #fff;
    border: none;
    padding: 12px;
    border-radius: 8px;
    font-size: 14px;
    font-weight: 500;
    cursor: pointer;
    transition: background 0.2s;
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 8px;
}

.login-btn:hover:not(:disabled) {
    background: #43a047;
}

.login-btn:disabled {
    background: #a5d6a7;
    cursor: not-allowed;
}

.loading-spinner {
    width: 16px;
    height: 16px;
    border: 2px solid rgba(255, 255, 255, 0.3);
    border-top-color: #fff;
    border-radius: 50%;
    animation: spin 0.6s linear infinite;
}

@keyframes spin {
    to {
        transform: rotate(360deg);
    }
}

.error-message {
    margin-top: 12px;
    padding: 10px 12px;
    background: #ffebee;
    color: #e53935;
    border-radius: 6px;
    font-size: 13px;
    text-align: center;
}
</style>