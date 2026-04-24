<template>
    <div class="friendlink-container">
        <div class="page-header">
            <h1 class="page-title">友链管理</h1>
            <button class="btn-add" @click="showAddForm">+ 添加友链</button>
        </div>

        <div v-show="!isEditing" class="list-section">
            <div class="search-bar">
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
                                <div class="avatar" :style="{ background: getAvatarColor(link.name) }">
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

const getAvatarColor = (name?: string): string => {
    const colors = ['#81c784', '#ffb74d', '#64b5f6', '#a1887f'];
    if (!name) return colors[0];
    let hash = 0;
    for (let i = 0; i < name.length; i++) {
        hash = name.charCodeAt(i) + ((hash << 5) - hash);
    }
    return colors[Math.abs(hash) % colors.length];
};

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

.page-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 24px;
}

.page-title {
    font-size: 20px;
    font-weight: 600;
    color: #333;
    margin: 0;
}

.btn-add {
    background: #4caf50;
    color: #fff;
    border: none;
    padding: 8px 16px;
    border-radius: 6px;
    font-size: 14px;
    cursor: pointer;
    transition: background 0.2s;
}

.btn-add:hover {
    background: #43a047;
}

.search-bar {
    margin-bottom: 16px;
}

.search-input {
    width: 300px;
    padding: 10px 14px;
    border: 1px solid #e8e8e8;
    border-radius: 6px;
    font-size: 14px;
    outline: none;
    transition: border-color 0.2s;
}

.search-input:focus {
    border-color: #4caf50;
}

.table-container {
    background: #fff;
    border-radius: 8px;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
}

.friendlink-table {
    width: 100%;
    border-collapse: collapse;
}

.friendlink-table th,
.friendlink-table td {
    padding: 14px 16px;
    text-align: left;
    border-bottom: 1px solid #f0f0f0;
}

.friendlink-table th {
    background: #fafafa;
    font-weight: 600;
    font-size: 14px;
    color: #666;
}

.friendlink-table tr:last-child td {
    border-bottom: none;
}

.avatar {
    width: 36px;
    height: 36px;
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    color: #fff;
    font-size: 14px;
    font-weight: 500;
    overflow: hidden;
}

.avatar-img {
    width: 100%;
    height: 100%;
    object-fit: cover;
}

.link-url {
    color: #4caf50;
    text-decoration: none;
    font-size: 14px;
}

.link-url:hover {
    text-decoration: underline;
}

.actions {
    display: flex;
    gap: 8px;
}

.btn-edit {
    background: #4caf50;
    color: #fff;
    border: none;
    padding: 4px 10px;
    border-radius: 4px;
    font-size: 12px;
    cursor: pointer;
    transition: background 0.2s;
}

.btn-edit:hover {
    background: #43a047;
}

.btn-delete {
    background: #fff;
    color: #e53935;
    border: 1px solid #e53935;
    padding: 4px 10px;
    border-radius: 4px;
    font-size: 12px;
    cursor: pointer;
    transition: all 0.2s;
}

.btn-delete:hover {
    background: #ffebee;
}

.empty-state {
    padding: 48px;
    text-align: center;
    color: #999;
}

.pagination {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 16px;
    border-top: 1px solid #f0f0f0;
}

.total {
    font-size: 13px;
    color: #999;
}

.pagination-controls {
    display: flex;
    gap: 4px;
}

.pagination-btn {
    width: 32px;
    height: 32px;
    border: 1px solid #e8e8e8;
    background: #fff;
    border-radius: 4px;
    cursor: pointer;
    font-size: 14px;
    transition: all 0.2s;
}

.pagination-btn:hover:not(:disabled) {
    border-color: #4caf50;
    color: #4caf50;
}

.pagination-btn.active {
    background: #4caf50;
    color: #fff;
    border-color: #4caf50;
}

.pagination-btn:disabled {
    opacity: 0.5;
    cursor: not-allowed;
}

.form-section {
    max-width: 500px;
    margin: 0 auto;
}

.form-header {
    display: flex;
    align-items: center;
    gap: 12px;
    margin-bottom: 24px;
}

.btn-back {
    background: none;
    border: none;
    font-size: 20px;
    cursor: pointer;
    color: #666;
    padding: 4px 8px;
}

.btn-back:hover {
    color: #333;
}

.form-title {
    font-size: 18px;
    font-weight: 600;
    color: #333;
    margin: 0;
}

.friendlink-form {
    background: #fff;
    border-radius: 8px;
    padding: 24px;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
}

.form-group {
    margin-bottom: 20px;
}

.form-group label {
    display: block;
    font-size: 14px;
    font-weight: 500;
    color: #333;
    margin-bottom: 8px;
}

.form-input {
    width: 100%;
    padding: 12px;
    border: 1px solid #e8e8e8;
    border-radius: 6px;
    font-size: 14px;
    outline: none;
    transition: border-color 0.2s;
    box-sizing: border-box;
}

.form-input:focus {
    border-color: #4caf50;
}

.form-actions {
    display: flex;
    gap: 12px;
    margin-top: 24px;
}

.btn-save {
    background: #4caf50;
    color: #fff;
    border: none;
    padding: 10px 24px;
    border-radius: 6px;
    font-size: 14px;
    cursor: pointer;
    transition: background 0.2s;
}

.btn-save:hover {
    background: #43a047;
}

.btn-cancel {
    background: #fff;
    color: #666;
    border: 1px solid #e8e8e8;
    padding: 10px 24px;
    border-radius: 6px;
    font-size: 14px;
    cursor: pointer;
    transition: all 0.2s;
}

.btn-cancel:hover {
    background: #f5f5f5;
}
</style>