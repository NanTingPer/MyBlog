import { createApp } from 'vue'
import App from './App.vue'
import router from './router'

const app = createApp(App)
app.use(router)
app.mount('#app')

// 如何启动项目？
// npm install -g serve
// serve -s dist -l 9999 

// 如果在dist目录下不需要写dist 但是需要-s ，-l 用于指定端口