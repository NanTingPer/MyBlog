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
                <table class="friendlink-table">
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
                                <button class="btn-delete" @click="deleteFriendlink(link.id)">删除</button>
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
                        <button class="btn-delete-mobile" @click="deleteFriendlink(link.id)">删除</button>
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

        <div v-show="isEditing" class="form-section">
            <div class="form-header">
                <button class="btn-back" @click="cancelEdit">‹ 返回</button>
                <h2 class="form-title">{{ isAddMode ? '添加友链' : '编辑友链' }}</h2>
            </div>

            <form class="friendlink-form" @submit.prevent="saveFriendlink">
                <div class="form-group">
                    <label>名称</label>
                    <input type="text" v-model="formData.name" class="form-input" placeholder="请输入友链名称" required />
                </div>

                <div class="form-group">
                    <label>跳转链接</label>
                    <input type="url" v-model="formData.url" class="form-input" placeholder="请输入友链地址" required />
                </div>

                <div class="form-group">
                    <label>格言</label>
                    <input type="text" v-model="formData.dictum" class="form-input" placeholder="请输入友链格言" />
                </div>

                <div class="form-group">
                    <label>头像 URL</label>
                    <input type="url" v-model="formData.avatar" class="form-input" placeholder="请输入头像URL" />
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
import { ref, computed } from 'vue';
import type { Friendslink } from '../ts/types/friendlink/Friendslink';
import { FriendlinkAPI } from '../ts/utils/FriendlinkAPI';
import type { DeleteByIdInput } from '../ts/types/friendlink/DeleteByIdInput';

const isEditing = ref(false);
const isAddMode = ref(false);
const searchKeyword = ref('');
const currentPage = ref(1);

const friendlinks = ref<Friendslink[]>([]);

const formData = ref<Friendslink>({
    id: undefined,
    name: '',
    url: '',
    dictum: '',
    avatar: ''
});

const filteredLinks = computed(() => {
    if (!searchKeyword.value) {
        return friendlinks.value;
    }
    return friendlinks.value.filter(link =>
        link.name?.toLowerCase().includes(searchKeyword.value.toLowerCase())
    );
});

const totalPages = computed(() => Math.ceil(filteredLinks.value.length / 10));

const formatDate = (dateStr?: string): string => {
    if (!dateStr) return '';
    return dateStr.split(' ')[0];
};

const handleSearch = () => {
    currentPage.value = 1;
};

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

const showEditForm = (link: Friendslink) => {
    isAddMode.value = false;
    isEditing.value = true;
    formData.value = { ...link };
};

const cancelEdit = () => {
    isEditing.value = false;
};

const saveFriendlink = async () => {
    try {
        await FriendlinkAPI.addOrUpdate(formData.value);
        alert(isAddMode.value ? '友链添加成功' : '友链更新成功');
        await loadFriendlinks();
        cancelEdit();
    } catch (error) {
        console.error('保存友链失败:', error);
        alert('保存失败，请稍后重试');
    }
};

const deleteFriendlink = async (id?: string) => {
    if (!id) return;

    if (!confirm('确定要删除这个友链吗？')) return;

    try {
        const input: DeleteByIdInput = { id };
        await FriendlinkAPI.delete(input);
        alert('删除成功');
        await loadFriendlinks();
    } catch (error) {
        console.error('删除友链失败:', error);
        alert('删除失败，请稍后重试');
    }
};

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

@media (max-width: 768px) {
    .friendlink-container {
        padding: 16px;
        min-height: 100vh;
        background: #f5f5f5;
    }

    .btn-back-mobile {
        background: none;
        border: none;
        font-size: 24px;
        cursor: pointer;
        color: #333;
        padding: 4px 8px;
        display: block;
    }

    .card-url {
        font-size: 12px;
        color: #666;
        margin: 0 0 4px 0;
        overflow: hidden;
        text-overflow: ellipsis;
        white-space: nowrap;
    }

    .card-dictum {
        font-size: 13px;
        color: #999;
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
