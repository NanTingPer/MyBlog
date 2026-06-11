import { createApp } from 'vue'
import App from './App.vue'
import router from './router'

const app = createApp(App)
app.use(router)
app.mount('#app')

// import './assets/css/index.css'
import('./assets/css/index.css');
import('./assets/js/prism.js' as any);
import('./assets/css/prism.css');

import('katex/dist/katex.min.css');
import('katex').then(k => (window as any).katex = k.default);
import('katex/contrib/auto-render/auto-render.ts').then(k => (window as any).randerMathInElement = k.default);

// 如何启动项目？
// npm install -g http-server
// http-server .\dist\ -p 8080

// 如果在dist目录下不需要写dist 但是需要-s ，-l 用于指定端口

// git reset --hard HEAD~1
// git push --force-with-lease origin main