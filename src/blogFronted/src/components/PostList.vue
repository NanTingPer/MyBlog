<template>
    <div class="articles-list">
        <div v-for="article in articles" :key="article.id" class="card article-card" @click="goToPost(article.id!)">
            <img v-show="article.drawingUrl != ''" :src="article.drawingUrl" :alt="article.name"
                class="article-image" />
            <div class="article-content">
                <h3 class="article-title">{{ article.title }}</h3>
                <p class="article-description">{{ article.description }}</p>
                <div class="article-footer">
                    <span class="article-date">{{ formatDate(article.createTime!) }}</span>
                    <router-link v-for="tag in article.tag" :key="tag" :to="`/tag/${tag}`" class="article-tag tag" @click.stop>
                        {{ tag }}
                    </router-link>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { useRouter } from 'vue-router';
import type { BlogInfo } from '../ts/types/blogs/BlogInfo';

defineProps<{
    articles: BlogInfo[];
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
</script>

<style scoped>
.articles-list {
    display: flex;
    flex-direction: column;
    gap: 24px;
}

.article-card {
    display: flex;
    overflow: hidden;
    cursor: pointer;
}

.article-image {
    width: 220px;
    height: 180px;
    object-fit: cover;
    flex-shrink: 0;
}

.article-content {
    flex: 1;
    padding: 24px;
    display: flex;
    flex-direction: column;
}

.article-title {
    font-size: 18px;
    font-weight: 600;
    color: #333;
    margin: 0 0 12px;
    line-height: 1.4;
}

.article-description {
    font-size: 14px;
    color: #666;
    line-height: 1.6;
    margin: 0 0 16px;
    flex: 1;
    display: -webkit-box;
    -webkit-box-orient: vertical;
    overflow: hidden;
}

.article-footer {
    display: flex;
    align-items: center;
    gap: 12px;
}

.article-date {
    font-size: 13px;
    color: #999;
}

.article-tag {
    font-size: 12px;
    padding: 4px 10px;
    border-radius: 12px;
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
