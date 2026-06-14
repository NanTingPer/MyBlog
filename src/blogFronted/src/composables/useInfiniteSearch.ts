import { ref } from "vue";
import type { BlogInfo } from "../ts/types/blogs/BlogInfo";
import type { SearchBlogInput } from "../ts/types/blogs/SearchBlogInput";
import { BlogAPI } from "../ts/utils/BlogAPI";

/**
 * 无限滚动搜索组合式函数
 *
 * 封装搜索关键词管理、名称+内容双接口并发请求、结果去重、分页追加等逻辑。
 * 组件只需提供 IntersectionObserver 触发 fetchPage 即可。
 *
 * @param api   BlogAPI 实例
 * @param limit 每页条数，默认 5
 */
export function useInfiniteSearch(api: BlogAPI, limit = 5) {
    const articles = ref<BlogInfo[]>([]);
    /** 首次搜索加载中，控制 StaggerTransition 旋转圈 */
    const loading = ref(false);
    /** 加载更多中，防止 Observer 重复触发 */
    const loadingMore = ref(false);
    const searched = ref(false);
    const hasMore = ref(true);
    const page = ref(1);
    const keyword = ref("");

    /** 已展示文章的 ID 集合，用于去重 */
    const seenIds = new Set<string>();

    /**
     * 过滤掉已存在的文章，同时将新文章 ID 记录到 seenIds
     */
    function deduplicate(items: BlogInfo[]): BlogInfo[] {
        const fresh: BlogInfo[] = [];
        for (const item of items) {
            if (item.id && !seenIds.has(item.id)) {
                seenIds.add(item.id);
                fresh.push(item);
            }
        }
        return fresh;
    }

    /**
     * 重置所有状态，准备新一轮搜索
     */
    function reset() {
        articles.value = [];
        seenIds.clear();
        page.value = 1;
        hasMore.value = true;
    }

    /**
     * 发起新搜索
     * @param kw 搜索关键词
     */
    async function search(kw: string) {
        const trimmed = kw.trim();
        if (!trimmed) return;

        keyword.value = trimmed;
        reset();
        searched.value = true;
        loading.value = true;

        await fetchPage();
    }

    /**
     * 加载下一页数据
     * 同时调用 SearchOnName 和 SearchOnContent，合并去重后追加到列表
     */
    async function fetchPage() {
        if (loadingMore.value || !hasMore.value) return;

        loadingMore.value = true;
        try {
            const input: SearchBlogInput = {
                keyWord: keyword.value,
                limit,
                page: page.value,
            };

            const [nameRes, contentRes] = await Promise.all([
                api.SearchOnName(input),
                api.SearchOnContent(input),
            ]);

            const [nameData, contentData] = await Promise.all([
                nameRes.json(),
                contentRes.json(),
            ]);

            const newItems = deduplicate([
                ...(nameData.data || []),
                ...(contentData.data || []),
            ]);

            if (newItems.length === 0) {
                hasMore.value = false;
            } else {
                articles.value.push(...newItems);
                if (newItems.length < limit) {
                    hasMore.value = false;
                } else {
                    page.value++;
                }
            }
        } catch (e) {
            console.error("搜索失败:", e);
            hasMore.value = false;
        } finally {
            loading.value = false;
            loadingMore.value = false;
        }
    }

    return {
        articles,
        loading,
        loadingMore,
        searched,
        hasMore,
        keyword,
        search,
        fetchPage,
    };
}