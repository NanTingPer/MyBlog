<template>
    <div class="friendlink-container">
        <div class="page-header">
            <h1 class="page-title">友链</h1>
            <button class="btn-add" @click="showAddForm">+ 添加友链</button>
            <button class="btn-add-mobile" @click="showAddForm">+</button>
        </div>

        <div v-show="!isEditing" class="list-section">
            <div class="search-bar">
                <span class="search-icon"></span>
                <input type="text" v-model="searchKeyword" placeholder="搜索友链名称..." class="search-input"
                    @input="handleSearch" />
            </div>

            <div class="table-container">
                <table class="friendlink-table table-fixed">
                    <colgroup>
                        <col class="col-avatar">
                        <col class="col-name">
                        <col class="col-url">
                        <col class="col-dictum">
                        <col class="col-date">
                        <col class="col-actions">
                    </colgroup>
                    <thead>
                        <tr>
                            <th>头像</th>
                            <th>名称</th>
                            <th>跳转链接</th>
                            <th>格言</th>
                            <th>创建时间</th>
                            <th>操作</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-for="link in filteredLinks" :key="link.id">
                            <td>
                                <div class="avatar">
                                    <img v-if="link.avatar" :src="link.avatar" alt="" class="avatar-img">
                                    <span v-else>{{ link.name?.charAt(0) }}</span>
                                </div>
                            </td>
                            <td>{{ link.name }}</td>
                            <td><a :href="link.url" target="_blank" class="link-url">{{ link.url }}</a></td>
                            <td>{{ link.dictum }}</td>
                            <td>{{ formatDate(link.createTime) }}</td>
                            <td class="actions">
                                <button class="btn-edit" @click="showEditForm(link)">编辑</button>
                                <button class="btn-delete" @click="openDeleteDialog(link.id)">删除</button>
                            </td>
                        </tr>
                    </tbody>
                </table>

                <div v-if="filteredLinks.length === 0" class="empty-state">
                    <p>暂无友链数据</p>
                </div>

                <div v-if="filteredLinks.length > 0" class="pagination">
                    <span class="total">共 {{ filteredLinks.length }} 条数据</span>
                    <div class="pagination-controls">
                        <button class="pagination-btn" :disabled="currentPage === 1">‹</button>
                        <button class="pagination-btn active">{{ currentPage }}</button>
                        <button class="pagination-btn" :disabled="currentPage >= totalPages">›</button>
                    </div>
                </div>
            </div>

            <div class="mobile-list">
                <div v-for="link in filteredLinks" :key="link.id" class="mobile-card">
                    <div class="avatar">
                        <img v-if="link.avatar" :src="link.avatar" alt="" class="avatar-img">
                        <span v-else>{{ link.name?.charAt(0) }}</span>
                    </div>
                    <div class="card-content">
                        <h3 class="card-name">{{ link.name }}</h3>
                        <p class="card-url">{{ link.url }}</p>
                        <p class="card-dictum">{{ link.dictum }}</p>
                        <p class="card-date">{{ formatDate(link.createTime) }}</p>
                    </div>
                    <div class="card-actions">
                        <button class="btn-edit-mobile" @click="showEditForm(link)">编辑</button>
                        <button class="btn-delete-mobile" @click="openDeleteDialog(link.id)">删除</button>
                    </div>
                </div>

                <div v-if="filteredLinks.length === 0" class="empty-state-mobile">
                    <p>暂无友链数据</p>
                </div>

                <div v-if="filteredLinks.length > 0" class="mobile-footer">
                    <span class="total">共 {{ filteredLinks.length }} 条友链</span>
                </div>
            </div>
        </div>

        <!-- 表单：使用 ObjectForm 组件 -->
        <div v-show="isEditing">
            <ObjectForm
                v-model="formData"
                :fields="friendlinkFields"
                :title="isAddMode ? '添加友链' : '编辑友链'"
                :is-add-mode="isAddMode"
                :loading="saving"
                @submit="saveFriendlink"
                @cancel="cancelEdit"
            />
        </div>

        <!-- 删除确认对话框：使用 ConfirmDialog 组件 -->
        <ConfirmDialog
            v-model:visible="showDeleteConfirm"
            title="确认删除友链"
            content="确定要删除这个友链吗？此操作不可撤销。"
            confirm-text="删除"
            :danger="true"
            :loading="deleting"
            @confirm="confirmDelete"
        />
    </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue';
import type { Friendslink } from '../../ts/types/friendlink/Friendslink';
import type { DeleteByIdInput } from '../../ts/types/friendlink/DeleteByIdInput';
import { FriendlinkAPI } from '../../ts/utils/FriendlinkAPI';
import ObjectForm from '../../components/ObjectForm.vue';
import type { FieldConfig } from '../../components/ObjectForm.vue';
import ConfirmDialog from '../../components/ConfirmDialog.vue';

/* ===== 字段配置 ===== */
/** 友链表单字段配置，驱动 ObjectForm 自动渲染 */
const friendlinkFields: FieldConfig[] = [
    { key: 'name', label: '名称', type: 'text', required: true },
    { key: 'url', label: '跳转链接', type: 'url', required: true },
    { key: 'dictum', label: '格言', type: 'text' },
    { key: 'avatar', label: '头像 URL', type: 'url' },
    { key: 'id', label: 'ID', type: 'readonly', order: 99, hideOnAdd: true },
    { key: 'createTime', label: '创建时间', type: 'readonly', order: 100, hideOnAdd: true },
];

/* ===== 列表状态 ===== */
const isEditing = ref(false);
const isAddMode = ref(false);
const searchKeyword = ref('');
const currentPage = ref(1);
const friendlinks = ref<Friendslink[]>([]);

/* ===== 表单状态 ===== */
const formData = ref<Record<string, any>>({
    id: undefined,
    name: '',
    url: '',
    dictum: '',
    avatar: ''
});
const saving = ref(false);

/* ===== 删除对话框状态 ===== */
const showDeleteConfirm = ref(false);
const deleting = ref(false);
const deleteTargetId = ref('');

/* ===== 计算属性 ===== */
const filteredLinks = computed(() => {
    if (!searchKeyword.value) {
        return friendlinks.value;
    }
    return friendlinks.value.filter(link =>
        link.name?.toLowerCase().includes(searchKeyword.value.toLowerCase())
    );
});

const totalPages = computed(() => Math.ceil(filteredLinks.value.length / 10));

/* ===== 工具函数 ===== */
const formatDate = (dateStr?: string): string => {
    if (!dateStr) return '';
    return dateStr.split(' ')[0];
};

const handleSearch = () => {
    currentPage.value = 1;
};

/* ===== 表单操作 ===== */
/** 显示新增表单 */
const showAddForm = () => {
    isAddMode.value = true;
    isEditing.value = true;
    formData.value = {
        id: undefined,
        name: '',
        url: '',
        dictum: '',
        avatar: ''
    };
};

/** 显示编辑表单 */
const showEditForm = (link: Friendslink) => {
    isAddMode.value = false;
    isEditing.value = true;
    formData.value = { ...link };
};

/** 取消编辑，返回列表 */
const cancelEdit = () => {
    isEditing.value = false;
};

/**
 * 保存友链（ObjectForm submit 事件回调）
 * 组件传递的 data 是当前表单数据的副本
 */
const saveFriendlink = async (data: Record<string, any>) => {
    saving.value = true;
    try {
        await FriendlinkAPI.addOrUpdate(data as Friendslink);
        alert(isAddMode.value ? '友链添加成功' : '友链更新成功');
        await loadFriendlinks();
        cancelEdit();
    } catch (error) {
        console.error('保存友链失败:', error);
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
        const input: DeleteByIdInput = { id: deleteTargetId.value };
        await FriendlinkAPI.delete(input);
        showDeleteConfirm.value = false;
        alert('删除成功');
        await loadFriendlinks();
    } catch (error) {
        console.error('删除友链失败:', error);
        alert('删除失败，请稍后重试');
    } finally {
        deleting.value = false;
    }
};

/* ===== 数据加载 ===== */
const loadFriendlinks = async () => {
    try {
        const response = await FriendlinkAPI.getAll();
        const data = await response.json();
        friendlinks.value = data.data || [];
    } catch (error) {
        console.error('加载友链失败:', error);
    }
};

loadFriendlinks();
</script>

<style scoped>
.friendlink-container {
    padding: 24px;
}

.table-container {
    border-radius: 10px;
    box-shadow: var(--shadow-md);
    overflow: hidden;
}

.col-avatar { width: 5rem; }
.col-name { width: 8rem; }
.col-url { width: 14rem; }
.col-dictum { width: 10rem; }
.col-date { width: 9rem; }
.col-actions { width: 9rem; }
@media (max-width: 768px) {
    .friendlink-container {
        padding: 16px;
        min-height: 100vh;
        background: var(--color-bg-light);
    }

    .btn-back-mobile {
        background: none;
        border: none;
        font-size: 24px;
        cursor: pointer;
        color: var(--color-text);
        padding: 4px 8px;
        display: block;
    }

    .card-url {
        font-size: 12px;
        color: var(--color-text-secondary);
        margin: 0 0 4px 0;
        overflow: hidden;
        text-overflow: ellipsis;
        white-space: nowrap;
    }

    .card-dictum {
        font-size: 13px;
        color: var(--color-text-muted);
        margin: 0 0 4px 0;
        overflow: hidden;
        text-overflow: ellipsis;
        white-space: nowrap;
    }
}

@media (min-width: 769px) {
    .btn-back-mobile {
        display: none;
    }
}
</style>