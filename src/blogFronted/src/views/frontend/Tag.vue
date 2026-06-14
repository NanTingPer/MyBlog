<template>
    <div class="tag-page page-container">
        <h2 class="section-title">{{ tagName }}</h2>
        <div class="tag-layout">
            <div class="tag-main">
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
                <div v-if="articles.length === 0 && !loading" class="empty-tip">
                    该标签下暂无文章
                </div>
            </div>
            <aside class="tag-sidebar">
                <div class="tag-sidebar-card">
                    <h3 class="tag-sidebar-title">全部标签</h3>
                    <div class="tag-list">
                        <router-link v-for="t in allTags" :key="t" :to="`/tag/${t}`" class="tag-item tag">
                            {{ t }}
                        </router-link>
                    </div>
                </div>
            </aside>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, watch, onMounted } from 'vue';
import { useRoute } from 'vue-router';
import type { BlogInfo } from '../../ts/types/blogs/BlogInfo';
import { BlogAPI } from '../../ts/utils/BlogAPI';
import { API_BASE_URL } from '../../ts/config/apiConfig';
import PostList from '../../components/PostList.vue';

const route = useRoute();
const api = new BlogAPI(API_BASE_URL);

const tagName = ref('');
const articles = ref<BlogInfo[]>([]);
const allTags = ref<string[]>([]);
const totalPages = ref(0);
const currentPage = ref(1);
const loading = ref(false);
const limit = 5;

const fetchArticles = async (name: string, page: number) => {
    loading.value = true;
    try {
        const [articleRes, countRes] = await Promise.all([
            api.SearchOnTag({ keyWord: name, limit, page }),
            api.TagCount({ keyWord: name })
        ]);
        const articleData = await articleRes.json();
        const countData = await countRes.json();
        articles.value = articleData.data || [];
        totalPages.value = countData.data ? Math.ceil(countData.data / limit) : 0;
    } catch {
        articles.value = [];
        totalPages.value = 0;
    }
    loading.value = false;
};

const fetchAllTags = async () => {
    try {
        const res = await api.TagList();
        const data = await res.json();
        allTags.value = data.data || [];
    } catch {
        allTags.value = [];
    }
};

const changePage = async (page: number) => {
    if (page < 1 || page > totalPages.value) return;
    currentPage.value = page;
    await fetchArticles(tagName.value, page);
};

const init = async (name: string) => {
    tagName.value = name;
    currentPage.value = 1;
    await fetchArticles(name, 1);
};

onMounted(async () => {
    await init(route.params.name as string);
    fetchAllTags();
});

watch(() => route.params.name, async (newName) => {
    if (newName) {
        await init(newName as string);
    }
});
</script>

<style scoped>
.tag-page {
    max-width: 1100px;
}

.tag-layout {
    display: flex;
    gap: 24px;
    align-items: flex-start;
}

.tag-main {
    flex: 1;
    min-width: 0;
}

.tag-sidebar {
    width: 280px;
    flex-shrink: 0;
    position: sticky;
    top: 72px;
}

.tag-sidebar-card {
    padding: 20px;
}

.tag-sidebar-title {
    font-size: 16px;
    font-weight: 600;
    color: var(--color-text);
    margin: 0 0 16px;
}

.tag-list {
    display: flex;
    flex-wrap: wrap;
    gap: 8px;
}

.tag-item {
    font-size: 13px;
    padding: 5px 12px;
    border-radius: 14px;
    text-decoration: none;
    transition: background 0.2s, color 0.2s;
}

.tag-item:hover {
    background: var(--color-primary-light-bg-hover);
}

.tag-item.router-link-active {
    background: var(--color-primary);
    color: var(--color-text-white);
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

.empty-tip {
    text-align: center;
    color: var(--color-text-muted);
    font-size: 14px;
    padding: 60px 0;
}

@media (max-width: 768px) {
    .tag-layout {
        flex-direction: column;
    }

    .tag-sidebar {
        width: 100%;
        position: static;
        order: -1;
    }

    .tag-main {
        width: 100%;
        max-width: 600px;
        margin: 0 auto;
    }

    .tag-list {
        justify-content: center;
    }
}
</style>