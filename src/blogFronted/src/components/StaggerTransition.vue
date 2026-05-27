<!--
  StaggerTransition - 交错进入动画包装组件

  封装 Vue <TransitionGroup> + 交错动画逻辑，提供加载旋转圈和列表渐入动画。
  详细文档见 src/composables/useStaggerAnimation.md
-->
<template>
    <!-- 加载中：显示旋转圆圈 -->
    <div v-if="loading" class="flex items-center justify-center py-20">
        <div class="stagger-spinner"></div>
    </div>
    <!-- 加载完成：使用 TransitionGroup 展示交错进入动画 -->
    <TransitionGroup v-else appear tag="div" class="stagger-group flex flex-col gap-6"
        @before-enter="onBeforeEnter" @enter="onEnter">
        <slot></slot>
    </TransitionGroup>
</template>

<script setup lang="ts">
import { useStaggerAnimation } from '../composables/useStaggerAnimation';

/**
 * 组件 Props
 */
const props = withDefaults(defineProps<{
    /** 是否处于加载状态，为 true 时显示旋转圈，为 false 时显示列表动画 */
    loading?: boolean;
    /** 单个元素的动画时长（毫秒） */
    duration?: number;
    /** 每个元素之间的延迟增量（毫秒） */
    delayStep?: number;
    /** 元素进入前的垂直偏移量（px） */
    offset?: number;
}>(), {
    loading: false,
    duration: 400,
    delayStep: 80,
    offset: 20
});

/** 从 composable 获取交错动画钩子 */
const { onBeforeEnter, onEnter } = useStaggerAnimation(
    props.duration,
    props.delayStep,
    props.offset
);
</script>

<style scoped>
/* ========== 原子化工具类 ========== */
.flex {
    display: flex;
}

.flex-col {
    flex-direction: column;
}

.items-center {
    align-items: center;
}

.justify-center {
    justify-content: center;
}

.py-20 {
    padding-top: 5rem;
    padding-bottom: 5rem;
}

.gap-6 {
    gap: 1.5rem;
}

/* ========== 交错动画容器 ========== */
.stagger-group {
    display: flex;
    flex-direction: column;
}

/* ========== 加载旋转动画 ========== */
.stagger-spinner {
    width: 2.5rem;
    height: 2.5rem;
    border: 3px solid #e0e0e0;
    border-top-color: #4caf50;
    border-radius: 50%;
    animation: stagger-spin 0.8s linear infinite;
}

/* 复杂动画关键帧 - 旋转加载指示器 */
@keyframes stagger-spin {
    to {
        transform: rotate(360deg);
    }
}
</style>