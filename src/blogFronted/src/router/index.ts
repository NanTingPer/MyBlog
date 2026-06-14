import { createRouter, createWebHistory } from "vue-router";
import LatestArticles from "../views/frontend/LatestPosts.vue";
import Articles from "../views/frontend/Posts.vue";
import Post from "../views/frontend/Post.vue";
import Tag from "../views/frontend/Tag.vue";
import FriendLink from "../views/frontend/FriendLink.vue";
import About from "../views/frontend/About.vue";
import Search from "../views/frontend/Search.vue";

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
    {
        path: "/tag/:name",
        name: "tag",
        component: Tag,
    },
    {
        path: "/search",
        name: "search",
        component: Search,
    },
];

const router = createRouter({
    history: createWebHistory(),
    routes,
});

export default router;
