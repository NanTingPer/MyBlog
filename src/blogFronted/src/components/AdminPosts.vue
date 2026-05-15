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
                                <button class="btn-delete" @click="deletePost(post.id)">删除</button>
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
                        <button class="btn-delete-mobile" @click="deletePost(post.id)">删除</button>
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

        <div v-show="isEditing" class="form-section">
            <div class="form-header">
                <button class="btn-back" @click="cancelEdit">‹ 返回</button>
                <h2 class="form-title">{{ isAddMode ? '添加文章' : '编辑文章' }}</h2>
            </div>

            <form class="posts-form" @submit.prevent="savePost">
                <div class="form-group">
                    <label>名称</label>
                    <input type="text" v-model="formData.name" class="form-input" placeholder="请输入文章名称" required />
                </div>

                <div class="form-group">
                    <label>标题</label>
                    <input type="text" v-model="formData.title" class="form-input" placeholder="请输入文章标题" />
                </div>

                <div class="form-group">
                    <label>描述</label>
                    <input type="text" v-model="formData.description" class="form-input" placeholder="请输入文章描述" />
                </div>

                <!-- Author 列表编辑 -->
                <div class="form-group">
                    <label>作者</label>
                    <div class="list-display">
                        <div v-for="(a, index) in formData.author" :key="index" class="list-display-item">
                            <span>{{ a }}</span>
                            <button type="button" class="btn-remove-inline" @click="removeAuthor(index)">×</button>
                        </div>
                        <span v-if="formData.author.length === 0" class="list-empty">暂无作者</span>
                    </div>
                    <button type="button" class="btn-toggle-edit" @click="showAuthorEditor = !showAuthorEditor">
                        {{ showAuthorEditor ? '收起编辑' : '编辑作者' }}
                    </button>
                    <div v-show="showAuthorEditor" class="list-editor">
                        <div class="list-editor-items">
                            <div v-for="(_, index) in editingAuthors" :key="index" class="list-editor-row">
                                <input type="text" v-model="editingAuthors[index]" class="form-input list-input"
                                    placeholder="请输入作者名称" />
                                <button type="button" class="btn-remove" @click="editingAuthors.splice(index, 1)">删除</button>
                            </div>
                        </div>
                        <div class="list-editor-actions">
                            <button type="button" class="btn-add-item" @click="editingAuthors.push('')">+ 添加作者</button>
                            <button type="button" class="btn-confirm-list" @click="confirmAuthors">确认</button>
                        </div>
                    </div>
                </div>

                <!-- Tag 列表编辑 -->
                <div class="form-group">
                    <label>标签</label>
                    <div class="list-display">
                        <div v-for="(t, index) in formData.tag" :key="index" class="list-display-item">
                            <span>{{ t }}</span>
                            <button type="button" class="btn-remove-inline" @click="removeTag(index)">×</button>
                        </div>
                        <span v-if="formData.tag.length === 0" class="list-empty">暂无标签</span>
                    </div>
                    <button type="button" class="btn-toggle-edit" @click="showTagEditor = !showTagEditor">
                        {{ showTagEditor ? '收起编辑' : '编辑标签' }}
                    </button>
                    <div v-show="showTagEditor" class="list-editor">
                        <div class="list-editor-items">
                            <div v-for="(_, index) in editingTags" :key="index" class="list-editor-row">
                                <input type="text" v-model="editingTags[index]" class="form-input list-input"
                                    placeholder="请输入标签名称" />
                                <button type="button" class="btn-remove" @click="editingTags.splice(index, 1)">删除</button>
                            </div>
                        </div>
                        <div class="list-editor-actions">
                            <button type="button" class="btn-add-item" @click="editingTags.push('')">+ 添加标签</button>
                            <button type="button" class="btn-confirm-list" @click="confirmTags">确认</button>
                        </div>
                    </div>
                </div>

                <!-- Content 编辑 -->
                <div class="form-group">
                    <label>内容</label>
                    <div v-show="showContentEditor" class="content-editor">
                        <textarea v-model="formData.content" class="form-textarea" placeholder="请输入文章内容"
                            rows="12"></textarea>
                        <button type="button" class="btn-confirm-list" @click="showContentEditor = false">收起编辑</button>
                    </div>
                    <div v-show="!showContentEditor" class="content-preview" @click="showContentEditor = true">
                        <p class="content-preview-text">{{ formData.content || '暂无内容，点击编辑' }}</p>
                        <button type="button" class="btn-toggle-edit">编辑内容</button>
                    </div>
                </div>

                <div class="form-actions">
                    <button type="submit" class="btn-save">保存</button>
                    <button type="button" class="btn-cancel" @click="cancelEdit">取消</button>
                </div>
            </form>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import type { BlogInfo } from '../ts/types/blogs/BlogInfo';
import { AdminBlogAPI } from '../ts/utils/AdminBlogAPI';

const isEditing = ref(false);
const isAddMode = ref(false);
const searchKeyword = ref('');
const currentPage = ref(1);
const pageSize = 10;
const totalCount = ref(0);

const posts = ref<BlogInfo[]>([]);

const showAuthorEditor = ref(false);
const showTagEditor = ref(false);
const showContentEditor = ref(false);

const editingAuthors = ref<string[]>([]);
const editingTags = ref<string[]>([]);

const formData = ref<BlogInfo>({
    id: undefined,
    name: '',
    author: [],
    content: '',
    tag: [],
    description: "",
    title: ""
});

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

const showAddForm = () => {
    isAddMode.value = true;
    isEditing.value = true;
    showAuthorEditor.value = false;
    showTagEditor.value = false;
    showContentEditor.value = false;
    formData.value = {
        id: undefined,
        name: '',
        author: [],
        content: '',
        tag: [],
        description: '',
        title: ''
    };
    editingAuthors.value = [];
    editingTags.value = [];
};

const showEditForm = (post: BlogInfo) => {
    isAddMode.value = false;
    isEditing.value = true;
    showAuthorEditor.value = false;
    showTagEditor.value = false;
    showContentEditor.value = false;
    formData.value = { ...post, author: [...post.author], tag: [...post.tag] };
    editingAuthors.value = [...post.author];
    editingTags.value = [...post.tag];
};

const cancelEdit = () => {
    isEditing.value = false;
    showAuthorEditor.value = false;
    showTagEditor.value = false;
    showContentEditor.value = false;
};

const removeAuthor = (index: number) => {
    formData.value.author.splice(index, 1);
};

const removeTag = (index: number) => {
    formData.value.tag.splice(index, 1);
};

const confirmAuthors = () => {
    formData.value.author = editingAuthors.value.filter(a => a.trim() !== '');
    showAuthorEditor.value = false;
};

const confirmTags = () => {
    formData.value.tag = editingTags.value.filter(t => t.trim() !== '');
    showTagEditor.value = false;
};

const savePost = async () => {
    try {
        const postToSave: BlogInfo = {
            ...formData.value,
            author: formData.value.author.filter(a => a.trim() !== ''),
            tag: formData.value.tag.filter(t => t.trim() !== '')
        };
        await AdminBlogAPI.addOrReplace(postToSave);
        alert(isAddMode.value ? '文章添加成功' : '文章更新成功');
        await loadPosts();
        cancelEdit();
    } catch (error) {
        console.error('保存文章失败:', error);
        alert('保存失败，请稍后重试');
    }
};

const deletePost = async (id?: string) => {
    if (!id) return;

    if (!confirm('确定要删除这篇文章吗？')) return;

    try {
        await AdminBlogAPI.delete({ ids: [id] });
        alert('删除成功');
        await loadPosts();
    } catch (error) {
        console.error('删除文章失败:', error);
        alert('删除失败，请稍后重试');
    }
};

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
    color: #666;
    border-bottom: 1px solid #eee;
}

.posts-table td {
    padding: 14px 16px;
    font-size: 14px;
    color: #333;
    border-bottom: 1px solid #f0f0f0;
    max-width: 200px;
    overflow: hidden;
    text-overflow: ellipsis;
}

/* 标签样式 */
.tag-item {
    display: inline-block;
    background: #e8f5e9;
    color: #2e7d32;
    padding: 2px 8px;
    border-radius: 10px;
    font-size: 12px;
    margin: 2px 4px 2px 0;
}

.actions {
    white-space: nowrap;
}

/* 文章操作按钮覆盖：蓝色编辑按钮（区别于 admin.css 的绿色） */
.btn-edit {
    padding: 6px 14px;
    background: #e3f2fd;
    color: #1976d2;
    font-size: 13px;
    margin-right: 8px;
}

.btn-edit:hover {
    background: #bbdefb;
}

/* 文章操作按钮覆盖：红色背景删除按钮（区别于 admin.css 的白底红边） */
.btn-delete {
    padding: 6px 14px;
    background: #ffebee;
    color: #d32f2f;
    border: none;
    font-size: 13px;
}

.btn-delete:hover {
    background: #ffcdd2;
}

/* 分页禁用按钮覆盖：使用 color 而非 opacity */
.pagination-btn:disabled {
    color: #ccc;
    cursor: not-allowed;
}

/* 表单区域覆盖：移除 admin.css 的 max-width 限制 */
.form-section {
    background: #fff;
    border-radius: 10px;
    box-shadow: 0 2px 12px rgba(0, 0, 0, 0.06);
    padding: 24px;
    max-width: none;
    margin: 0;
}

/* 列表展示样式 */
.list-display {
    display: flex;
    flex-wrap: wrap;
    gap: 8px;
    margin-bottom: 8px;
    min-height: 28px;
    align-items: center;
}

.list-display-item {
    display: inline-flex;
    align-items: center;
    gap: 4px;
    background: #e8f5e9;
    color: #2e7d32;
    padding: 4px 10px;
    border-radius: 12px;
    font-size: 13px;
}

.btn-remove-inline {
    background: none;
    border: none;
    color: #c62828;
    font-size: 16px;
    cursor: pointer;
    padding: 0 2px;
    line-height: 1;
}

.list-empty {
    font-size: 13px;
    color: #999;
}

.btn-toggle-edit {
    background: none;
    border: 1px solid #e0e0e0;
    border-radius: 6px;
    padding: 6px 14px;
    font-size: 13px;
    color: #666;
    cursor: pointer;
    transition: all 0.2s;
    margin-top: 4px;
}

.btn-toggle-edit:hover {
    border-color: #4caf50;
    color: #4caf50;
}

/* 列表编辑器样式 */
.list-editor {
    margin-top: 12px;
    padding: 16px;
    background: #f8f9fa;
    border-radius: 8px;
    border: 1px solid #e8e8e8;
}

.list-editor-items {
    display: flex;
    flex-direction: column;
    gap: 8px;
    margin-bottom: 12px;
}

.list-editor-row {
    display: flex;
    gap: 8px;
    align-items: center;
}

.list-input {
    flex: 1;
}

.btn-remove {
    padding: 6px 14px;
    background: #ffebee;
    color: #d32f2f;
    border: none;
    border-radius: 4px;
    font-size: 13px;
    cursor: pointer;
    white-space: nowrap;
    transition: background 0.2s;
}

.btn-remove:hover {
    background: #ffcdd2;
}

.list-editor-actions {
    display: flex;
    gap: 8px;
    margin-top: 8px;
}

.btn-add-item {
    padding: 6px 14px;
    background: #e8f5e9;
    color: #2e7d32;
    border: none;
    border-radius: 4px;
    font-size: 13px;
    cursor: pointer;
    transition: background 0.2s;
}

.btn-add-item:hover {
    background: #c8e6c9;
}

.btn-confirm-list {
    padding: 6px 14px;
    background: #4caf50;
    color: #fff;
    border: none;
    border-radius: 4px;
    font-size: 13px;
    cursor: pointer;
    transition: background 0.2s;
}

.btn-confirm-list:hover {
    background: #43a047;
}

/* 内容编辑器样式 */
.content-editor {
    display: flex;
    flex-direction: column;
    gap: 8px;
}

.content-preview {
    padding: 12px 14px;
    background: #f8f9fa;
    border-radius: 8px;
    border: 1px solid #e8e8e8;
    cursor: pointer;
    transition: border-color 0.2s;
}

.content-preview:hover {
    border-color: #4caf50;
}

.content-preview-text {
    font-size: 14px;
    color: #333;
    margin: 0 0 8px 0;
    line-height: 1.6;
    max-height: 120px;
    overflow: hidden;
    word-break: break-all;
}

/* 移动端卡片覆盖：block 布局（区别于 admin.css 的 flex 布局） */
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
    color: #999;
    margin: 0 0 8px 0;
}

.card-authors {
    margin: 0 0 8px 0;
}

.card-content-text {
    font-size: 13px;
    color: #666;
    margin: 0 0 8px 0;
    line-height: 1.5;
    word-break: break-all;
}

.card-tags {
    margin-bottom: 12px;
}

/* 卡片操作按钮覆盖：水平布局（区别于 admin.css 的垂直布局） */
.card-actions {
    display: flex;
    gap: 8px;
    border-top: 1px solid #f0f0f0;
    padding-top: 12px;
    flex-direction: row;
    margin-left: 0;
}

.btn-edit-mobile {
    flex: 1;
    padding: 8px;
    background: #e3f2fd;
    color: #1976d2;
    border-radius: 6px;
    font-size: 13px;
}

.btn-delete-mobile {
    flex: 1;
    padding: 8px;
    background: #ffebee;
    color: #d32f2f;
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

@media (max-width: 768px) {
    .posts-container {
        padding: 16px;
        min-height: 100vh;
        background: #f5f5f5;
    }

    .form-section {
        max-width: 100%;
        padding: 16px;
    }

    .list-editor-row {
        flex-direction: column;
    }

    .list-input {
        width: 100%;
    }
}
</style>
