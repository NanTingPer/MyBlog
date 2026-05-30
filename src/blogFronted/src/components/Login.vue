<template>
    <div class="login-bg flex items-center justify-center min-h-screen">
        <div ref="formSectionRef" class="form-section max-w-360">
            <div class="friendlink-form">
                <div class="text-center mb-8">
                    <h1 class="text-3xl font-bold text-gray-800 mb-2">南亭</h1>
                    <p class="text-sm text-gray-400">
                        {{ activeTab === 'login' ? '欢迎回来，请输入密码登录' : '创建新账户' }}
                    </p>
                </div>

                <div class="tabs flex mb-6 border-b border-gray-200">
                    <button class="tab-btn flex-1 py-2 text-center text-sm"
                        :class="activeTab === 'login' ? 'tab-active' : 'tab-inactive'" @click="switchTab('login')">
                        登录
                    </button>
                    <button class="tab-btn flex-1 py-2 text-center text-sm"
                        :class="activeTab === 'register' ? 'tab-active' : 'tab-inactive'"
                        @click="switchTab('register')">
                        注册
                    </button>
                </div>
                <!-- const { onBeforeEnter, onEnter } = useStaggerAnimation(); -->
                <Transition v-on:before-enter="onBeforeEnter" v-on:enter="onEnter">
                    <!-- 登录表单 -->
                    <form v-if="activeTab === 'login'" class="flex flex-col" @submit.prevent="handleLogin">
                        <div class="form-group">
                            <div class="input-icon-wrap relative flex items-center">
                                <img :src="userIconSvg" class="input-icon absolute left-3 w-4 h-4" alt="用户名">
                                <input type="text" v-model="loginForm.userName" class="form-input pl-10"
                                    placeholder="输入用户名" required
                                    @keyup.enter="(loginPasswordInput as HTMLInputElement)?.focus()" />
                            </div>
                        </div>
                        <div class="form-group">
                            <div class="input-icon-wrap relative flex items-center">
                                <img :src="lockIconSvg" class="input-icon absolute left-3 w-4 h-4" alt="密码">
                                <input ref="loginPasswordInput" type="password" v-model="loginForm.password"
                                    class="form-input pl-10" placeholder="输入密码" required @keyup.enter="handleLogin" />
                            </div>
                        </div>
                        <div class="form-actions">
                            <button type="submit" class="btn-save w-full flex items-center justify-center gap-2"
                                :disabled="loginLoading">
                                <span v-if="loginLoading" class="loading-spinner"></span>
                                {{ loginLoading ? '登录中...' : '登录' }}
                            </button>
                        </div>
                        <div v-if="loginError" class="error-msg mt-3">
                            {{ loginError }}
                        </div>
                    </form>

                    <!-- 注册表单 -->
                    <form v-else class="flex flex-col" @submit.prevent="handleRegister">
                        <div class="form-group">
                            <div class="input-icon-wrap relative flex items-center">
                                <img :src="userIconSvg" class="input-icon absolute left-3 w-4 h-4" alt="用户名">
                                <input type="text" v-model="registerForm.userName" class="form-input pl-10"
                                    placeholder="输入用户名" required />
                            </div>
                        </div>
                        <div class="form-group">
                            <div class="input-icon-wrap relative flex items-center">
                                <img :src="mailIconSvg" class="input-icon absolute left-3 w-4 h-4" alt="邮箱">
                                <input type="email" v-model="registerForm.mailAddress" class="form-input pl-10"
                                    placeholder="输入邮箱地址" required />
                            </div>
                        </div>
                        <div class="form-group">
                            <div class="flex gap-2">
                                <div class="input-icon-wrap relative flex items-center flex-1">
                                    <img :src="codeIconSvg" class="input-icon absolute left-3 w-4 h-4" alt="验证码">
                                    <input type="text" v-model="registerForm.mailCode" class="form-input pl-10"
                                        placeholder="邮箱验证码" required />
                                </div>
                                <button type="button" class="btn-send-code"
                                    :disabled="mailCodeCooldown > 0 || mailCodeSending" @click="handleSendMailCode">
                                    {{ mailCodeCooldown > 0 ? `${mailCodeCooldown}s` : '发送验证码' }}
                                </button>
                            </div>
                        </div>
                        <div class="form-group">
                            <div class="input-icon-wrap relative flex items-center">
                                <img :src="lockIconSvg" class="input-icon absolute left-3 w-4 h-4" alt="密码">
                                <input type="password" v-model="registerForm.password" class="form-input pl-10"
                                    placeholder="输入密码" required />
                            </div>
                        </div>
                        <div class="form-group">
                            <div class="input-icon-wrap relative flex items-center">
                                <img :src="lockIconSvg" class="input-icon absolute left-3 w-4 h-4" alt="确认密码">
                                <input type="password" v-model="registerForm.confirmPassword" class="form-input pl-10"
                                    placeholder="确认密码" required />
                            </div>
                        </div>
                        <div class="form-actions">
                            <button type="submit" class="btn-save w-full flex items-center justify-center gap-2"
                                :disabled="!isRegisterFormValid || registerLoading">
                                <span v-if="registerLoading" class="loading-spinner"></span>
                                {{ registerLoading ? '注册中...' : '注册' }}
                            </button>
                        </div>
                        <div v-if="registerError" class="mt-3" :class="registerSuccess ? 'success-msg' : 'error-msg'">
                            {{ registerError }}
                        </div>
                    </form>
                </Transition>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, nextTick } from 'vue';
import { AuthAPI } from '../ts/utils/AuthAPI';
import { useRouter } from 'vue-router';
import { useStaggerAnimation } from '../composables/useStaggerAnimation'
import { useLerpHeight } from '../composables/useLerpHeight'
const { onBeforeEnter, onEnter } = useStaggerAnimation();

const formSectionRef = ref<HTMLElement | null>(null);
const { lockHeight, lerpToContent } = useLerpHeight(formSectionRef);

const router = useRouter();

const userIconSvg = "data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='16' height='16' viewBox='0 0 24 24' fill='none' stroke='%23999' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'%3E%3Cpath d='M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2'%3E%3C/path%3E%3Ccircle cx='12' cy='7' r='4'%3E%3C/circle%3E%3C/svg%3E";
const lockIconSvg = "data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='16' height='16' viewBox='0 0 24 24' fill='none' stroke='%23999' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'%3E%3Crect x='3' y='11' width='18' height='11' rx='2' ry='2'%3E%3C/rect%3E%3Cpath d='M7 11V7a5 5 0 0 1 10 0v4'%3E%3C/path%3E%3C/svg%3E";
const mailIconSvg = "data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='16' height='16' viewBox='0 0 24 24' fill='none' stroke='%23999' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'%3E%3Crect x='2' y='4' width='20' height='16' rx='2'%3E%3C/rect%3E%3Cpath d='M22 7l-10 7L2 7'%3E%3C/path%3E%3C/svg%3E";
const codeIconSvg = "data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='16' height='16' viewBox='0 0 24 24' fill='none' stroke='%23999' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'%3E%3Crect x='3' y='11' width='18' height='11' rx='2' ry='2'%3E%3C/rect%3E%3Cpath d='M7 11V7a5 5 0 0 1 10 0v4'%3E%3C/path%3E%3C/svg%3E";

type TabType = 'login' | 'register';

const activeTab = ref<TabType>('login');
const loginPasswordInput = ref<HTMLInputElement | null>(null);

// 登录表单
const loginForm = reactive({ userName: '', password: '' });
const loginLoading = ref(false);
const loginError = ref('');

// 注册表单
const registerForm = reactive({
    userName: '',
    mailAddress: '',
    mailCode: '',
    mailId: '',
    password: '',
    confirmPassword: ''
});
const registerLoading = ref(false);
const registerError = ref('');
const registerSuccess = ref(false);
const mailCodeSending = ref(false);
const mailCodeCooldown = ref(0);

const isRegisterFormValid = computed(() => {
    return (
        registerForm.userName.trim() !== '' &&
        registerForm.mailAddress.trim() !== '' &&
        registerForm.mailCode.trim() !== '' &&
        registerForm.mailId !== '' &&
        registerForm.password.trim() !== '' &&
        registerForm.confirmPassword.trim() !== '' &&
        registerForm.password === registerForm.confirmPassword
    );
});

let cooldownTimer: ReturnType<typeof setInterval> | null = null;

function switchTab(tab: TabType) {
    if (tab === activeTab.value) return;
    const oldHeight = formSectionRef.value?.offsetHeight ?? 0;
    // 先锁定高度，防止内容变化导致跳动
    lockHeight(oldHeight);
    activeTab.value = tab;
    loginError.value = '';
    registerError.value = '';
    registerSuccess.value = false;
    // 内容切换后，测量目标高度并开始 Lerp
    nextTick(() => {
        lerpToContent();
    });
}

async function handleLogin() {
    if (!loginForm.userName.trim()) {
        loginError.value = '请输入用户名';
        return;
    }
    if (!loginForm.password.trim()) {
        loginError.value = '请输入密码';
        return;
    }

    loginLoading.value = true;
    loginError.value = '';

    try {
        const response = await AuthAPI.login(loginForm.userName, loginForm.password);

        if (response && response.code === 200) {
            router.push('/friendlink');
        } else {
            loginError.value = response?.msg || '用户名或密码错误，请重试';
        }
    } catch (error) {
        loginError.value = '登录失败，请稍后重试';
        console.error('登录错误:', error);
    } finally {
        loginLoading.value = false;
    }
}

async function handleSendMailCode() {
    if (!registerForm.mailAddress.trim()) {
        registerError.value = '请先输入邮箱地址';
        registerSuccess.value = false;
        return;
    }

    mailCodeSending.value = true;
    registerError.value = '';

    try {
        const response = await AuthAPI.getMailVerificationCode(
            registerForm.mailAddress,
            registerForm.userName || undefined
        );

        if (response && response.code === 200 && response.data) {
            registerForm.mailId = response.data.id;
            registerError.value = '';
            startCooldown();
        } else {
            registerError.value = response?.msg || '发送验证码失败';
            registerSuccess.value = false;
        }
    } catch (error) {
        registerError.value = '发送验证码失败，请稍后重试';
        registerSuccess.value = false;
        console.error('发送验证码错误:', error);
    } finally {
        mailCodeSending.value = false;
    }
}

function startCooldown() {
    mailCodeCooldown.value = 60 * 5;
    if (cooldownTimer) {
        clearInterval(cooldownTimer);
    }
    cooldownTimer = setInterval(() => {
        mailCodeCooldown.value--;
        if (mailCodeCooldown.value <= 0) {
            if (cooldownTimer) {
                clearInterval(cooldownTimer);
                cooldownTimer = null;
            }
        }
    }, 1000);
}

async function handleRegister() {
    if (!isRegisterFormValid.value) {
        if (registerForm.password !== registerForm.confirmPassword) {
            registerError.value = '两次输入的密码不一致';
        } else {
            registerError.value = '请填写所有必填项';
        }
        registerSuccess.value = false;
        return;
    }

    registerLoading.value = true;
    registerError.value = '';

    try {
        const response = await AuthAPI.register(
            registerForm.userName,
            registerForm.password,
            registerForm.mailAddress,
            registerForm.mailId,
            registerForm.mailCode
        );

        if (response && response.code === 200) {
            registerSuccess.value = true;
            registerError.value = '注册成功！请登录';
            setTimeout(() => {
                switchTab('login');
                loginForm.userName = registerForm.userName;
            }, 1500);
        } else {
            registerSuccess.value = false;
            registerError.value = response?.msg || '注册失败，请重试';
        }
    } catch (error) {
        registerSuccess.value = false;
        registerError.value = '注册失败，请稍后重试';
        console.error('注册错误:', error);
    } finally {
        registerLoading.value = false;
    }
}
</script>

<style scoped>
/* 背景渐变 */
.login-bg {
    background: linear-gradient(135deg, #f5f7fa 0%, #e4e8eb 100%);
    position: relative;
}

/* Tab 切换 */
.tab-btn {
    border: none;
    background: none;
    cursor: pointer;
    transition: color 0.15s, border-color 0.15s;
    border-bottom: 2px solid transparent;
}

.tab-active {
    color: #4caf50;
    border-bottom-color: #4caf50;
    font-weight: 500;
}

.tab-inactive {
    color: #9ca3af;
}

.tab-inactive:hover {
    color: #4b5563;
}

/* 输入框图标区域 */
.input-icon-wrap .form-input {
    padding-left: 40px;
}

.input-icon {
    color: #999;
    pointer-events: none;
}

/* 发送验证码按钮 */
.btn-send-code {
    white-space: nowrap;
    padding: 12px 16px;
    border-radius: 6px;
    font-size: 14px;
    background: #fff;
    color: #4caf50;
    border: 1px solid #4caf50;
    cursor: pointer;
    transition: all 0.2s;
}

.btn-send-code:hover:not(:disabled) {
    background: #4caf50;
    color: #fff;
}

.btn-send-code:disabled {
    border-color: #d1d5db;
    color: #9ca3af;
    cursor: not-allowed;
    background: #f9fafb;
}

/* 加载动画 */
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

/* 消息提示 */
.error-msg {
    padding: 10px 12px;
    background: #ffebee;
    color: #e53935;
    border-radius: 6px;
    font-size: 13px;
    text-align: center;
}

.success-msg {
    padding: 10px 12px;
    background: #f0fdf4;
    color: #16a34a;
    border-radius: 6px;
    font-size: 13px;
    text-align: center;
}
</style>
