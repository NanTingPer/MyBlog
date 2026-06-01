<!--
  ObjectForm.vue - 通用对象表单编辑器组件

  功能说明：
    根据传入的 fields 配置，自动渲染表单字段，支持 text/url/textarea/array/readonly/hidden 类型。
    通过 v-model 双向绑定表单数据对象，submit 事件交由父组件处理 API 调用。

  Props：
    - modelValue: Record<string, any>  必需，v-model 绑定的数据对象
    - fields: FieldConfig[]            必需，字段配置数组
    - title: string                    必需，表单标题（如"添加友链"）
    - isAddMode: boolean               可选，是否为新增模式（默认 false）
    - loading: boolean                 可选，保存按钮 loading 状态（默认 false）
    - saveText: string                 可选，保存按钮文案（默认"保存"）
    - cancelText: string               可选，取消按钮文案（默认"取消"）

  Events：
    - update:modelValue  v-model 双向绑定
    - submit(data)       点击保存时触发，传递当前表单数据
    - cancel             点击取消/返回时触发

  FieldConfig 字段类型说明：
    - text:     普通文本输入框
    - url:      URL 输入框
    - textarea: 多行文本域
    - array:    数组编辑器（标签展示 + 可展开的逐行编辑器）
    - readonly: 只读展示，不可编辑
    - hidden:   完全不渲染

  使用示例见 ObjectForm.md
-->
<template>
    <div class="form-section">
        <div class="form-header">
            <button class="btn-back" @click="handleCancel">‹ 返回</button>
            <h2 class="form-title">{{ title }}</h2>
        </div>

        <form class="friendlink-form" @submit.prevent="handleSubmit">
            <div v-for="field in visibleFields" :key="field.key" class="form-group">
                <label>{{ field.label }}</label>

                <!-- readonly 只读展示 -->
                <div v-if="field.type === 'readonly'" class="form-readonly">
                    <p class="form-readonly-text">{{ getDisplayValue(field) }}</p>
                </div>

                <!-- text 输入框 -->
                <input
                    v-else-if="field.type === 'text'"
                    type="text"
                    :value="localData[field.key]"
                    class="form-input"
                    :placeholder="field.placeholder || `请输入${field.label}`"
                    :required="field.required"
                    @input="updateField(field.key, ($event.target as HTMLInputElement).value)"
                />

                <!-- url 输入框 -->
                <input
                    v-else-if="field.type === 'url'"
                    type="url"
                    :value="localData[field.key]"
                    class="form-input"
                    :placeholder="field.placeholder || `请输入${field.label}`"
                    :required="field.required"
                    @input="updateField(field.key, ($event.target as HTMLInputElement).value)"
                />

                <!-- textarea 多行文本域 -->
                <div v-else-if="field.type === 'textarea'" class="textarea-wrapper">
                    <div v-show="!getEditorState(field.key)" class="content-preview" @click="setEditorState(field.key, true)">
                        <p class="content-preview-text">{{ localData[field.key] || '暂无内容，点击编辑' }}</p>
                        <span class="btn-toggle-edit">编辑{{ field.label }}</span>
                    </div>
                    <div v-show="getEditorState(field.key)" class="content-editor">
                        <textarea
                            :value="localData[field.key]"
                            class="form-textarea"
                            :placeholder="field.placeholder || `请输入${field.label}`"
                            rows="12"
                            @input="updateField(field.key, ($event.target as HTMLTextAreaElement).value)"
                        ></textarea>
                        <span class="btn-toggle-edit" @click="setEditorState(field.key, false)">收起编辑</span>
                    </div>
                </div>

                <!-- array 数组编辑器 -->
                <div v-else-if="field.type === 'array'" class="array-editor">
                    <!-- 当前数组值展示（标签形式） -->
                    <div class="array-display">
                        <span
                            v-for="(item, index) in getArrayValue(field.key)"
                            :key="index"
                            class="array-tag"
                        >
                            {{ item }}
                            <button type="button" class="array-tag-remove" @click="removeArrayItem(field.key, index)">×</button>
                        </span>
                        <span v-if="getArrayValue(field.key).length === 0" class="array-empty">暂无{{ field.label }}</span>
                    </div>
                    <!-- 展开/收起编辑器按钮 -->
                    <button type="button" class="btn-toggle-edit" @click="toggleArrayEditor(field.key)">
                        {{ getEditorState(field.key) ? '收起编辑' : `编辑${field.label}` }}
                    </button>
                    <!-- 数组逐行编辑器 -->
                    <div v-show="getEditorState(field.key)" class="array-editor-panel">
                        <div class="array-editor-items">
                            <div
                                v-for="(_, index) in getArrayEditing(field.key)"
                                :key="index"
                                class="array-editor-row"
                            >
                                <input
                                    type="text"
                                    :value="getArrayEditing(field.key)[index]"
                                    class="form-input array-input"
                                    :placeholder="field.placeholder || `请输入${field.label}`"
                                    @input="updateArrayEditing(field.key, index, ($event.target as HTMLInputElement).value)"
                                />
                                <button type="button" class="btn-array-remove" @click="removeArrayEditingItem(field.key, index)">删除</button>
                            </div>
                        </div>
                        <div class="array-editor-actions">
                            <button type="button" class="btn-array-add" @click="addArrayEditingItem(field.key)">+ 添加{{ field.label }}</button>
                            <button type="button" class="btn-array-confirm" @click="confirmArrayEdit(field.key)">确认</button>
                        </div>
                    </div>
                </div>
            </div>

            <div class="form-actions">
                <button type="submit" class="btn-save" :disabled="loading">
                    {{ loading ? '保存中...' : saveText }}
                </button>
                <button type="button" class="btn-cancel" @click="handleCancel" :disabled="loading">
                    {{ cancelText }}
                </button>
            </div>
        </form>
    </div>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue';

/**
 * 字段配置接口
 * @property key - 对象属性名
 * @property label - 表单中显示的标签文本
 * @property type - 字段渲染类型
 * @property required - 是否必填（默认 false）
 * @property placeholder - 输入框占位文本（默认根据 label 自动生成）
 * @property order - 排序权重，越小越靠前（默认 0）
 * @property hidden - 是否在新增模式下隐藏（如 id、createTime 等自动生成的字段）
 */
export interface FieldConfig {
    key: string;
    label: string;
    type: 'text' | 'url' | 'textarea' | 'array' | 'readonly' | 'hidden';
    required?: boolean;
    placeholder?: string;
    order?: number;
    /** 是否在新增模式下隐藏此字段，编辑模式仍显示（适用于 id、createTime 等） */
    hideOnAdd?: boolean;
}

const props = withDefaults(defineProps<{
    /** v-model 绑定的数据对象 */
    modelValue: Record<string, any>;
    /** 字段配置数组 */
    fields: FieldConfig[];
    /** 表单标题 */
    title: string;
    /** 是否为新增模式 */
    isAddMode?: boolean;
    /** 保存按钮 loading 状态 */
    loading?: boolean;
    /** 保存按钮文案 */
    saveText?: string;
    /** 取消按钮文案 */
    cancelText?: string;
}>(), {
    isAddMode: false,
    loading: false,
    saveText: '保存',
    cancelText: '取消',
});

const emit = defineEmits<{
    'update:modelValue': [data: Record<string, any>];
    'submit': [data: Record<string, any>];
    'cancel': [];
}>();

/**
 * 本地数据副本，避免直接修改 props
 * 通过 watch 与 modelValue 同步
 */
const localData = ref<Record<string, any>>({ ...props.modelValue });

/**
 * textarea / array 编辑器的展开/收起状态
 * key 为字段名，value 为是否展开
 */
const editorStates = ref<Record<string, boolean>>({});

/**
 * array 字段的临时编辑数据
 * key 为字段名，value 为正在编辑的数组
 */
const arrayEditing = ref<Record<string, string[]>>({});

/**
 * 监听 modelValue 变化，同步到本地数据
 * 同时重置编辑器状态
 */
watch(() => props.modelValue, (newVal) => {
    localData.value = { ...newVal };
    editorStates.value = {};
    arrayEditing.value = {};
}, { deep: true });

/**
 * 可见字段列表（按 order 排序，排除 hidden 类型）
 * 新增模式下排除 hideOnAdd 标记的字段
 */
const visibleFields = computed(() => {
    return props.fields
        .filter(f => {
            if (f.type === 'hidden') return false;
            if (props.isAddMode && f.hideOnAdd) return false;
            return true;
        })
        .sort((a, b) => (a.order ?? 0) - (b.order ?? 0));
});

/**
 * 更新单个字段值并触发 v-model 同步
 */
function updateField(key: string, value: any) {
    localData.value[key] = value;
    emit('update:modelValue', { ...localData.value });
}

/**
 * 获取编辑器展开/收起状态
 */
function getEditorState(key: string): boolean {
    return editorStates.value[key] ?? false;
}

/**
 * 设置编辑器展开/收起状态
 */
function setEditorState(key: string, state: boolean) {
    editorStates.value[key] = state;
}

/**
 * 获取 readonly 字段的显示值
 */
function getDisplayValue(field: FieldConfig): string {
    const val = localData.value[field.key];
    if (val === null || val === undefined) return '-';
    if (typeof val === 'object') return JSON.stringify(val);
    return String(val);
}

/**
 * 获取 array 字段的当前值
 */
function getArrayValue(key: string): string[] {
    const val = localData.value[key];
    return Array.isArray(val) ? val : [];
}

/**
 * 获取 array 字段的临时编辑数据
 * 首次访问时自动初始化为当前数组的副本
 */
function getArrayEditing(key: string): string[] {
    if (!arrayEditing.value[key]) {
        arrayEditing.value[key] = [...getArrayValue(key)];
    }
    return arrayEditing.value[key];
}

/**
 * 更新 array 编辑器中某一项的值
 */
function updateArrayEditing(key: string, index: number, value: string) {
    arrayEditing.value[key][index] = value;
}

/**
 * 删除 array 展示中的某一项（直接生效）
 */
function removeArrayItem(key: string, index: number) {
    const arr = [...getArrayValue(key)];
    arr.splice(index, 1);
    updateField(key, arr);
}

/**
 * 删除 array 编辑器中的某一项
 */
function removeArrayEditingItem(key: string, index: number) {
    arrayEditing.value[key].splice(index, 1);
}

/**
 * 向 array 编辑器末尾添加空项
 */
function addArrayEditingItem(key: string) {
    getArrayEditing(key).push('');
}

/**
 * 切换 array 编辑器的展开/收起
 * 展开时自动同步当前值到临时编辑数据
 */
function toggleArrayEditor(key: string) {
    const isExpanded = getEditorState(key);
    if (!isExpanded) {
        // 展开时同步当前数据
        arrayEditing.value[key] = [...getArrayValue(key)];
    }
    setEditorState(key, !isExpanded);
}

/**
 * 确认 array 编辑，过滤空值后更新字段
 */
function confirmArrayEdit(key: string) {
    const filtered = arrayEditing.value[key].filter(item => item.trim() !== '');
    updateField(key, filtered);
    setEditorState(key, false);
}

/**
 * 提交表单，触发 submit 事件
 */
function handleSubmit() {
    emit('submit', { ...localData.value });
}

/**
 * 取消表单，触发 cancel 事件
 */
function handleCancel() {
    emit('cancel');
}
</script>

<style scoped>
/* 只读展示 */
.form-readonly {
    padding: 12px 14px;
    background: #f8f9fa;
    border-radius: 8px;
    border: 1px solid #e8e8e8;
}

.form-readonly-text {
    font-size: 14px;
    color: #666;
    margin: 0;
    word-break: break-all;
}

/* textarea 编辑器包装 */
.textarea-wrapper {
    display: flex;
    flex-direction: column;
    gap: 8px;
}

/* 内容预览 */
.content-preview {
    padding: 12px 14px;
    background: #f8f9fa;
    border-radius: 8px;
    border: 1px solid #e8e8e8;
    cursor: pointer;
    transition: border-color 0.2s;
}

.content-preview:hover {
    border-color: #4caf50;
}

.content-preview-text {
    font-size: 14px;
    color: #333;
    margin: 0 0 8px 0;
    line-height: 1.6;
    max-height: 120px;
    overflow: hidden;
    word-break: break-all;
}

.content-editor {
    display: flex;
    flex-direction: column;
    gap: 8px;
}

/* 切换编辑按钮 */
.btn-toggle-edit {
    background: none;
    border: 1px solid #e0e0e0;
    border-radius: 6px;
    padding: 6px 14px;
    font-size: 13px;
    color: #666;
    cursor: pointer;
    transition: all 0.2s;
    margin-top: 4px;
    display: inline-block;
    width: fit-content;
}

.btn-toggle-edit:hover {
    border-color: #4caf50;
    color: #4caf50;
}

/* array 编辑器 */
.array-editor {
    display: flex;
    flex-direction: column;
    gap: 8px;
}

.array-display {
    display: flex;
    flex-wrap: wrap;
    gap: 8px;
    min-height: 28px;
    align-items: center;
}

.array-tag {
    display: inline-flex;
    align-items: center;
    gap: 4px;
    background: #e8f5e9;
    color: #2e7d32;
    padding: 4px 10px;
    border-radius: 12px;
    font-size: 13px;
}

.array-tag-remove {
    background: none;
    border: none;
    color: #c62828;
    font-size: 16px;
    cursor: pointer;
    padding: 0 2px;
    line-height: 1;
}

.array-empty {
    font-size: 13px;
    color: #999;
}

.array-editor-panel {
    margin-top: 12px;
    padding: 16px;
    background: #f8f9fa;
    border-radius: 8px;
    border: 1px solid #e8e8e8;
}

.array-editor-items {
    display: flex;
    flex-direction: column;
    gap: 8px;
    margin-bottom: 12px;
}

.array-editor-row {
    display: flex;
    gap: 8px;
    align-items: center;
}

.array-input {
    flex: 1;
}

.btn-array-remove {
    padding: 6px 14px;
    background: #ffebee;
    color: #d32f2f;
    border: none;
    border-radius: 4px;
    font-size: 13px;
    cursor: pointer;
    white-space: nowrap;
    transition: background 0.2s;
}

.btn-array-remove:hover {
    background: #ffcdd2;
}

.array-editor-actions {
    display: flex;
    gap: 8px;
    margin-top: 8px;
}

.btn-array-add {
    padding: 6px 14px;
    background: #e8f5e9;
    color: #2e7d32;
    border: none;
    border-radius: 4px;
    font-size: 13px;
    cursor: pointer;
    transition: background 0.2s;
}

.btn-array-add:hover {
    background: #c8e6c9;
}

.btn-array-confirm {
    padding: 6px 14px;
    background: #4caf50;
    color: #fff;
    border: none;
    border-radius: 4px;
    font-size: 13px;
    cursor: pointer;
    transition: background 0.2s;
}

.btn-array-confirm:hover {
    background: #43a047;
}

@media (max-width: 768px) {
    .array-editor-row {
        flex-direction: column;
    }

    .array-input {
        width: 100%;
    }
}
</style>