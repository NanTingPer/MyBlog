<template>
    <Transition appear @before-enter="onBeforeEnter" @enter="onEnter">
        <div id="content" class="post-container">
            <div v-html="content"></div>
        </div>
    </Transition>
</template>
<script setup lang="ts">
import { ref, onMounted, nextTick } from 'vue';
import { useRoute } from 'vue-router';
import { apiFetch } from '../../ts/config/apiConfig';
import { useStaggerAnimation } from '../../composables/useStaggerAnimation';

const { onBeforeEnter, onEnter } = useStaggerAnimation();
const route = useRoute();
const content = ref('');

onMounted(async () => {
    const id = route.params.id as string;
    try {
        apiFetch(`/api/blog/postHTML?id=${id}`, {
            method: "GET"
        }).then(response => {
            if (response.status != 200)
                return;

            response.json().then(json => {
                content.value = json.data;
                nextTick(() => {
                    let inter = setInterval(() => {
                        let window_ = (window as any);
                        if (window_.randerMathInElement && window_.Prism) {
                            eval(`
                            randerMathInElement(document.getElementById('content'), 
                            { 
                                strict: false,
                                preProcess: (math) => {
                                    if (math.includes('&') && !math.includes('\\\\begin{')) {
                                        return '\\\\begin{aligned}' + math + '\\\\end{aligned}';
                                    }
                                    return math;
                                }
                            })`);
                            eval('Prism.highlightAll();')
                            clearTimeout(inter);
                        }
                    }, 20);

                });
            })
        });
    } catch (error) {
        console.error('Failed to fetch post:', error);
        content.value = '<p>获取文章内容失败</p>';
    }
});
</script>

<style scoped>
.post-container {
    max-width: 80%;
    margin: 0 auto;
    padding: 40px 20px;
    background: var(--color-bg-white);
    border-radius: 16px;
    box-shadow: var(--shadow-md);
}
</style>

<!-- 非 scoped：覆盖 v-html 渲染的服务端 HTML 默认黑色文字 -->
<style>
#content {
    color: var(--color-text);
}

#content a {
    color: var(--color-primary);
}

#content a:hover {
    color: var(--color-primary-hover);
}

#content blockquote {
    border-left: 4px solid var(--color-border);
    color: var(--color-text-secondary);
}

#content hr {
    border-color: var(--color-border);
}

#content table th {
    background: var(--color-bg-light);
}

#content table th,
#content table td {
    border-color: var(--color-border);
}

/* Prism 代码块主题适配 */
#content pre[class*="language-"],
#content code[class*="language-"] {
    color: var(--color-text);
    text-shadow: none;
    background: var(--color-bg-light);
}

#content pre[class*="language-"],
#content :not(pre) > code[class*="language-"] {
    background: var(--color-bg-light);
}

/* 修复 token.operator 等半透明背景色在暗色主题下不可视 */
#content .token.operator {
    background: transparent;
}

/* 隐藏 code 块滚动条，保留滚动能力（Shift+滚轮可横向滚动） */
#content pre[class*="language-"] {
    scrollbar-width: none;
}

#content pre[class*="language-"]::-webkit-scrollbar {
    display: none;
}
</style>
