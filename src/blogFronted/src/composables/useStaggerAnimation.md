# 交错进入动画（Stagger Animation）

> 模块位置：`src/composables/useStaggerAnimation.ts`
> 组件位置：`src/components/StaggerTransition.vue`

## 概述

为 Vue 应用提供「由上往下、依次渐入」的列表/内容动画效果。

包含两个层次的抽象：

| 抽象 | 说明 | 适用场景 |
|------|------|----------|
| `useStaggerAnimation` | Composable 函数，返回 Vue Transition 钩子 | 需要自行控制 `<Transition>` / `<TransitionGroup>` 的场景 |
| `StaggerTransition` | 包装组件，内置旋转加载圈 + TransitionGroup | 列表加载动画，直接替换列表容器 |

**核心动画效果**：每个子元素从 `opacity: 0` + `translateY(-20px)` 逐渐变为 `opacity: 1` + `translateY(0)`，每个元素比前一个延迟 `80ms` 出现，实现由上往下的视觉效果。

底层使用 **Web Animations API**（`el.animate()`），通过 `animation.finished` Promise 精确通知 Vue 动画结束。

---

## useStaggerAnimation

### 签名

```ts
function useStaggerAnimation(
    duration?: number,   // 动画时长，默认 400ms
    delayStep?: number,  // 每项延迟增量，默认 80ms
    offset?: number      // 垂直偏移量，默认 20px
): {
    onBeforeEnter: (el: Element) => void;
    onEnter: (el: Element, done: () => void) => void;
}
```

### 使用方式

#### 1. TransitionGroup（列表动画）

子元素必须添加 `data-index` 属性，用于计算交错延迟：

```vue
<template>
    <TransitionGroup appear tag="div"
        @before-enter="onBeforeEnter" @enter="onEnter">
        <div v-for="(item, index) in items" :key="item.id" :data-index="index">
            {{ item.name }}
        </div>
    </TransitionGroup>
</template>

<script setup lang="ts">
import { useStaggerAnimation } from '@/composables/useStaggerAnimation';

const { onBeforeEnter, onEnter } = useStaggerAnimation();
</script>
```

#### 2. Transition（单元素淡入）

单元素场景无交错效果，但同样使用 composable 保持一致性：

```vue
<template>
    <Transition appear @before-enter="onBeforeEnter" @enter="onEnter">
        <div v-if="visible">内容</div>
    </Transition>
</template>

<script setup lang="ts">
import { useStaggerAnimation } from '@/composables/useStaggerAnimation';

const { onBeforeEnter, onEnter } = useStaggerAnimation();
</script>
```

---

## StaggerTransition

### Props

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `loading` | `boolean` | `false` | 是否显示加载旋转圈；为 `false` 时展示列表动画 |
| `duration` | `number` | `400` | 单个元素动画时长（ms） |
| `delayStep` | `number` | `80` | 每个元素间的延迟增量（ms） |
| `offset` | `number` | `20` | 垂直偏移量（px） |

### Slots

| 插槽 | 说明 |
|------|------|
| `default` | 列表项，**每个直接子元素必须添加 `data-index` 属性** |

### 使用示例

```vue
<template>
    <StaggerTransition :loading="loading">
        <div v-for="(item, index) in items" :key="item.id" :data-index="index" class="card">
            {{ item.name }}
        </div>
    </StaggerTransition>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import StaggerTransition from '@/components/StaggerTransition.vue';

const items = ref([]);
const loading = ref(true);

onMounted(async () => {
    try {
        const res = await fetch('/api/items');
        items.value = await res.json();
    } finally {
        loading.value = false;
    }
});
</script>
```

---

## 实现原理

```
onBeforeEnter          onEnter
    │                     │
    ▼                     ▼
设置初始状态          使用 Web Animations API
opacity: 0            el.animate(keyframes, {
translateY(-20px)       duration: 400,
                        delay: index * 80,
                        fill: 'forwards'
                      })
                        │
                        ▼
                      animation.finished
                        │
                        ▼
                      done() → 通知 Vue 动画结束
```

**为什么使用 Web Animations API 而非 CSS transition？**

- CSS transition 的 `transitionend` 事件在某些场景下不会触发（浏览器渲染合并问题）
- Web Animations API 的 `animation.finished` Promise 保证在动画完成后触发，更加可靠
- 可以通过 JS 参数精确控制 `delay`，无需在元素上设置 `transition-delay` CSS 属性