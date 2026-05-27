<template>
    <div class="articles-list">
        <div v-if="loading" class="flex items-center justify-center py-20">
            <div class="loading-spinner"></div>
        </div>
        <TransitionGroup v-else name="post-list" tag="div" class="flex flex-col gap-6"
            appear
            @before-enter="onBeforeEnter" @enter="onEnter">
            <div v-for="(article, index) in articles" :key="article.id" :data-index="index"
                class="card article-card cursor-pointer" @click="goToPost(article.id!)">
                <img v-show="article.drawingUrl != ''" :src="article.drawingUrl" :alt="article.name"
                    class="article-image" />
                <div class="article-content">
                    <h3 class="article-title">{{ article.title }}</h3>
                    <p class="article-description">{{ article.description }}</p>
                    <div class="article-footer">
                        <span class="article-date">{{ formatDate(article.createTime!) }}</span>
                        <router-link v-for="tag in article.tag" :key="tag" :to="`/tag/${tag}`" class="article-tag tag"
                            @click.stop>
                            {{ tag }}
                        </router-link>
                    </div>
                </div>
            </div>
        </TransitionGroup>
    </div>
</template>

<script setup lang="ts">
import { useRouter } from 'vue-router';
import type { BlogInfo } from '../ts/types/blogs/BlogInfo';

defineProps<{
    articles: BlogInfo[];
    loading?: boolean;
}>();

const router = useRouter();

const formatDate = (timestamp: number): string => {
    let ms = Number(timestamp.toString().substring(0, 13));
    const date = new Date(ms);
    if (isNaN(date.getTime()) || date.getFullYear() < 2000 || date.getFullYear() > 2100) {
        return '未知日期';
    }
    return `${date.getFullYear()}年${date.getMonth() + 1}月${date.getDate()}日`;
};

const goToPost = (id: string) => {
    router.push(`/post/${id}`);
};

/**
 * 每个列表项进入前设置初始状态和延迟
 */
const onBeforeEnter = (el: Element) => {
    const htmlEl = el as HTMLElement;
    htmlEl.style.opacity = '0';
    htmlEl.style.transform = 'translateY(-20px)';
    const index = parseInt(htmlEl.dataset.index || '0', 10);
    htmlEl.style.transitionDelay = `${index * 80}ms`;
};

/**
 * 每个列表项进入时触发过渡
 */
const onEnter = (el: Element, done: () => void) => {
    const htmlEl = el as HTMLElement;
    // 强制 reflow，确保 onBeforeEnter 设置的初始样式被应用
    htmlEl.offsetHeight;
    // 使用 requestAnimationFrame 确保浏览器完成初始渲染后再设置结束状态
    requestAnimationFrame(() => {
        htmlEl.style.transition = 'opacity 0.4s ease, transform 0.4s ease';
        htmlEl.style.opacity = '1';
        htmlEl.style.transform = 'translateY(0)';
        // 将 done 回调绑定到 transitionend 事件
        htmlEl.ontransitionend = done;
    });
};
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

.cursor-pointer {
    cursor: pointer;
}

/* ========== 加载旋转动画 ========== */
.loading-spinner {
    width: 2.5rem;
    height: 2.5rem;
    border: 3px solid #e0e0e0;
    border-top-color: #4caf50;
    border-radius: 50%;
    animation: spin 0.8s linear infinite;
}

/* 复杂动画关键帧 - 旋转加载指示器 */
@keyframes spin {
    to {
        transform: rotate(360deg);
    }
}

/* ========== 文章卡片样式（组件独有） ========== */
.articles-list {
    display: flex;
    flex-direction: column;
}

.article-card {
    display: flex;
    overflow: hidden;
}

.article-image {
    width: 220px;
    height: 180px;
    object-fit: cover;
    flex-shrink: 0;
}

.article-content {
    flex: 1;
    padding: 1.5rem;
    display: flex;
    flex-direction: column;
}

.article-title {
    font-size: 1.125rem;
    font-weight: 600;
    color: #333;
    margin: 0 0 0.75rem;
    line-height: 1.4;
}

.article-description {
    font-size: 0.875rem;
    color: #666;
    line-height: 1.6;
    margin: 0 0 1rem;
    flex: 1;
    display: -webkit-box;
    -webkit-box-orient: vertical;
    overflow: hidden;
}

.article-footer {
    display: flex;
    align-items: center;
    gap: 0.75rem;
}

.article-date {
    font-size: 0.8125rem;
    color: #999;
}

.article-tag {
    font-size: 0.75rem;
    padding: 0.25rem 0.625rem;
    border-radius: 0.75rem;
    text-decoration: none;
}

.tag {
    background: #e8f5e9;
    color: #000000;
}

@media (max-width: 768px) {
    .article-card {
        flex-direction: column;
    }

    .article-image {
        width: 100%;
        height: 180px;
    }
}
</style>