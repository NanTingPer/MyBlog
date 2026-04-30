<template>
    <div class="latest-articles">
        <h2 class="section-title">最新文章</h2>
        <ArticleList :articles="articles" />
    </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from "vue";
import type { BlogInfo } from "../ts/types/blogs/BlogInfo";
import { BlogAPI } from "../ts/utils/BlogAPI";
import { API_BASE_URL } from "../ts/config/apiConfig";
import ArticleList from "./ArticleList.vue";

const articles = ref<BlogInfo[]>([]);

onMounted(async () => {
    try {
        const api = new BlogAPI(API_BASE_URL);
        const response = await api.Search();
        const data = await response.json();
        articles.value = data.data || [];
    } catch (error) {
        console.error("Failed to fetch articles:", error);
        articles.value = [
            {
                id: "2",
                name: "设计中的留白，是一种温柔的呼吸",
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
</style>
