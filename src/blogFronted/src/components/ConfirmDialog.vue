<!--
  ConfirmDialog.vue - 确认对话框组件

  功能说明：
    替代原生 window.confirm() 的 Vue 确认对话框。
    支持遮罩层、标题、内容、确认/取消按钮自定义、loading 状态、危险操作样式。

  Props：
    - visible: boolean    必需，v-model:visible 控制对话框显隐
    - title: string       可选，对话框标题（默认"确认操作"）
    - content: string     可选，对话框内容文本（默认"确定要执行此操作吗？"）
    - confirmText: string 可选，确认按钮文案（默认"确定"）
    - cancelText: string  可选，取消按钮文案（默认"取消"）
    - danger: boolean     可选，是否为危险操作（红色确认按钮，默认 false）
    - loading: boolean    可选，确认按钮 loading 状态（默认 false）

  Events：
    - update:visible  v-model:visible 双向绑定
    - confirm         点击确认按钮时触发
    - cancel          点击取消按钮或遮罩层时触发

  使用示例见 ConfirmDialog.md
-->
<template>
    <Teleport to="body">
        <Transition name="dialog-fade">
            <div v-if="visible" class="dialog-overlay" @click.self="handleCancel">
                <div class="dialog-box">
                    <!-- 标题 -->
                    <h3 class="dialog-title">{{ title }}</h3>
                    <!-- 内容 -->
                    <p class="dialog-content">{{ content }}</p>
                    <!-- 操作按钮 -->
                    <div class="dialog-actions">
                        <button
                            class="dialog-btn dialog-btn-cancel"
                            @click="handleCancel"
                            :disabled="loading"
                        >
                            {{ cancelText }}
                        </button>
                        <button
                            class="dialog-btn"
                            :class="danger ? 'dialog-btn-danger' : 'dialog-btn-confirm'"
                            @click="handleConfirm"
                            :disabled="loading"
                        >
                            {{ loading ? '处理中...' : confirmText }}
                        </button>
                    </div>
                </div>
            </div>
        </Transition>
    </Teleport>
</template>

<script setup lang="ts">
const props = withDefaults(defineProps<{
    /** 是否显示对话框 */
    visible: boolean;
    /** 对话框标题 */
    title?: string;
    /** 对话框内容文本 */
    content?: string;
    /** 确认按钮文案 */
    confirmText?: string;
    /** 取消按钮文案 */
    cancelText?: string;
    /** 是否为危险操作（红色确认按钮） */
    danger?: boolean;
    /** 确认按钮 loading 状态 */
    loading?: boolean;
}>(), {
    title: '确认操作',
    content: '确定要执行此操作吗？',
    confirmText: '确定',
    cancelText: '取消',
    danger: false,
    loading: false,
});

const emit = defineEmits<{
    'update:visible': [value: boolean];
    'confirm': [];
    'cancel': [];
}>();

/**
 * 关闭对话框并更新 visible
 */
function close() {
    emit('update:visible', false);
}

/**
 * 确认操作，触发 confirm 事件
 * 父组件可通过 loading prop 控制按钮禁用状态
 */
function handleConfirm() {
    emit('confirm');
}

/**
 * 取消操作，关闭对话框并触发 cancel 事件
 */
function handleCancel() {
    close();
    emit('cancel');
}
</script>

<style scoped>
/* ===== ConfirmDialog 样式 ===== */
/* 颜色使用 admin.css :root 中定义的 CSS 变量 */

/* 遮罩层 */
.dialog-overlay {
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background: rgba(0, 0, 0, 0.5);
    display: flex;
    align-items: center;
    justify-content: center;
    z-index: 2000;
}

/* 对话框容器 */
.dialog-box {
    background: var(--color-bg-white);
    border-radius: 12px;
    padding: 28px 24px 20px;
    width: 90%;
    max-width: 400px;
    box-shadow: 0 12px 32px rgba(0, 0, 0, 0.15);
}

/* 标题 */
.dialog-title {
    font-size: 18px;
    font-weight: 600;
    color: var(--color-text);
    margin: 0 0 12px 0;
}

/* 内容 */
.dialog-content {
    font-size: 14px;
    color: var(--color-text-secondary);
    margin: 0 0 24px 0;
    line-height: 1.6;
}

/* 按钮区域 */
.dialog-actions {
    display: flex;
    justify-content: flex-end;
    gap: 12px;
}

/* 按钮基础样式 */
.dialog-btn {
    padding: 10px 24px;
    border-radius: 6px;
    font-size: 14px;
    cursor: pointer;
    border: none;
    transition: all 0.2s;
}

.dialog-btn:disabled {
    opacity: 0.6;
    cursor: not-allowed;
}

/* 取消按钮 */
.dialog-btn-cancel {
    background: var(--color-bg-white);
    color: var(--color-text-secondary);
    border: 1px solid var(--color-border);
}

.dialog-btn-cancel:hover:not(:disabled) {
    background: var(--color-bg-hover);
}

/* 确认按钮（默认绿色） */
.dialog-btn-confirm {
    background: var(--color-primary);
    color: var(--color-text-white);
}

.dialog-btn-confirm:hover:not(:disabled) {
    background: var(--color-primary-hover);
}

/* 确认按钮（危险操作红色） */
.dialog-btn-danger {
    background: var(--color-danger);
    color: var(--color-text-white);
}

.dialog-btn-danger:hover:not(:disabled) {
    background: var(--color-danger-hover);
}

/* 过渡动画 */
.dialog-fade-enter-active,
.dialog-fade-leave-active {
    transition: opacity 0.2s ease;
}

.dialog-fade-enter-active .dialog-box,
.dialog-fade-leave-active .dialog-box {
    transition: transform 0.2s ease;
}

.dialog-fade-enter-from,
.dialog-fade-leave-to {
    opacity: 0;
}

.dialog-fade-enter-from .dialog-box {
    transform: scale(0.95) translateY(-10px);
}

.dialog-fade-leave-to .dialog-box {
    transform: scale(0.95) translateY(-10px);
}

@media (max-width: 768px) {
    .dialog-box {
        width: 85%;
        max-width: 320px;
        padding: 24px 20px 16px;
    }

    .dialog-title {
        font-size: 16px;
    }

    .dialog-content {
        font-size: 13px;
    }

    .dialog-btn {
        flex: 1;
        padding: 12px;
        font-size: 14px;
    }
}
</style>