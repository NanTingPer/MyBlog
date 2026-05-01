import { createApp } from 'vue';
import Admin from './Admin.vue';
import adminRouter from './router/adminRouter';
import './assets/css/admin.css';

const app = createApp(Admin);
app.use(adminRouter);
app.mount('#admin-app');