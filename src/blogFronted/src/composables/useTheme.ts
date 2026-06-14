import { ref } from 'vue';

/**
 * 主题颜色定义接口
 * key 为 CSS 变量名（如 '--color-text'），value 为颜色值
 */
export interface ThemeColors {
    [cssVar: string]: string;
}

/**
 * 预设主题集合
 * 可通过 addTheme() 动态添加更多主题
 */
const themes: Record<string, ThemeColors> = {
    light: {
        '--color-primary': '#4caf50',
        '--color-primary-hover': '#43a047',
        '--color-primary-light-bg': '#e8f5e9',
        '--color-primary-light-bg-hover': '#c8e6c9',
        '--color-primary-text': '#2e7d32',
        '--color-danger': '#e53935',
        '--color-danger-hover': '#c62828',
        '--color-danger-light-bg': '#ffebee',
        '--color-danger-light-bg-hover': '#ffcdd2',
        '--color-danger-text': '#d32f2f',
        '--color-info': '#1976d2',
        '--color-info-light-bg': '#e3f2fd',
        '--color-info-light-bg-hover': '#bbdefb',
        '--color-success': '#22c55e',
        '--color-success-light-bg': '#f0fdf4',
        '--color-text': '#333',
        '--color-text-secondary': '#666',
        '--color-text-muted': '#999',
        '--color-text-light': '#bbb',
        '--color-text-white': '#fff',
        '--color-bg-white': '#fff',
        '--color-bg-warm': '#faf8f5',
        '--color-bg-warm-hover': '#f0ede8',
        '--color-bg-light': '#f8f9fa',
        '--color-bg-lighter': '#fafafa',
        '--color-bg-hover': '#f5f5f5',
        '--color-border': '#e8e8e8',
        '--color-border-light': '#f0f0f0',
        '--shadow-sm': '0 1px 3px rgba(0, 0, 0, 0.06)',
        '--shadow-md': '0 2px 8px rgba(0, 0, 0, 0.06)',
        '--shadow-lg': '0 4px 16px rgba(0, 0, 0, 0.1)',
    },
    dark: {
        '--color-primary': '#66bb6a',
        '--color-primary-hover': '#81c784',
        '--color-primary-light-bg': '#1b3a1d',
        '--color-primary-light-bg-hover': '#2e5a30',
        '--color-primary-text': '#a5d6a7',
        '--color-danger': '#ef5350',
        '--color-danger-hover': '#e57373',
        '--color-danger-light-bg': '#3e1a1a',
        '--color-danger-light-bg-hover': '#5a2a2a',
        '--color-danger-text': '#ef9a9a',
        '--color-info': '#42a5f5',
        '--color-info-light-bg': '#0d2137',
        '--color-info-light-bg-hover': '#173a5a',
        '--color-success': '#66bb6a',
        '--color-success-light-bg': '#1b3a1d',
        '--color-text': '#e0e0e0',
        '--color-text-secondary': '#aaa',
        '--color-text-muted': '#888',
        '--color-text-light': '#666',
        '--color-text-white': '#fff',
        '--color-bg-white': '#1e1e1e',
        '--color-bg-warm': '#181818',
        '--color-bg-warm-hover': '#2a2a2a',
        '--color-bg-light': '#252525',
        '--color-bg-lighter': '#2a2a2a',
        '--color-bg-hover': '#333',
        '--color-border': '#3a3a3a',
        '--color-border-light': '#2e2e2e',
        '--shadow-sm': '0 1px 3px rgba(0, 0, 0, 0.2)',
        '--shadow-md': '0 2px 8px rgba(0, 0, 0, 0.3)',
        '--shadow-lg': '0 4px 16px rgba(0, 0, 0, 0.4)',
    },
};

/** 当前主题名称 */
const currentTheme = ref<string>(
    localStorage.getItem('theme') || 'light'
);

// ==================== 动画引擎 ====================

/** 动画时长（毫秒） */
const ANIMATION_DURATION = 400;
/** 当前 rAF id，用于中断取消 */
let animationId: number | null = null;

/**
 * 创建 cubic-bezier 缓动函数
 * 使用二分法求解贝塞尔曲线参数 t，确保精度和稳定性
 *
 * @param p1x 控制点 1 的 x 坐标
 * @param p1y 控制点 1 的 y 坐标
 * @param p2x 控制点 2 的 x 坐标
 * @param p2y 控制点 2 的 y 坐标
 * @returns 缓动函数 (x: number) => number
 */
function createEasing(
    p1x: number,
    p1y: number,
    p2x: number,
    p2y: number
): (x: number) => number {
    return function (x: number): number {
        if (x <= 0) return 0;
        if (x >= 1) return 1;

        let lo = 0;
        let hi = 1;
        let t = x;

        for (let i = 0; i < 20; i++) {
            const mt = 1 - t;
            const currentX =
                3 * mt * mt * t * p1x +
                3 * mt * t * t * p2x +
                t * t * t;
            if (Math.abs(currentX - x) < 1e-7) break;
            if (currentX < x) {
                lo = t;
            } else {
                hi = t;
            }
            t = (lo + hi) / 2;
        }

        const mt = 1 - t;
        return (
            3 * mt * mt * t * p1y +
            3 * mt * t * t * p2y +
            t * t * t
        );
    };
}

/** 非线性缓动：cubic-bezier(0.4, 0, 0.2, 1) — Material Design 标准曲线 */
const easeInOut = createEasing(0.4, 0, 0.2, 1);

/**
 * 解析颜色字符串为 [r, g, b, a] 数值数组
 * 支持格式：#hex3、#hex6、#hex8、rgb()、rgba()
 */
function parseColor(color: string): [number, number, number, number] {
    color = color.trim();

    if (color.startsWith('#')) {
        const hex = color.slice(1);
        if (hex.length === 3) {
            return [
                parseInt(hex[0] + hex[0], 16),
                parseInt(hex[1] + hex[1], 16),
                parseInt(hex[2] + hex[2], 16),
                1,
            ];
        }
        if (hex.length === 6) {
            return [
                parseInt(hex.slice(0, 2), 16),
                parseInt(hex.slice(2, 4), 16),
                parseInt(hex.slice(4, 6), 16),
                1,
            ];
        }
        if (hex.length === 8) {
            return [
                parseInt(hex.slice(0, 2), 16),
                parseInt(hex.slice(2, 4), 16),
                parseInt(hex.slice(4, 6), 16),
                parseInt(hex.slice(6, 8), 16) / 255,
            ];
        }
    }

    const match = color.match(/rgba?\(([^)]+)\)/);
    if (match) {
        const parts = match[1].split(',').map((s) => parseFloat(s.trim()));
        return [parts[0], parts[1], parts[2], parts[3] ?? 1];
    }

    return [0, 0, 0, 1];
}

/**
 * 线性插值两个颜色值，返回 rgba 字符串
 */
function lerpColor(from: string, to: string, t: number): string {
    const [r1, g1, b1, a1] = parseColor(from);
    const [r2, g2, b2, a2] = parseColor(to);
    const r = Math.round(r1 + (r2 - r1) * t);
    const g = Math.round(g1 + (g2 - g1) * t);
    const b = Math.round(b1 + (b2 - b1) * t);
    const a = a1 + (a2 - a1) * t;
    return `rgba(${r}, ${g}, ${b}, ${a.toFixed(2)})`;
}

/**
 * 解析 box-shadow 值，分离前缀（偏移/模糊）和 rgba 颜色
 *
 * 例如 '0 2px 8px rgba(0, 0, 0, 0.06)'
 *   → { prefix: '0 2px 8px ', color: [0, 0, 0, 0.06] }
 */
function parseShadow(shadow: string): {
    prefix: string;
    color: [number, number, number, number];
} {
    shadow = shadow.trim();
    const rgbaMatch = shadow.match(/rgba?\([^)]+\)/);
    if (!rgbaMatch) {
        return { prefix: shadow, color: [0, 0, 0, 0] };
    }
    const prefix = shadow.substring(0, shadow.indexOf(rgbaMatch[0]));
    const color = parseColor(rgbaMatch[0]);
    return { prefix, color };
}

/**
 * 线性插值两个 box-shadow 值
 * 偏移/模糊部分取目标值，仅对 rgba 颜色部分做插值
 */
function lerpShadow(from: string, to: string, t: number): string {
    const fromParsed = parseShadow(from);
    const toParsed = parseShadow(to);
    const r = Math.round(
        fromParsed.color[0] + (toParsed.color[0] - fromParsed.color[0]) * t
    );
    const g = Math.round(
        fromParsed.color[1] + (toParsed.color[1] - fromParsed.color[1]) * t
    );
    const b = Math.round(
        fromParsed.color[2] + (toParsed.color[2] - fromParsed.color[2]) * t
    );
    const a =
        fromParsed.color[3] + (toParsed.color[3] - fromParsed.color[3]) * t;
    return `${toParsed.prefix}rgba(${r}, ${g}, ${b}, ${a.toFixed(2)})`;
}

// ==================== 核心方法 ====================

/**
 * 将主题颜色应用到 :root，支持非线性动画过渡
 *
 * 动画过程中如果再次调用，会从当前中间值平滑过渡到新目标，
 * 通过 getComputedStyle 读取实时值实现中断续接。
 *
 * @param colors 目标主题颜色
 * @param animate 是否使用动画过渡（默认 true）
 */
function applyTheme(colors: ThemeColors, animate = true) {
    const root = document.documentElement;

    // 取消正在进行的动画
    if (animationId !== null) {
        cancelAnimationFrame(animationId);
        animationId = null;
    }

    // 预分类：颜色变量 vs 阴影变量
    const colorKeys: string[] = [];
    const shadowKeys: string[] = [];
    for (const key of Object.keys(colors)) {
        if (key.startsWith('--shadow')) {
            shadowKeys.push(key);
        } else {
            colorKeys.push(key);
        }
    }

    if (!animate) {
        // 直接应用，不走动画
        for (const [key, value] of Object.entries(colors)) {
            root.style.setProperty(key, value);
        }
        return;
    }

    // 读取当前值作为动画起点（中断时从中间状态续接）
    const computed = getComputedStyle(root);
    const fromValues: Record<string, string> = {};
    const toValues: Record<string, string> = {};

    for (const [key, value] of Object.entries(colors)) {
        const current =
            root.style.getPropertyValue(key) ||
            computed.getPropertyValue(key).trim();
        fromValues[key] = current || value;
        toValues[key] = value;
    }

    // rAF 动画循环
    const startTime = performance.now();

    function tick(now: number) {
        const elapsed = now - startTime;
        const rawProgress = Math.min(elapsed / ANIMATION_DURATION, 1);
        const t = easeInOut(rawProgress);

        // 颜色变量插值
        for (const key of colorKeys) {
            root.style.setProperty(
                key,
                lerpColor(fromValues[key], toValues[key], t)
            );
        }

        // 阴影变量插值
        for (const key of shadowKeys) {
            root.style.setProperty(
                key,
                lerpShadow(fromValues[key], toValues[key], t)
            );
        }

        if (rawProgress < 1) {
            animationId = requestAnimationFrame(tick);
        } else {
            animationId = null;
        }
    }

    animationId = requestAnimationFrame(tick);
}

/**
 * 主题切换 composable
 *
 * 使用方式：
 * ```ts
 * import { useTheme } from '@/composables/useTheme';
 * const { currentTheme, setTheme, addTheme } = useTheme();
 * setTheme('dark');  // 切换到暗色主题（带动画）
 * setTheme('light'); // 切换回亮色主题（带动画）
 * setTheme('dark', false); // 立即切换，不使用动画
 * ```
 */
export function useTheme() {
    /**
     * 设置主题
     * @param name 主题名称
     * @param animate 是否使用非线性动画过渡（默认 true）
     */
    function setTheme(name: string, animate = true) {
        if (name === 'light') {
            applyTheme(themes.light, animate);
        } else if (themes[name]) {
            applyTheme(themes[name], animate);
        } else {
            console.warn(`[useTheme] 未知主题: ${name}`);
            return;
        }
        currentTheme.value = name;
        localStorage.setItem('theme', name);
    }

    /**
     * 动态添加/覆盖主题
     * @param name 主题名称
     * @param colors 主题颜色对象
     */
    function addTheme(name: string, colors: ThemeColors) {
        themes[name] = colors;
    }

    /**
     * 获取所有可用主题名称
     */
    function getThemeNames(): string[] {
        return Object.keys(themes);
    }

    // 初始化：恢复上次保存的主题（跳过动画，避免页面加载时闪烁）
    if (currentTheme.value !== 'light') {
        setTheme(currentTheme.value, false);
    }

    return {
        currentTheme,
        setTheme,
        addTheme,
        getThemeNames,
    };
}