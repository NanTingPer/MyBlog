<template>
    <div class="page-container">
        <div class="page-header">
            <h1 class="page-title">友情链接</h1>
            <p class="page-subtitle">那些温暖而有趣的角落，是互联网上的星光</p>
        </div>

        <div class="friendlink-list">
            <a v-for="link in friendlinks" :key="link.id" :href="link.url" target="_blank" rel="noopener noreferrer"
                class="card friendlink-card">
                <img :src="link.avatar" :alt="link.name" class="friendlink-avatar" />
                <div class="friendlink-info">
                    <h3 class="friendlink-name">{{ link.name }}</h3>
                    <p class="friendlink-dictum">{{ link.dictum }}</p>
                </div>
            </a>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import type { Friendslink } from '../ts/types/friendlink/Friendslink';
import type { FriendslinkListBaseResult } from '../ts/types/friendlink/FriendslinkListBaseResult';
import { FriendlinkAPI } from '../ts/utils/FriendlinkAPI';

const friendlinks = ref<Friendslink[]>([]);

onMounted(async () => {
    try {
        const response = await FriendlinkAPI.getAll();
        const data: FriendslinkListBaseResult = await response.json();
        if (data.code === 200 && data.data) {
            friendlinks.value = data.data;
        }
    } catch (error) {
        console.error('Failed to fetch friendlinks:', error);
        friendlinks.value = [
            {
                id: '1',
                name: '南亭',
                url: 'https://www.nantingya.top',
                dictum: '你好',
                avatar: 'https://www.nantingya.top/avatar.png'
            }
        ];
    }
});
</script>

<style scoped>
.friendlink-list {
    display: flex;
    flex-direction: column;
    gap: 16px;
}

.friendlink-card {
    display: flex;
    align-items: center;
    padding: 20px 24px;
    text-decoration: none;
}

.friendlink-avatar {
    width: 64px;
    height: 64px;
    border-radius: 50%;
    object-fit: cover;
    flex-shrink: 0;
}

.friendlink-info {
    flex: 1;
    margin-left: 20px;
}

.friendlink-name {
    font-size: 16px;
    font-weight: 600;
    color: #333;
    margin: 0 0 6px;
}

.friendlink-dictum {
    font-size: 13px;
    color: #999;
    margin: 0;
}
</style>