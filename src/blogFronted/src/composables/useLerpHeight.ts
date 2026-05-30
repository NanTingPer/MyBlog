/**
 * Lerp 高度过渡动画 Composable
 *
 * 核心思路：锁定高度和 overflow，切换内容，然后 Lerp 到目标高度。
 * 元素始终保持在 flex 文档流中，由 flex 自动处理居中，无需绝对定位。
 */

import type { Ref } from 'vue';

interface LerpHeightHooks {
    /** 锁定容器高度和 overflow，为内容切换做准备 */
    lockHeight: (height: number) => void;
    /** 测量目标自然高度并开始 Lerp 动画 */
    lerpToContent: () => void;
}

/** 离屏测量容器（单例，复用） */
let measureContainer: HTMLDivElement | null = null;

function getMeasureContainer(): HTMLDivElement {
    if (!measureContainer) {
        measureContainer = document.createElement('div');
        measureContainer.style.cssText =
            'position:fixed;visibility:hidden;top:-9999px;left:-9999px;pointer-events:none;';
    }
    if (!measureContainer.parentNode) {
        document.body.appendChild(measureContainer);
    }
    return measureContainer;
}

export function useLerpHeight(
    wrapperRef: Ref<HTMLElement | null>,
    factor: number = 0.12
): LerpHeightHooks {
    let animationId: number | null = null;
    let currentHeight: number = 0;
    let targetHeight: number = 0;

    function tick() {
        if (animationId === null) return;

        currentHeight += (targetHeight - currentHeight) * factor;

        const wrapper = wrapperRef.value;
        if (!wrapper) {
            cancel();
            return;
        }

        // 差值足够小，停止动画，清除内联样式
        if (Math.abs(targetHeight - currentHeight) < 0.5) {
            cleanup();
            return;
        }

        // 只设置高度，flex 容器自然处理居中位置
        wrapper.style.height = `${currentHeight}px`;
        animationId = requestAnimationFrame(tick);
    }

    function cancel() {
        if (animationId !== null) {
            cancelAnimationFrame(animationId);
            animationId = null;
        }
    }

    /**
     * 清除所有动画内联样式，恢复正常布局
     */
    function cleanup() {
        cancel();

        const wrapper = wrapperRef.value;
        if (!wrapper) return;

        wrapper.style.height = '';
        wrapper.style.overflow = '';
    }

    /**
     * 锁定容器当前高度和 overflow
     * 在切换内容之前调用，防止内容变化导致容器跳动
     * 元素保持在 flex 文档流中，无需绝对定位
     */
    function lockHeight(height: number) {
        cancel();

        const wrapper = wrapperRef.value;
        if (!wrapper) return;

        wrapper.style.height = `${height}px`;
        wrapper.style.overflow = 'hidden';

        currentHeight = height;
    }

    /**
     * 使用离屏 clone 测量目标自然高度，然后开始 Lerp
     */
    function lerpToContent() {
        cancel();

        const wrapper = wrapperRef.value;
        if (!wrapper) return;

        // clone wrapper 到离屏测量容器
        const container = getMeasureContainer();
        const clone = wrapper.cloneNode(true) as HTMLElement;

        // 重置 clone 的高度约束，让其自然撑开
        clone.style.height = 'auto';
        clone.style.overflow = 'visible';

        // 复制 wrapper 的宽度约束，确保 clone 内容宽度一致
        const computedStyle = getComputedStyle(wrapper);
        clone.style.width = computedStyle.width;
        clone.style.maxWidth = computedStyle.maxWidth;
        clone.style.minWidth = computedStyle.minWidth;
        clone.style.padding = computedStyle.padding;
        clone.style.border = computedStyle.border;
        clone.style.boxSizing = computedStyle.boxSizing;

        container.appendChild(clone);

        // 测量目标高度
        targetHeight = clone.offsetHeight;

        // 清理 clone
        container.removeChild(clone);

        // 开始 Lerp
        animationId = requestAnimationFrame(tick);
    }

    return { lockHeight, lerpToContent };
}