<template>
    <div>
        {{ content }}
    </div>
</template>
<script setup lang="ts">
import { ref } from 'vue';
const props = defineProps({
    id: { type: String }
});
const content = ref('')
let response = await fetch('http://localhost:5162/api/blog/postHTML?id=' + props.id, {
    method: "GET"
});
let json = await response.json();
content.value = json.data;
</script>