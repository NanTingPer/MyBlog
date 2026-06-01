# ObjectForm 通用对象表单编辑器

## 简介

`ObjectForm` 是一个通用的对象表单编辑器组件，用于后台管理系统中对象的**新增**和**编辑**操作。

核心设计理念：传入一个对象和字段配置，组件自动渲染对应的表单界面。**组件本身不处理 API 调用**，而是通过事件将提交操作交由父组件完成，保持职责单一。

## 导入

```typescript
import ObjectForm from '../components/ObjectForm.vue'
import type { FieldConfig } from '../components/ObjectForm.vue'
```

## Props

| 属性 | 类型 | 必需 | 默认值 | 说明 |
|------|------|------|--------|------|
| `modelValue` | `Record<string, any>` | ✅ | - | v-model 绑定的数据对象 |
| `fields` | `FieldConfig[]` | ✅ | - | 字段配置数组 |
| `title` | `string` | ✅ | - | 表单标题（如"添加友链"、"编辑文章"） |
| `isAddMode` | `boolean` | ❌ | `false` | 是否为新增模式（影响 hideOnAdd 字段的显示） |
| `loading` | `boolean` | ❌ | `false` | 保存按钮 loading 状态，为 true 时按钮禁用 |
| `saveText` | `string` | ❌ | `'保存'` | 保存按钮文案 |
| `cancelText` | `string` | ❌ | `'取消'` | 取消按钮文案 |

## Events

| 事件名 | 参数 | 说明 |
|--------|------|------|
| `update:modelValue` | `data: Record<string, any>` | v-model 双向绑定，表单数据变化时触发 |
| `submit` | `data: Record<string, any>` | 用户点击"保存"按钮时触发，传递当前表单数据副本 |
| `cancel` | 无 | 用户点击"取消"或"返回"按钮时触发 |

## FieldConfig 字段配置

```typescript
interface FieldConfig {
    key: string        // 对象属性名，必须与 modelValue 中的 key 一致
    label: string      // 表单中显示的标签文本
    type: FieldType    // 字段渲染类型（见下表）
    required?: boolean // 是否必填（默认 false，原生 HTML 表单验证）
    placeholder?: string // 输入框占位文本（默认自动生成：`请输入${label}`）
    order?: number     // 排序权重，越小越靠前（默认 0）
    hideOnAdd?: boolean // 新增模式下隐藏此字段（适用于 id、createTime 等自动生成的字段）
}
```

### 支持的字段类型

| type | 渲染方式 | 适用场景 |
|------|----------|----------|
| `text` | 普通文本输入框 `<input type="text">` | 名称、标题等短文本 |
| `url` | URL 输入框 `<input type="url">` | 链接地址，浏览器原生 URL 格式校验 |
| `textarea` | 多行文本域，带预览/编辑切换 | 文章内容、描述等长文本 |
| `array` | 数组编辑器：标签展示 + 可展开的逐行编辑器 | 作者列表、标签列表等字符串数组 |
| `readonly` | 只读展示文本（灰色背景） | ID、创建时间等由系统生成的字段 |
| `hidden` | 完全不渲染 | 不需要用户看到的内部字段 |

## 使用示例

### 示例 1：友链管理（简单字段）

```vue
<template>
    <ObjectForm
        v-model="formData"
        :fields="friendlinkFields"
        :title="isAddMode ? '添加友链' : '编辑友链'"
        :is-add-mode="isAddMode"
        :loading="saving"
        @submit="handleSave"
        @cancel="cancelEdit"
    />
</template>

<script setup lang="ts">
import ObjectForm from '../components/ObjectForm.vue'
import type { FieldConfig } from '../components/ObjectForm.vue'

const friendlinkFields: FieldConfig[] = [
    { key: 'name', label: '名称', type: 'text', required: true },
    { key: 'url', label: '跳转链接', type: 'url', required: true },
    { key: 'dictum', label: '格言', type: 'text' },
    { key: 'avatar', label: '头像 URL', type: 'url' },
    { key: 'id', label: 'ID', type: 'readonly', order: 99, hideOnAdd: true },
]

const formData = ref({ name: '', url: '', dictum: '', avatar: '' })
const isAddMode = ref(false)
const saving = ref(false)

const handleSave = async (data: Record<string, any>) => {
    saving.value = true
    try {
        await FriendlinkAPI.addOrUpdate(data as Friendslink)
        alert(isAddMode.value ? '添加成功' : '更新成功')
    } finally {
        saving.value = false
    }
}
</script>
```

### 示例 2：文章管理（含数组和 textarea）

```vue
<template>
    <ObjectForm
        v-model="formData"
        :fields="postFields"
        :title="isAddMode ? '添加文章' : '编辑文章'"
        :is-add-mode="isAddMode"
        :loading="saving"
        @submit="handleSave"
        @cancel="cancelEdit"
    />
</template>

<script setup lang="ts">
const postFields: FieldConfig[] = [
    { key: 'name', label: '名称', type: 'text', required: true },
    { key: 'title', label: '标题', type: 'text' },
    { key: 'description', label: '描述', type: 'text' },
    { key: 'author', label: '作者', type: 'array' },
    { key: 'tag', label: '标签', type: 'array' },
    { key: 'content', label: '内容', type: 'textarea' },
    { key: 'id', label: 'ID', type: 'readonly', order: 99, hideOnAdd: true },
]

const formData = ref({
    name: '', title: '', description: '',
    author: [] as string[], tag: [] as string[], content: ''
})
</script>
```

## 字段排序

使用 `order` 属性控制字段显示顺序，数值越小越靠前：

```typescript
const fields: FieldConfig[] = [
    { key: 'name', label: '名称', type: 'text', order: 1 },
    { key: 'id', label: 'ID', type: 'readonly', order: 99 },
    // order 默认为 0，不设置则按数组原始顺序
]
```

## hideOnAdd 用法

某些字段（如 `id`、`createTime`）由系统自动生成，新增时无需显示，但编辑时需要查看：

```typescript
const fields: FieldConfig[] = [
    { key: 'name', label: '名称', type: 'text' },
    { key: 'id', label: 'ID', type: 'readonly', hideOnAdd: true },
    // 新增模式下不显示 ID 字段，编辑模式下显示为只读
]
```

## 与 ConfirmDialog 配合

推荐搭配 `ConfirmDialog` 组件实现删除确认：

```vue
<ConfirmDialog
    v-model:visible="showDeleteConfirm"
    title="确认删除"
    content="确定要删除这条数据吗？此操作不可撤销。"
    :danger="true"
    @confirm="handleDelete"
/>
```

## 注意事项

1. **组件不处理 API**：`submit` 事件仅传递数据，由父组件决定如何调用 API
2. **数据隔离**：组件内部维护 `localData` 副本，不会直接修改父组件传入的 `modelValue`
3. **编辑器状态重置**：当 `modelValue` 发生变化时，所有 textarea/array 编辑器的展开状态会自动重置
4. **数组字段**：`type: 'array'` 要求对应的 `modelValue[key]` 是 `string[]` 类型