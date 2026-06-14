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

/**
 * 将主题颜色应用到 :root
 * @param colors 主题颜色对象
 */
function applyTheme(colors: ThemeColors) {
    const root = document.documentElement;
    for (const [key, value] of Object.entries(colors)) {
        root.style.setProperty(key, value);
    }
}

/**
 * 移除所有内联 CSS 变量（恢复 variables.css 中的默认值）
 */
function resetTheme() {
    const root = document.documentElement;
    for (const key of Object.keys(themes.light)) {
        root.style.removeProperty(key);
    }
}

/**
 * 主题切换 composable
 *
 * 使用方式：
 * ```ts
 * import { useTheme } from '@/composables/useTheme';
 * const { currentTheme, setTheme, addTheme } = useTheme();
 * setTheme('dark'); // 切换到暗色主题
 * setTheme('light'); // 切换回亮色主题（恢复默认变量）
 * ```
 */
export function useTheme() {
    /**
     * 设置主题
     * @param name 主题名称，'light' 时恢复默认 CSS 变量
     */
    function setTheme(name: string) {
        if (name === 'light') {
            resetTheme();
        } else if (themes[name]) {
            applyTheme(themes[name]);
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

    // 初始化：恢复上次保存的主题
    if (currentTheme.value !== 'light') {
        setTheme(currentTheme.value);
    }

    return {
        currentTheme,
        setTheme,
        addTheme,
        getThemeNames,
    };
}