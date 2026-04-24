<template>
    <div class="friendlink-container">
        <div class="friendlink-header">
            <h1 class="friendlink-title">友情链接</h1>
            <p class="friendlink-subtitle">那些温暖而有趣的角落，是互联网上的星光</p>
        </div>
        
        <div class="friendlink-list">
            <a 
                v-for="link in friendlinks" 
                :key="link.id" 
                :href="link.url"
                target="_blank"
                rel="noopener noreferrer"
                class="friendlink-card"
            >
                <img 
                    :src="link.avatar" 
                    :alt="link.name" 
                    class="friendlink-avatar"
                />
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
.friendlink-container {
    width: 100%;
    max-width: 800px;
    margin: 0 auto;
    padding: 40px 20px;
}

.friendlink-header {
    text-align: center;
    margin-bottom: 40px;
}

.friendlink-title {
    font-size: 28px;
    font-weight: 600;
    color: #333;
    margin: 0 0 12px;
}

.friendlink-subtitle {
    font-size: 14px;
    color: #999;
    margin: 0;
}

.friendlink-list {
    display: flex;
    flex-direction: column;
    gap: 16px;
}

.friendlink-card {
    display: flex;
    align-items: center;
    background: #fff;
    border-radius: 16px;
    padding: 20px 24px;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
    transition: transform 0.2s, box-shadow 0.2s;
    text-decoration: none;
}

.friendlink-card:hover {
    transform: translateX(4px);
    box-shadow: 0 4px 16px rgba(0, 0, 0, 0.1);
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