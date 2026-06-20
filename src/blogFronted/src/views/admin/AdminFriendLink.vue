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
                    <thead>
                        <tr>
                            <th v-for="col in tableColumns" :key="col">{{ col }}</th>
                            <th>操作</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-for="link in filteredLinks" :key="link.id">
                            <td v-for="col in tableColumns" :key="col">
                                <!-- 头像列：图片展示 -->
                                <template v-if="col === 'avatar'">
                                    <div class="avatar">
                                        <img v-if="link[col]" :src="link[col]" alt="" class="avatar-img">
                                        <span v-else>{{ link.name?.charAt(0) }}</span>
                                    </div>
                                </template>
                                <!-- URL列：可点击链接 -->
                                <template v-else-if="col === 'url'">
                                    <a :href="link[col]" target="_blank" class="link-url">{{ link[col] }}</a>
                                </template>
                                <!-- 时间列：格式化 -->
                                <template v-else-if="col === 'createTime' || col === 'editTime'">
                                    {{ formatDate(link[col]) }}
                                </template>
                                <!-- 数组列：标签展示 -->
                                <template v-else-if="Array.isArray(link[col])">
                                    <span v-for="(item, idx) in link[col]" :key="idx" class="tag-item">{{ item }}</span>
                                </template>
                                <!-- 默认列 -->
                                <template v-else>
                                    {{ link[col] }}
                                </template>
                            </td>
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
                        <template v-for="col in tableColumns" :key="col">
                            <p v-if="col !== 'avatar'" class="card-field">
                                <span class="card-field-label">{{ col }}: </span>
                                <template v-if="col === 'createTime' || col === 'editTime'">
                                    {{ formatDate(link[col]) }}
                                </template>
                                <template v-else-if="Array.isArray(link[col])">
                                    {{ link[col].join(', ') }}
                                </template>
                                <template v-else>
                                    {{ link[col] }}
                                </template>
                            </p>
                        </template>
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
import { FriendlinkAPI } from '../../ts/utils/FriendlinkAPI';
import { generateFields, generateTableColumns } from '../../ts/utils/fieldUtils';
import ObjectForm from '../../components/ObjectForm.vue';
import type { FieldConfig } from '../../components/ObjectForm.vue';
import ConfirmDialog from '../../components/ConfirmDialog.vue';

/* ===== 动态字段配置 ===== */
/** 表单字段配置，从第一条数据动态生成 */
const friendlinkFields = ref<FieldConfig[]>([]);
/** 表格列 key，从第一条数据动态生成 */
const tableColumns = ref<string[]>([]);

/* ===== 列表状态 ===== */
const isEditing = ref(false);
const isAddMode = ref(false);
const searchKeyword = ref('');
const currentPage = ref(1);
const friendlinks = ref<Record<string, any>[]>([]);

/* ===== 表单状态 ===== */
const formData = ref<Record<string, any>>({});
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

/** 根据数据动态生成空的表单初始值 */
function buildEmptyForm(data: Record<string, any>): Record<string, any> {
    const form: Record<string, any> = {};
    for (const [key, value] of Object.entries(data)) {
        if (Array.isArray(value)) {
            form[key] = [];
        } else if (typeof value === 'number') {
            form[key] = 0;
        } else if (typeof value === 'boolean') {
            form[key] = false;
        } else {
            form[key] = '';
        }
    }
    return form;
}

/* ===== 表单操作 ===== */
/** 显示新增表单 */
const showAddForm = () => {
    isAddMode.value = true;
    isEditing.value = true;
    // 从当前字段配置生成空表单
    const empty: Record<string, any> = {};
    for (const f of friendlinkFields.value) {
        if (f.type === 'array') empty[f.key] = [];
        else empty[f.key] = '';
    }
    formData.value = empty;
};

/** 显示编辑表单 */
const showEditForm = (link: Record<string, any>) => {
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
        await FriendlinkAPI.addOrUpdate(data as any);
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
        await FriendlinkAPI.delete({ id: deleteTargetId.value } as any);
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
        const list = data.data || [];
        friendlinks.value = list;

        // 从第一条数据推断表格列和表单字段
        if (list.length > 0) {
            tableColumns.value = generateTableColumns(list[0]);
            friendlinkFields.value = generateFields(list[0]);
        }
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

    .card-field {
        font-size: 13px;
        color: var(--color-text-secondary);
        margin: 0 0 4px 0;
        overflow: hidden;
        text-overflow: ellipsis;
        white-space: nowrap;
    }

    .card-field-label {
        font-weight: 600;
        color: var(--color-text-muted);
    }
}

@media (min-width: 769px) {
    .btn-back-mobile {
        display: none;
    }
}
</style>
