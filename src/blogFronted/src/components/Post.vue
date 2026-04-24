<template>
    <div class="post-container">
        <div v-html="content"></div>
    </div>
</template>
<script setup lang="ts">
import { ref, onMounted, nextTick } from 'vue';
import { useRoute } from 'vue-router';
import { apiFetch } from '../ts/config/apiConfig';

const route = useRoute();
const content = ref('');

onMounted(async () => {
    const id = route.params.id as string;
    try {
        apiFetch(`/api/blog/postHTML?id=${id}`, {
            method: "GET"
        }).then(response => {
            if(response.status != 200)
                return;
            
            response.json().then(json => {
                content.value = json.data;
                nextTick(() => eval('Prism.highlightAll();'));
            })
        });
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