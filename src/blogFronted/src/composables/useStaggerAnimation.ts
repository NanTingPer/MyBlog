/**
 * 交错进入动画 Composable
 *
 * 提供 Vue Transition / TransitionGroup 所需的钩子函数，
 * 实现由上往下的透明度渐入 + 滑动效果，每个子元素依次延迟出现。
 *
 * 详细文档见同目录 useStaggerAnimation.md
 */

import type { TransitionProps } from 'vue';

/**
 * 交错动画钩子的返回类型
 */
interface StaggerAnimationHooks {
    /** TransitionGroup / Transition 的 @before-enter 钩子 */
    onBeforeEnter: NonNullable<TransitionProps['onBeforeEnter']>;
    /** TransitionGroup / Transition 的 @enter 钩子 */
    onEnter: NonNullable<TransitionProps['onEnter']>;
}

/**
 * 创建一组用于 Vue `<TransitionGroup>` 或 `<Transition>` 的交错进入动画钩子。
 *
 * 动画效果：每个子元素从透明 + 上方偏移 → 不透明 + 原始位置，依次延迟出现。
 *
 * @param duration  单个元素的动画时长（毫秒），默认 400
 * @param delayStep 每个元素之间的延迟增量（毫秒），默认 80
 * @param offset    元素进入前的垂直偏移量（px），默认 20
 * @returns 包含 onBeforeEnter 和 onEnter 的对象，可直接绑定到 Transition/TransitionGroup
 *
 * @example
 * ```ts
 * import { useStaggerAnimation } from '@/composables/useStaggerAnimation';
 *
 * const { onBeforeEnter, onEnter } = useStaggerAnimation();
 * ```
 */
export function useStaggerAnimation(
    duration: number = 400,
    delayStep: number = 80,
    offset: number = 20
): StaggerAnimationHooks {

    /**
     * 进入前：设置初始状态（完全透明 + 向上偏移）
     */
    const onBeforeEnter: StaggerAnimationHooks['onBeforeEnter'] = (el) => {
        const htmlEl = el as HTMLElement;
        htmlEl.style.opacity = '0';
        htmlEl.style.transform = `translateY(-${offset}px)`;
    };

    /**
     * 进入时：使用 Web Animations API 执行渐入动画
     * 通过 `data-index` 属性读取元素索引，计算交错延迟
     * 动画结束后调用 `done` 回调通知 Vue
     */
    const onEnter: StaggerAnimationHooks['onEnter'] = (el, done) => {
        const htmlEl = el as HTMLElement;
        const index = parseInt(htmlEl.dataset.index || '0', 10);
        const animation = htmlEl.animate(
            [
                { opacity: '0', transform: `translateY(-${offset}px)` },
                { opacity: '1', transform: 'translateY(0)' }
            ],
            {
                duration,
                delay: index * delayStep,
                easing: 'ease',
                fill: 'forwards'
            }
        );
        animation.finished.then(() => {
            // 先设置最终状态的 inline style，防止 cancel 后闪烁
            htmlEl.style.opacity = '1';
            htmlEl.style.transform = 'translateY(0)';
            // 取消 WAAPI 动画
            animation.cancel();
            // 在下一帧移除 inline style，让 CSS 平滑接管
            // 这样 CSS 的 .card:hover { transform: translateY(-2px) } 才能生效
            requestAnimationFrame(() => {
                htmlEl.style.removeProperty('opacity');
                htmlEl.style.removeProperty('transform');
                done();
            });
        });
    };

    return { onBeforeEnter, onEnter };
}