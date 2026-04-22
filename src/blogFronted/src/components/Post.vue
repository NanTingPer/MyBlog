<template>
    <div class="post-container">
        <div v-html="content"></div>
    </div>
</template>
<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRoute } from 'vue-router';

const route = useRoute();
const content = ref('');

onMounted(async () => {
    const id = route.params.id as string;
    try {
        const response = await fetch(`http://localhost:5162/api/blog/postHTML?id=${id}`, {
            method: "GET"
        });
        const json = await response.json();
        content.value = json.data;
    } catch (error) {
        console.error('Failed to fetch post:', error);
        content.value = '<p>获取文章内容失败</p>';
    }
});
</script>

<style scoped>
.post-container {
    max-width: 80%;
    margin: 0 auto;
    padding: 40px 20px;
    background: #fff;
    border-radius: 16px;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
}

.h1 {
    color: black;
}
</style>