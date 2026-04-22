<template>
    <div class="latest-articles">
        <h2 class="section-title">最新文章</h2>
        <div class="articles-list">
            <div v-for="article in articles" :key="article.id" class="article-card">
                <img v-show="article.drawingUrl != ''" :src="article.drawingUrl" :alt="article.name" class="article-image" />
                <div class="article-content">
                    <h3 class="article-title">{{ article.name }}</h3>
                    <p class="article-description">{{ article.content.substring(0, 90) + '...' }}</p>
                    <div class="article-footer">
                        <span class="article-date">{{ formatDate(article.createTime) }}</span>
                        <span v-for="tag in article.tag" :key="tag" class="article-tag tag">
                            {{ tag }}
                        </span>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import type { BlogInfo } from '../ts/types/blogs/BlogInfo';
import { BlogAPI } from '../ts/utils/BlogAPI';

const articles = ref<BlogInfo[]>([]);

const formatDate = (timestamp: number): string => {
    let ms = Number(timestamp.toString().substring(0, 13));
    const date = new Date(ms);
    if (isNaN(date.getTime()) || date.getFullYear() < 2000 || date.getFullYear() > 2100) {
        return '未知日期';
    }
    return `${date.getFullYear()}年${date.getMonth() + 1}月${date.getDate()}日`;
};

onMounted(async () => {
    try {
        const api = new BlogAPI('http://localhost:5162');
        const response = await api.Search();
        const data = await response.json();
        articles.value = data.data || [];
    } catch (error) {
        console.error('Failed to fetch articles:', error);
        articles.value = [
            {
                id: '2',
                name: '设计中的留白，是一种温柔的呼吸',
                createTime: Date.now() - 86400000,
                editTime: Date.now() - 86400000,
                author: ['作者'],
                content: '好的设计从来不是把每一寸空间都填满，而是懂得什么时候该留白。就像呼吸一样，有呼才有吸，有留白才有焦点，有克制才有高级感。',
                tag: ['设计'],
                drawingUrl: 'https://images.unsplash.com/photo-1459749411175-04bf5292ceea?w=300&h=200&fit=crop'
            }
        ];
    }
});
</script>

<style scoped>
.latest-articles {
    width: 100%;
    max-width: 900px;
    margin: 0 auto;
    padding: 40px 20px;
}

.section-title {
    text-align: center;
    font-size: 24px;
    font-weight: 600;
    color: #333;
    margin-bottom: 32px;
}

.articles-list {
    display: flex;
    flex-direction: column;
    gap: 24px;
}

.article-card {
    display: flex;
    background: #fff;
    border-radius: 16px;
    overflow: hidden;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
    transition: transform 0.2s, box-shadow 0.2s;
}

.article-card:hover {
    transform: translateY(-2px);
    box-shadow: 0 4px 16px rgba(0, 0, 0, 0.1);
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
