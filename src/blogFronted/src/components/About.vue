<!--
  About - 关于页面组件

  使用 useStaggerAnimation 提供内容淡入动画。
  动画细节见 src/composables/useStaggerAnimation.md
-->
<template>

    <div class="page-container">
        <div class="page-header">
            <h1 class="page-title">关于</h1>
            <p class="page-subtitle">关于这个博客以及我</p>
        </div>
        <Transition appear @before-enter="onBeforeEnter" @enter="onEnter">
            <div class="about-content">
                <div class="card about-card">
                    <h2 class="card-title">平台帐号</h2>
                    <div class="account-list">
                        <a href="https://github.com/NanTingPer" target="_blank" rel="noopener noreferrer"
                            class="account-item">
                            <svg class="account-icon" viewBox="0 0 24 24" fill="currentColor">
                                <path
                                    d="M12 0c-6.626 0-12 5.373-12 12 0 5.302 3.438 9.8 8.207 11.387.599.111.793-.261.793-.577v-2.234c-3.338.726-4.033-1.416-4.033-1.416-.546-1.387-1.333-1.756-1.333-1.756-1.089-.745.083-.729.083-.729 1.205.084 1.839 1.237 1.839 1.237 1.07 1.834 2.807 1.304 3.492.997.107-.775.418-1.305.762-1.604-2.665-.305-5.467-1.334-5.467-5.931 0-1.311.469-2.381 1.236-3.221-.124-.303-.535-1.524.117-3.176 0 0 1.008-.322 3.301 1.23.957-.266 1.983-.399 3.003-.404 1.02.005 2.047.138 3.006.404 2.291-1.552 3.297-1.23 3.297-1.23.653 1.653.242 2.874.118 3.176.77.84 1.235 1.911 1.235 3.221 0 4.609-2.807 5.624-5.479 5.921.43.372.823 1.102.823 2.222v3.293c0 .319.192.694.801.576 4.765-1.589 8.199-6.086 8.199-11.386 0-6.627-5.373-12-12-12z" />
                            </svg>
                            <div class="account-info">
                                <span class="account-name">GitHub</span>
                                <span class="account-link">github.com/NanTingPer</span>
                            </div>
                            <svg class="arrow-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor"
                                stroke-width="2">
                                <path d="M9 18l6-6-6-6" />
                            </svg>
                        </a>
                        <a href="https://space.bilibili.com/691183498" target="_blank" rel="noopener noreferrer"
                            class="account-item">
                            <svg class="account-icon bilibili-icon" viewBox="0 0 24 24" fill="currentColor">
                                <path
                                    d="M17.813 4.653h.854c1.51.054 2.769.578 3.773 1.574 1.004.995 1.524 2.249 1.56 3.76v7.36c-.036 1.51-.556 2.769-1.56 3.773s-2.262 1.524-3.773 1.56H5.333c-1.51-.036-2.769-.556-3.773-1.56S.036 18.858 0 17.347v-7.36c.036-1.511.556-2.765 1.56-3.76 1.004-.996 2.262-1.52 3.773-1.574h.774l-1.174-1.12a1.234 1.234 0 0 1-.373-.906c0-.356.124-.658.373-.907l.027-.027c.267-.249.573-.373.92-.373.347 0 .653.124.92.373L9.653 4.44c.071.071.134.142.187.213h4.267a.836.836 0 0 1 .16-.213l2.853-2.747c.267-.249.573-.373.92-.373.347 0 .662.151.929.4.267.249.391.551.391.907 0 .355-.124.657-.373.906zM5.333 7.24c-.746.018-1.373.276-1.88.773-.506.498-.769 1.13-.786 1.894v7.52c.017.764.28 1.395.786 1.893.507.498 1.134.756 1.88.773h13.334c.746-.017 1.373-.275 1.88-.773.506-.498.769-1.129.786-1.893v-7.52c-.017-.765-.28-1.396-.786-1.894-.507-.497-1.134-.755-1.88-.773zM8 11.107c.373 0 .684.124.933.373.25.249.383.569.4.96v1.173c-.017.391-.15.711-.4.96-.249.25-.56.374-.933.374s-.684-.125-.933-.374c-.25-.249-.383-.569-.4-.96V12.44c0-.373.129-.689.386-.947.258-.257.574-.386.947-.386zm8 0c.373 0 .684.124.933.373.25.249.383.569.4.96v1.173c-.017.391-.15.711-.4.96-.249.25-.56.374-.933.374s-.684-.125-.933-.374c-.25-.249-.383-.569-.4-.96V12.44c.017-.391.15-.711.4-.96.249-.249.56-.373.933-.373z" />
                            </svg>
                            <div class="account-info">
                                <span class="account-name">Bilibili</span>
                                <span class="account-link">space.bilibili.com/691183498</span>
                            </div>
                            <svg class="arrow-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor"
                                stroke-width="2">
                                <path d="M9 18l6-6-6-6" />
                            </svg>
                        </a>
                    </div>
                </div>
            </div>
        </Transition>
    </div>

</template>

<script setup lang="ts">
/**
 * 关于页面使用 useStaggerAnimation 的单元素淡入模式。
 * 虽然 composable 名为「交错动画」，但单元素场景下无交错效果，
 * 仅使用其渐入（opacity + translateY）能力。
 */
import { useStaggerAnimation } from '../composables/useStaggerAnimation';

/** 获取动画钩子 */
const { onBeforeEnter, onEnter } = useStaggerAnimation();
</script>

<style scoped>
/* ========== 关于页面内容样式 ========== */
.about-content {
    display: flex;
    flex-direction: column;
    gap: 24px;
}

.about-card {
    padding: 24px;
}

.card-title {
    font-size: 18px;
    font-weight: 600;
    color: #333;
    margin: 0 0 20px;
}

.account-list {
    display: flex;
    flex-direction: column;
    gap: 12px;
}

.account-item {
    display: flex;
    align-items: center;
    padding: 16px;
    border-radius: 12px;
    background: #faf8f5;
    text-decoration: none;
    transition: all 0.2s;
    cursor: pointer;
}

.account-item:hover {
    background: #f0ede8;
    transform: translateY(-2px);
}

.account-icon {
    width: 36px;
    height: 36px;
    flex-shrink: 0;
    color: #333;
}

.bilibili-icon {
    color: #00a1d6;
}

.account-info {
    flex: 1;
    margin-left: 16px;
    display: flex;
    flex-direction: column;
    gap: 4px;
}

.account-name {
    font-size: 15px;
    font-weight: 600;
    color: #333;
}

.account-link {
    font-size: 13px;
    color: #999;
}

.arrow-icon {
    width: 20px;
    height: 20px;
    color: #ccc;
    flex-shrink: 0;
}

@media (max-width: 768px) {
    .about-card {
        padding: 20px;
    }

    .account-item {
        padding: 12px;
    }

    .account-icon {
        width: 32px;
        height: 32px;
    }
}
</style>