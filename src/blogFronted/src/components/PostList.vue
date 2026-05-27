<!--
  PostList - 文章列表组件

  使用 StaggerTransition 提供加载旋转圈和列表交错进入动画。
  动画细节见 src/composables/useStaggerAnimation.md
-->
<template>
    <StaggerTransition :loading="loading">
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
    </StaggerTransition>
</template>

<script setup lang="ts">
import { useRouter } from 'vue-router';
import type { BlogInfo } from '../ts/types/blogs/BlogInfo';
import StaggerTransition from './StaggerTransition.vue';

/**
 * 组件 Props
 */
defineProps<{
    /** 文章列表数据 */
    articles: BlogInfo[];
    /** 是否处于加载状态 */
    loading?: boolean;
}>();

const router = useRouter();

/**
 * 将 Unix 时间戳格式化为中文日期字符串
 * @param timestamp 时间戳（支持秒级和毫秒级）
 * @returns 格式化后的日期字符串，如 "2024年1月1日"
 */
const formatDate = (timestamp: number): string => {
    let ms = Number(timestamp.toString().substring(0, 13));
    const date = new Date(ms);
    if (isNaN(date.getTime()) || date.getFullYear() < 2000 || date.getFullYear() > 2100) {
        return '未知日期';
    }
    return `${date.getFullYear()}年${date.getMonth() + 1}月${date.getDate()}日`;
};

/**
 * 跳转到文章详情页
 * @param id 文章 ID
 */
const goToPost = (id: string) => {
    router.push(`/post/${id}`);
};
</script>

<style scoped>
/* ========== 原子化工具类 ========== */
.cursor-pointer {
    cursor: pointer;
}

/* ========== 文章卡片样式 ========== */
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