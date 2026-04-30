import { createRouter, createWebHistory } from "vue-router";
import LatestArticles from "../components/LatestArticles.vue";
import Articles from "../components/Articles.vue";
import Post from "../components/Post.vue";
import FriendLink from "../components/FriendLink.vue";

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
];

const router = createRouter({
    history: createWebHistory(),
    routes,
});

export default router;
