<template>
    <div class="latest-articles page-container">
        <h2 class="section-title">最新文章</h2>
        <PostList :articles="articles" />
    </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from "vue";
import type { BlogInfo } from "../ts/types/blogs/BlogInfo";
import { BlogAPI } from "../ts/utils/BlogAPI";
import { API_BASE_URL } from "../ts/config/apiConfig";
import PostList from "./PostList.vue";

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
        ];
    }
});
</script>

<style scoped>
.latest-articles {
    max-width: 900px;
}
</style>
