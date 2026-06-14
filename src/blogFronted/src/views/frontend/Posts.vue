<template>
    <div class="articles-page page-container">
        <h2 class="section-title">文章</h2>
        <PostList :articles="articles" :loading="loading" />
        <div class="pagination" v-if="totalPages > 1">
            <button class="page-btn" :disabled="currentPage <= 1" @click="changePage(currentPage - 1)">
                上一页
            </button>
            <span class="page-info">{{ currentPage }} / {{ totalPages }}</span>
            <button class="page-btn" :disabled="currentPage >= totalPages" @click="changePage(currentPage + 1)">
                下一页
            </button>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from "vue";
import type { BlogInfo } from "../../ts/types/blogs/BlogInfo";
import { BlogAPI } from "../../ts/utils/BlogAPI";
import { API_BASE_URL } from "../../ts/config/apiConfig";
import PostList from "../../components/PostList.vue";

const api = new BlogAPI(API_BASE_URL);

const articles = ref<BlogInfo[]>([]);
const totalPages = ref(0);
const currentPage = ref(1);
const loading = ref(true);
const limit = 5;

/**
 * 获取给定页面的数据
 * @param page 给定页
 */
const fetchArticles = async (page: number) => {
    loading.value = true;
    try {
        const response = await api.GetArticlesToPage(limit, page);
        const data = await response.json();
        articles.value = data.data || [];
    } catch (error) {
        articles.value = [];
    } finally {
        loading.value = false;
    }
};

/**
 * 更改页数信息到给定page, 并获取给定page页的数据
 * @param page 第几页
 */
const changePage = async (page: number) => {
    if (page < 1 || page > totalPages.value) return;
    currentPage.value = page;
    await fetchArticles(page);
};

/**
 * 初始化页数，并获取一页的文章
 */
onMounted(async () => {
    try {
        const response = await api.GetPageCount(limit);
        const data = await response.json();
        totalPages.value = data.data || 0;
    } catch (error) {
        totalPages.value = 0;
    }
    await fetchArticles(1);
});
</script>

<style scoped>
.articles-page {
    max-width: 900px;
}

.pagination {
    display: flex;
    justify-content: center;
    align-items: center;
    gap: 16px;
    margin-top: 40px;
    padding-bottom: 20px;
}

.page-btn {
    padding: 8px 16px;
    border: 1px solid var(--color-border);
    border-radius: 8px;
    background: var(--color-bg-white);
    color: var(--color-text-secondary);
    font-size: 14px;
    cursor: pointer;
    transition: all 0.2s;
}

.page-btn:hover:not(:disabled) {
    background: var(--color-bg-light);
    color: var(--color-text);
}

.page-btn:disabled {
    opacity: 0.5;
    cursor: not-allowed;
}

.page-info {
    font-size: 14px;
    color: var(--color-text-muted);
}
</style>
