# ConfirmDialog 确认对话框

## 简介

`ConfirmDialog` 是一个替代原生 `window.confirm()` 的 Vue 确认对话框组件。

与原生 `confirm()` 相比的优势：
- 视觉风格与应用一致，支持自定义标题、内容、按钮文案
- 支持 **danger 模式**（红色确认按钮），适用于删除等危险操作
- 支持 **loading 状态**，防止重复点击
- 点击遮罩层可关闭
- 带有淡入淡出动画
- 使用 `<Teleport>` 渲染到 `<body>`，避免父组件 z-index 或 overflow 影响

## 导入

```typescript
import ConfirmDialog from '../components/ConfirmDialog.vue'
```

## Props

| 属性 | 类型 | 必需 | 默认值 | 说明 |
|------|------|------|--------|------|
| `visible` | `boolean` | ✅ | - | 控制对话框是否显示，支持 `v-model:visible` 双向绑定 |
| `title` | `string` | ❌ | `'确认操作'` | 对话框标题 |
| `content` | `string` | ❌ | `'确定要执行此操作吗？'` | 对话框内容文本 |
| `confirmText` | `string` | ❌ | `'确定'` | 确认按钮文案 |
| `cancelText` | `string` | ❌ | `'取消'` | 取消按钮文案 |
| `danger` | `boolean` | ❌ | `false` | 是否为危险操作（确认按钮变为红色） |
| `loading` | `boolean` | ❌ | `false` | 确认按钮 loading 状态，为 true 时按钮禁用并显示"处理中..." |

## Events

| 事件名 | 参数 | 说明 |
|--------|------|------|
| `update:visible` | `value: boolean` | v-model:visible 双向绑定 |
| `confirm` | 无 | 用户点击确认按钮时触发 |
| `cancel` | 无 | 用户点击取消按钮或遮罩层时触发，对话框自动关闭 |

## 使用示例

### 示例 1：基础用法（替代原生 confirm）

```vue
<template>
    <button @click="showConfirm = true">删除</button>

    <ConfirmDialog
        v-model:visible="showConfirm"
        title="确认删除"
        content="确定要删除这条数据吗？"
        @confirm="handleDelete"
    />
</template>

<script setup lang="ts">
import { ref } from 'vue'
import ConfirmDialog from '../components/ConfirmDialog.vue'

const showConfirm = ref(false)

const handleDelete = async () => {
    await deleteItem()
    showConfirm.value = false
}
</script>
```

### 示例 2：危险操作（删除友链）

```vue
<template>
    <button class="btn-delete" @click="openDelete(link.id)">删除</button>

    <ConfirmDialog
        v-model:visible="showDeleteConfirm"
        title="确认删除友链"
        content="确定要删除这个友链吗？此操作不可撤销。"
        confirm-text="删除"
        :danger="true"
        :loading="deleting"
        @confirm="confirmDelete"
    />
</template>

<script setup lang="ts">
import ConfirmDialog from '../components/ConfirmDialog.vue'

const showDeleteConfirm = ref(false)
const deleting = ref(false)
const deleteTargetId = ref('')

const openDelete = (id: string) => {
    deleteTargetId.value = id
    showDeleteConfirm.value = true
}

const confirmDelete = async () => {
    deleting.value = true
    try {
        await FriendlinkAPI.delete({ id: deleteTargetId.value })
        showDeleteConfirm.value = false
        await loadFriendlinks()
    } catch (error) {
        alert('删除失败')
    } finally {
        deleting.value = false
    }
}
</script>
```

### 示例 3：自定义按钮文案

```vue
<ConfirmDialog
    v-model:visible="showPublishConfirm"
    title="发布文章"
    content="文章将对外可见，确认发布？"
    confirm-text="立即发布"
    cancel-text="再想想"
    @confirm="publishPost"
/>
```

## 使用模式

### 模式 A：手动控制关闭（推荐）

在 `@confirm` 事件中执行操作，成功后手动关闭对话框：

```typescript
const handleConfirm = async () => {
    loading.value = true
    try {
        await doSomething()
        showConfirm.value = false  // 成功后关闭
    } catch (e) {
        // 失败时保持打开，用户可重试
    } finally {
        loading.value = false
    }
}
```

### 模式 B：父组件自动关闭

如果操作是同步的或不需要 loading，可以在模板中直接关闭：

```vue
<ConfirmDialog
    v-model:visible="showConfirm"
    @confirm="handleAction; showConfirm = false"
/>
```

## 注意事项

1. **使用 `<Teleport>`**：对话框渲染在 `<body>` 下，不受父组件样式影响
2. **点击遮罩层关闭**：点击对话框外部区域会触发 `cancel` 事件并关闭对话框
3. **loading 状态**：loading 为 true 时，确认和取消按钮均禁用，防止误操作
4. **动画**：组件内置淡入 + 缩放动画，无需额外配置