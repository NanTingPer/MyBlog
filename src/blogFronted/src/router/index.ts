import { createRouter, createWebHistory } from 'vue-router';
import LatestArticles from '../components/LatestArticles.vue';
import Post from '../components/Post.vue';

const routes = [
  {
    path: '/',
    name: 'home',
    component: LatestArticles
  },
  {
    path: '/post/:id',
    name: 'post',
    component: Post
  }
];

const router = createRouter({
  history: createWebHistory(),
  routes
});

export default router;
