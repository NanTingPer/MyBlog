<template>
    <div class="posts-container">
        <div class="page-header">
            <h1 class="page-title">文章管理</h1>
            <button class="btn-add" @click="showAddForm">+ 添加文章</button>
            <button class="btn-add-mobile" @click="showAddForm">+</button>
        </div>

        <div v-show="!isEditing" class="list-section">
            <div class="search-bar">
                <span class="search-icon"></span>
                <input type="text" v-model="searchKeyword" placeholder="搜索文章名称..." class="search-input" />
            </div>

            <div class="table-container">
                <table class="posts-table">
                    <thead>
                        <tr>
                            <th>名称</th>
                            <th>创建时间</th>
                            <th>作者</th>
                            <th>内容</th>
                            <th>标签</th>
                            <th>操作</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-for="post in posts" :key="post.id">
                            <td>{{ post.name }}</td>
                            <td>{{ formatDate(post.createTime) }}</td>
                            <td>
                                <span v-for="a in post.author" :key="a" class="tag-item">{{ a }}</span>
                            </td>
                            <td>{{ truncateContent(post.content) }}</td>
                            <td>
                                <span v-for="t in post.tag" :key="t" class="tag-item">{{ t }}</span>
                            </td>
                            <td class="actions">
                                <button class="btn-edit" @click="showEditForm(post)">编辑</button>
                                <button class="btn-delete" @click="openDeleteDialog(post.id)">删除</button>
                            </td>
                        </tr>
                    </tbody>
                </table>

                <div v-if="posts.length === 0" class="empty-state">
                    <p>暂无文章数据</p>
                </div>

                <div v-if="posts.length > 0" class="pagination">
                    <span class="total">共 {{ totalCount }} 条数据</span>
                    <div class="pagination-controls">
                        <button class="pagination-btn" :disabled="currentPage <= 1" @click="goToPage(currentPage - 1)">‹</button>
                        <button class="pagination-btn active">{{ currentPage }}</button>
                        <button class="pagination-btn" :disabled="posts.length < pageSize" @click="goToPage(currentPage + 1)">›</button>
                    </div>
                </div>
            </div>

            <div class="mobile-list">
                <div v-for="post in posts" :key="post.id" class="mobile-card">
                    <div class="card-content">
                        <h3 class="card-name">{{ post.name }}</h3>
                        <p class="card-date">{{ formatDate(post.createTime) }}</p>
                        <p class="card-authors">
                            <span v-for="a in post.author" :key="a" class="tag-item">{{ a }}</span>
                        </p>
                        <p class="card-content-text">{{ truncateContent(post.content) }}</p>
                        <div class="card-tags">
                            <span v-for="t in post.tag" :key="t" class="tag-item">{{ t }}</span>
                        </div>
                    </div>
                    <div class="card-actions">
                        <button class="btn-edit-mobile" @click="showEditForm(post)">编辑</button>
                        <button class="btn-delete-mobile" @click="openDeleteDialog(post.id)">删除</button>
                    </div>
                </div>

                <div v-if="posts.length === 0" class="empty-state-mobile">
                    <p>暂无文章数据</p>
                </div>

                <div v-if="posts.length > 0" class="mobile-footer">
                    <span class="total">共 {{ totalCount }} 篇文章</span>
                    <div class="pagination-controls">
                        <button class="pagination-btn" :disabled="currentPage <= 1" @click="goToPage(currentPage - 1)">‹</button>
                        <span>{{ currentPage }}</span>
                        <button class="pagination-btn" :disabled="posts.length < pageSize" @click="goToPage(currentPage + 1)">›</button>
                    </div>
                </div>
            </div>
        </div>

        <!-- 表单：使用 ObjectForm 组件 -->
        <div v-show="isEditing">
            <ObjectForm
                v-model="formData"
                :fields="postFields"
                :title="isAddMode ? '添加文章' : '编辑文章'"
                :is-add-mode="isAddMode"
                :loading="saving"
                @submit="savePost"
                @cancel="cancelEdit"
            />
        </div>

        <!-- 删除确认对话框：使用 ConfirmDialog 组件 -->
        <ConfirmDialog
            v-model:visible="showDeleteConfirm"
            title="确认删除文章"
            content="确定要删除这篇文章吗？此操作不可撤销。"
            confirm-text="删除"
            :danger="true"
            :loading="deleting"
            @confirm="confirmDelete"
        />
    </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import type { BlogInfo } from '../../ts/types/blogs/BlogInfo';
import { AdminBlogAPI } from '../../ts/utils/AdminBlogAPI';
import ObjectForm from '../../components/ObjectForm.vue';
import type { FieldConfig } from '../../components/ObjectForm.vue';
import ConfirmDialog from '../../components/ConfirmDialog.vue';

/* ===== 字段配置 ===== */
/** 文章表单字段配置，驱动 ObjectForm 自动渲染 */
const postFields: FieldConfig[] = [
    { key: 'name', label: '名称', type: 'text', required: true },
    { key: 'title', label: '标题', type: 'text' },
    { key: 'description', label: '描述', type: 'text' },
    { key: 'author', label: '作者', type: 'array' },
    { key: 'tag', label: '标签', type: 'array' },
    { key: 'content', label: '内容', type: 'textarea' },
    { key: 'id', label: 'ID', type: 'readonly', order: 99, hideOnAdd: true },
    { key: 'createTime', label: '创建时间', type: 'readonly', order: 100, hideOnAdd: true },
    { key: 'editTime', label: '编辑时间', type: 'readonly', order: 101, hideOnAdd: true },
];

/* ===== 列表状态 ===== */
const isEditing = ref(false);
const isAddMode = ref(false);
const searchKeyword = ref('');
const currentPage = ref(1);
const pageSize = 10;
const totalCount = ref(0);
const posts = ref<BlogInfo[]>([]);

/* ===== 表单状态 ===== */
const formData = ref<Record<string, any>>({
    id: undefined,
    name: '',
    title: '',
    description: '',
    author: [] as string[],
    content: '',
    tag: [] as string[],
});
const saving = ref(false);

/* ===== 删除对话框状态 ===== */
const showDeleteConfirm = ref(false);
const deleting = ref(false);
const deleteTargetId = ref('');

/* ===== 工具函数 ===== */
const truncateContent = (content: string): string => {
    if (!content) return '';
    return content.length > 20 ? content.substring(0, 20) + '...' : content;
};

const formatDate = (timestamp?: number): string => {
    if (!timestamp) return '';
    let ms = Number(timestamp.toString().substring(0, 13));
    const date = new Date(ms);
    if (isNaN(date.getTime()) || date.getFullYear() < 2000 || date.getFullYear() > 2100) {
        return '未知日期';
    };
    return `${date.getFullYear()}年${date.getMonth() + 1}月${date.getDate()}日`;
};

/* ===== 搜索与分页 ===== */
let searchTimer: ReturnType<typeof setTimeout> | null = null;

watch(searchKeyword, () => {
    if (searchTimer) clearTimeout(searchTimer);
    searchTimer = setTimeout(() => {
        currentPage.value = 1;
        loadPosts();
    }, 500);
});

const goToPage = (page: number) => {
    currentPage.value = page;
    loadPosts();
};

/* ===== 表单操作 ===== */
/** 显示新增表单 */
const showAddForm = () => {
    isAddMode.value = true;
    isEditing.value = true;
    formData.value = {
        id: undefined,
        name: '',
        title: '',
        description: '',
        author: [],
        content: '',
        tag: [],
    };
};

/** 显示编辑表单 */
const showEditForm = (post: BlogInfo) => {
    isAddMode.value = false;
    isEditing.value = true;
    formData.value = {
        ...post,
        author: [...post.author],
        tag: [...post.tag]
    };
};

/** 取消编辑，返回列表 */
const cancelEdit = () => {
    isEditing.value = false;
};

/**
 * 保存文章（ObjectForm submit 事件回调）
 * 组件传递的 data 是当前表单数据的副本
 */
const savePost = async (data: Record<string, any>) => {
    saving.value = true;
    try {
        const postToSave: BlogInfo = {
            ...data as BlogInfo,
            author: (data.author as string[]).filter((a: string) => a.trim() !== ''),
            tag: (data.tag as string[]).filter((t: string) => t.trim() !== '')
        };
        await AdminBlogAPI.addOrReplace(postToSave);
        alert(isAddMode.value ? '文章添加成功' : '文章更新成功');
        await loadPosts();
        cancelEdit();
    } catch (error) {
        console.error('保存文章失败:', error);
        alert('保存失败，请稍后重试');
    } finally {
        saving.value = false;
    }
};

/* ===== 删除操作 ===== */
/** 打开删除确认对话框 */
const openDeleteDialog = (id?: string) => {
    if (!id) return;
    deleteTargetId.value = id;
    showDeleteConfirm.value = true;
};

/** 确认删除（ConfirmDialog confirm 事件回调） */
const confirmDelete = async () => {
    deleting.value = true;
    try {
        await AdminBlogAPI.delete({ ids: [deleteTargetId.value] });
        showDeleteConfirm.value = false;
        alert('删除成功');
        await loadPosts();
    } catch (error) {
        console.error('删除文章失败:', error);
        alert('删除失败，请稍后重试');
    } finally {
        deleting.value = false;
    }
};

/* ===== 数据加载 ===== */
const loadPosts = async () => {
    try {
        const input = {
            keyWord: searchKeyword.value || '',
            limit: pageSize,
            page: currentPage.value
        };
        const response = await AdminBlogAPI.getAllToPage(input);
        const data = await response.json();
        posts.value = data.data || [];
        totalCount.value = data.total || posts.value.length;
    } catch (error) {
        console.error('加载文章失败:', error);
    }
};

loadPosts();
</script>

<style scoped>
/* Posts 容器 */
.posts-container {
    padding: 24px;
}

/* 搜索框覆盖：admin.css 默认 300px，文章页面需要 100% */
.search-input {
    width: 100%;
}

/* 表格容器覆盖：圆角和阴影微调 */
.table-container {
    border-radius: 10px;
    box-shadow: 0 2px 12px rgba(0, 0, 0, 0.06);
    overflow: hidden;
}

/* 文章表格（区别于 admin.css 中的 .friendlink-table） */
.posts-table {
    width: 100%;
    border-collapse: collapse;
}

.posts-table thead {
    background: #f8f9fa;
}

.posts-table th {
    padding: 14px 16px;
    text-align: left;
    font-size: 13px;
    font-weight: 600;
    color: var(--color-text-secondary);
    border-bottom: 1px solid var(--color-border-light);
}

.posts-table td {
    padding: 14px 16px;
    font-size: 14px;
    color: var(--color-text);
    border-bottom: 1px solid var(--color-border-light);
    max-width: 200px;
    overflow: hidden;
    text-overflow: ellipsis;
}

/* 标签样式 */
.tag-item {
    display: inline-block;
    background: var(--color-primary-light-bg);
    color: var(--color-primary-text);
    padding: 2px 8px;
    border-radius: 10px;
    font-size: 12px;
    margin: 2px 4px 2px 0;
}

.actions {
    white-space: nowrap;
}

/* 文章操作按钮覆盖：蓝色编辑按钮 */
.btn-edit {
    padding: 6px 14px;
    background: var(--color-info-light-bg);
    color: var(--color-info);
    font-size: 13px;
    margin-right: 8px;
}

.btn-edit:hover {
    background: var(--color-info-light-bg-hover);
}

/* 文章操作按钮覆盖：红色背景删除按钮 */
.btn-delete {
    padding: 6px 14px;
    background: var(--color-danger-light-bg);
    color: var(--color-danger-text);
    border: none;
    font-size: 13px;
}

.btn-delete:hover {
    background: var(--color-danger-light-bg-hover);
}

/* 分页禁用按钮覆盖 */
.pagination-btn:disabled {
    color: var(--color-text-muted);
    cursor: not-allowed;
}

/* 移动端卡片覆盖：block 布局 */
.mobile-card {
    display: block;
    border-radius: 10px;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
}

.card-content {
    margin-left: 0;
}

.card-name {
    margin: 0 0 8px 0;
}

.card-date {
    color: var(--color-text-muted);
    margin: 0 0 8px 0;
}

.card-authors {
    margin: 0 0 8px 0;
}

.card-content-text {
    font-size: 13px;
    color: var(--color-text-secondary);
    margin: 0 0 8px 0;
    line-height: 1.5;
    word-break: break-all;
}

.card-tags {
    margin-bottom: 12px;
}

/* 卡片操作按钮覆盖：水平布局 */
.card-actions {
    display: flex;
    gap: 8px;
    border-top: 1px solid var(--color-border-light);
    padding-top: 12px;
    flex-direction: row;
    margin-left: 0;
}

.btn-edit-mobile {
    flex: 1;
    padding: 8px;
    background: var(--color-info-light-bg);
    color: var(--color-info);
    border-radius: 6px;
    font-size: 13px;
}

.btn-delete-mobile {
    flex: 1;
    padding: 8px;
    background: var(--color-danger-light-bg);
    color: var(--color-danger-text);
    border: none;
    border-radius: 6px;
    font-size: 13px;
}

/* 移动端分页覆盖：水平布局 */
.mobile-footer {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 16px;
}

/* 表单区域：覆盖 admin.css 的 max-width: 500px，文章表单需要全宽 */
:deep(.form-section) {
    max-width: none;
}

@media (max-width: 768px) {
    .posts-container {
        padding: 16px;
        min-height: 100vh;
        background: #f5f5f5;
    }
}
</style>