<template>
    <div class="search-page page-container">
        <h2 class="section-title">搜索文章</h2>

        <div class="search-bar">
            <input v-model="keyword" type="text" class="search-input" placeholder="输入关键词..."
                @keydown.enter="doSearch" />
            <button class="search-btn" @click="doSearch">搜索</button>
        </div>

        <div v-if="searched && articles.length === 0 && !loading" class="empty-hint">
            <p>没有找到相关文章</p>
        </div>

        <PostList :articles="articles" :loading="loading" />

        <div ref="sentinelRef" class="sentinel"></div>

        <div v-if="!hasMore && articles.length > 0" class="no-more">
            <p>没有更多了</p>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, nextTick } from "vue";
import { BlogAPI } from "../../ts/utils/BlogAPI";
import { API_BASE_URL } from "../../ts/config/apiConfig";
import { useInfiniteSearch } from "../../composables/useInfiniteSearch";
import PostList from "../../components/PostList.vue";

const api = new BlogAPI(API_BASE_URL);
const { articles, loading, loadingMore, searched, hasMore, keyword, search, fetchPage } =
    useInfiniteSearch(api);

let observer: IntersectionObserver | null = null;
const sentinelRef = ref<HTMLElement | null>(null);

const doSearch = () => search(keyword.value);

const createObserver = () => {
    if (!sentinelRef.value) return;

    observer = new IntersectionObserver(
        (entries) => {
            if (entries[0].isIntersecting && !loadingMore.value && hasMore.value && searched.value) {
                fetchPage();
            }
        },
        { root: null, rootMargin: "0px", threshold: 0 }
    );

    observer.observe(sentinelRef.value);
};

onMounted(async () => {
    await nextTick();
    createObserver();
});

onUnmounted(() => observer?.disconnect());
</script>

<style scoped>
.search-page {
    max-width: 900px;
}

.search-bar {
    display: flex;
    flex-wrap: wrap;
    gap: 0.75rem;
    margin-bottom: 1.5rem;
}

.search-input {
    flex: 1;
    min-width: 0;
    padding: 0.625rem 1rem;
    font-size: 0.875rem;
    border: 1px solid var(--color-border);
    border-radius: 0.5rem;
    outline: none;
    background: var(--color-bg-warm);
    color: var(--color-text);
    transition: border-color 0.2s;
}

.search-input:focus {
    border-color: var(--color-primary);
}

.search-btn {
    padding: 0.625rem 1.5rem;
    font-size: 0.875rem;
    border: none;
    border-radius: 0.5rem;
    background: var(--color-primary);
    color: var(--color-text-white);
    cursor: pointer;
    transition: opacity 0.2s;
    white-space: nowrap;
}

.search-btn:hover {
    opacity: 0.85;
}

.sentinel {
    height: 1px;
}

.empty-hint,
.no-more {
    text-align: center;
    padding: 1.5rem 0;
}

.empty-hint p,
.no-more p {
    font-size: 0.875rem;
    color: var(--color-text-muted);
    margin: 0;
}

@media (max-width: 768px) {
    .search-bar {
        flex-direction: column;
    }

    .search-input {
        width: 100%;
    }

    .search-btn {
        width: 100%;
    }
}
</style>