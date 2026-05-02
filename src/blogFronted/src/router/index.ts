import { createRouter, createWebHistory } from "vue-router";
import LatestArticles from "../components/LatestPosts.vue";
import Articles from "../components/Posts.vue";
import Post from "../components/Post.vue";
import FriendLink from "../components/FriendLink.vue";
import About from "../components/About.vue";

const routes = [
    {
        path: "/",
        redirect: "/latest",
    },
    {
        path: "/latest",
        name: "latest",
        component: LatestArticles,
    },
    {
        path: "/articles",
        name: "articles",
        component: Articles,
    },
    {
        path: "/post/:id",
        name: "post",
        component: Post,
    },
    {
        path: "/friendlink",
        name: "friendlink",
        component: FriendLink,
    },
    {
        path: "/about",
        name: "about",
        component: About,
    },
];

const router = createRouter({
    history: createWebHistory(),
    routes,
});

export default router;
