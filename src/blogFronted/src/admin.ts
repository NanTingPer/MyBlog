import { createApp } from 'vue';
import Admin from './Admin.vue';
import adminRouter from './router/adminRouter';

const app = createApp(Admin);
app.use(adminRouter);
app.mount('#admin-app');