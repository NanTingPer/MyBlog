import { createRouter, createWebHistory } from 'vue-router';
import LatestArticles from '../components/LatestArticles.vue';
import Post from '../components/Post.vue';
import FriendLink from '../components/FriendLink.vue';

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
  },
  {
    path: '/friendlink',
    name: 'friendlink',
    component: FriendLink
  }
];

const router = createRouter({
  history: createWebHistory(),
  routes
});

export default router;
