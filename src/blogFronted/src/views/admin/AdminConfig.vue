<template>
    <div class="config-container">
        <div class="page-header">
            <h1 class="page-title">系统配置</h1>
        </div>

        <div v-if="loading" class="empty-state">加载中...</div>

        <div v-else-if="loadError" class="empty-state">
            <p class="text-red-500">{{ loadError }}</p>
            <button class="btn-save mt-4" @click="loadConfig">重试</button>
        </div>

        <form v-else class="config-form" @submit.prevent="handleSubmit">
            <template v-for="(value, key) in formData" :key="key">
                <!-- 子对象递归渲染 -->
                <template v-if="isPlainObject(value)">
                    <div class="config-object-title">
                        <span>{{ key }}</span>
                    </div>
                    <template v-for="(subValue, subKey) in value" :key="`${key}.${subKey}`">
                        <div class="form-group">
                            <label :for="`${key}.${subKey}`">{{ subKey }}</label>
                            <input
                                v-if="getInputType(subKey, subValue) === 'password'"
                                :id="`${key}.${subKey}`"
                                type="password"
                                class="form-input"
                                :placeholder="getPlaceholder(subKey, subValue)"
                                :value="subValue ?? ''"
                                @input="onFieldInput(key, subKey, ($event.target as HTMLInputElement).value, subValue)"
                            />
                            <input
                                v-else-if="getInputType(subKey, subValue) === 'checkbox'"
                                :id="`${key}.${subKey}`"
                                type="checkbox"
                                class="config-checkbox"
                                :checked="!!subValue"
                                @change="onCheckboxInput(key, subKey, ($event.target as HTMLInputElement).checked, subValue)"
                            />
                            <input
                                v-else-if="getInputType(subKey, subValue) === 'number'"
                                :id="`${key}.${subKey}`"
                                type="number"
                                class="form-input"
                                :value="subValue ?? ''"
                                @input="onFieldInput(key, subKey, ($event.target as HTMLInputElement).value, subValue)"
                            />
                            <input
                                v-else
                                :id="`${key}.${subKey}`"
                                type="text"
                                class="form-input"
                                :placeholder="getPlaceholder(subKey, subValue)"
                                :value="subValue ?? ''"
                                @input="onFieldInput(key, subKey, ($event.target as HTMLInputElement).value, subValue)"
                            />
                        </div>
                    </template>
                </template>

                <!-- 数组类型 -->
                <template v-else-if="Array.isArray(value)">
                    <div class="form-group">
                        <label :for="String(key)">{{ key }}</label>
                        <div class="tag-input-container">
                            <span v-for="(item, idx) in value" :key="idx" class="tag-item">
                                {{ item }}
                                <span class="tag-remove" @click="removeArrayItem(key, idx)">&times;</span>
                            </span>
                            <input
                                :id="String(key)"
                                type="text"
                                class="tag-input"
                                placeholder="输入后按 Enter 添加"
                                @keydown.enter.prevent="addArrayItem(key, ($event.target as HTMLInputElement))"
                            />
                        </div>
                    </div>
                </template>

                <!-- 简单类型 -->
                <template v-else>
                    <div class="form-group">
                        <label :for="String(key)">{{ key }}</label>
                        <input
                            v-if="getInputType(key, value) === 'password'"
                            :id="String(key)"
                            type="password"
                            class="form-input"
                            :placeholder="getPlaceholder(key, value)"
                            :value="value ?? ''"
                            @input="onSimpleFieldInput(key, ($event.target as HTMLInputElement).value, value)"
                        />
                        <input
                            v-else-if="getInputType(key, value) === 'checkbox'"
                            :id="String(key)"
                            type="checkbox"
                            class="config-checkbox"
                            :checked="!!value"
                            @change="onSimpleCheckboxInput(key, ($event.target as HTMLInputElement).checked, value)"
                        />
                        <input
                            v-else-if="getInputType(key, value) === 'number'"
                            :id="String(key)"
                            type="number"
                            class="form-input"
                            :value="value ?? ''"
                            @input="onSimpleFieldInput(key, ($event.target as HTMLInputElement).value, value)"
                        />
                        <input
                            v-else
                            :id="String(key)"
                            type="text"
                            class="form-input"
                            :placeholder="getPlaceholder(key, value)"
                            :value="value ?? ''"
                            @input="onSimpleFieldInput(key, ($event.target as HTMLInputElement).value, value)"
                        />
                    </div>
                </template>
            </template>

            <p class="config-tip">留空的字段将不会被修改，修改后可能需要重启服务生效。</p>

            <div v-if="submitResult" :class="submitSuccess ? 'config-msg-success' : 'config-msg-error'">
                {{ submitResult }}
            </div>

            <div class="form-actions">
                <button type="submit" class="btn-save" :disabled="submitting">
                    {{ submitting ? '保存中...' : '保存配置' }}
                </button>
            </div>
        </form>
    </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { ConfigAPI } from '../../ts/utils/ConfigAPI';

const loading = ref(true);
const loadError = ref('');
const submitting = ref(false);
const submitResult = ref('');
const submitSuccess = ref(false);

// 原始数据（GET返回的初始值）
let originalData: Record<string, unknown> = {};
// 表单数据（可修改的副本）
const formData = ref<Record<string, unknown>>({});

// 敏感字段关键词
const sensitiveKeys = ['password', 'key', 'connectionstring', 'authorizationcode'];

function isSensitiveKey(key: unknown): boolean {
    const k = String(key).toLowerCase();
    return sensitiveKeys.some(s => k.includes(s));
}

function isPlainObject(val: unknown): val is Record<string, unknown> {
    return typeof val === 'object' && val !== null && !Array.isArray(val);
}

function getInputType(key: unknown, value: unknown): string {
    if (typeof value === 'boolean') return 'checkbox';
    if (typeof value === 'number') return 'number';
    if (typeof value === 'string' && isSensitiveKey(key)) return 'password';
    return 'text';
}

function getPlaceholder(key: unknown, value: unknown): string {
    if (typeof value === 'string' && value === '') {
        return '留空不更改';
    }
    if (isSensitiveKey(key) && typeof value === 'string' && value !== '') {
        return '留空不更改';
    }
    return '';
}

/**
 * 深拷贝对象
 */
function deepClone<T>(obj: T): T {
    return JSON.parse(JSON.stringify(obj));
}

/**
 * 加载配置
 */
async function loadConfig() {
    loading.value = true;
    loadError.value = '';
    try {
        const result = await ConfigAPI.getConfig();
        if (result && result.data) {
            // 移除不需要显示的顶层字段
            const data = { ...result.data };
            delete data.code;
            delete data.msg;
            originalData = deepClone(data);
            formData.value = deepClone(data);
        } else {
            loadError.value = '获取配置失败';
        }
    } catch (e) {
        loadError.value = '获取配置失败';
    } finally {
        loading.value = false;
    }
}

/**
 * 子对象字段输入处理
 */
function onFieldInput(parentKey: unknown, subKey: unknown, newVal: string, originalVal: unknown) {
    const pk = String(parentKey);
    const sk = String(subKey);
    const parent = formData.value[pk] as Record<string, unknown>;

    if (typeof originalVal === 'number') {
        const parsed = Number(newVal);
        parent[sk] = isNaN(parsed) ? null : parsed;
    } else {
        parent[sk] = newVal === '' ? null : newVal;
    }
}

/**
 * 子对象复选框处理
 */
function onCheckboxInput(parentKey: unknown, subKey: unknown, checked: boolean, _originalVal: unknown) {
    const pk = String(parentKey);
    const sk = String(subKey);
    const parent = formData.value[pk] as Record<string, unknown>;
    parent[sk] = checked;
}

/**
 * 简单字段输入处理
 */
function onSimpleFieldInput(key: unknown, newVal: string, originalVal: unknown) {
    const k = String(key);
    if (typeof originalVal === 'number') {
        const parsed = Number(newVal);
        formData.value[k] = isNaN(parsed) ? null : parsed;
    } else {
        formData.value[k] = newVal === '' ? null : newVal;
    }
}

/**
 * 简单复选框处理
 */
function onSimpleCheckboxInput(key: unknown, checked: boolean, _originalVal: unknown) {
    formData.value[String(key)] = checked;
}

/**
 * 移除数组项
 */
function removeArrayItem(key: unknown, idx: number) {
    const k = String(key);
    const arr = formData.value[k] as unknown[];
    if (arr) {
        arr.splice(idx, 1);
    }
}

/**
 * 添加数组项
 */
function addArrayItem(key: unknown, input: HTMLInputElement) {
    const k = String(key);
    const val = input.value.trim();
    if (!val) return;
    const arr = formData.value[k] as unknown[];
    if (arr) {
        arr.push(val);
    }
    input.value = '';
}

/**
 * 构建提交对象：只包含已更改的字段，未更改的字段不加入
 */
function buildSubmitData(): Record<string, unknown> {
    const result: Record<string, unknown> = {};

    for (const key of Object.keys(formData.value)) {
        const current = formData.value[key];
        const original = originalData[key];

        if (isPlainObject(current) && isPlainObject(original)) {
            // 子对象：递归比较每个子字段
            const subResult: Record<string, unknown> = {};
            let hasChanges = false;
            for (const subKey of Object.keys(current)) {
                const subCurrent = current[subKey];
                const subOriginal = original[subKey];
                if (subCurrent !== subOriginal && subCurrent !== null) {
                    subResult[subKey] = subCurrent;
                    hasChanges = true;
                }
            }
            if (hasChanges) {
                result[key] = subResult;
            }
        } else if (Array.isArray(current) && Array.isArray(original)) {
            // 数组：比较序列化结果
            if (JSON.stringify(current) !== JSON.stringify(original)) {
                result[key] = current;
            }
        } else {
            // 简单类型：值不同且非null才加入
            if (current !== original && current !== null && current !== '') {
                result[key] = current;
            }
        }
    }

    return result;
}

/**
 * 提交保存
 */
async function handleSubmit() {
    if (submitting.value) return;

    const submitData = buildSubmitData();

    // 没有任何更改
    if (Object.keys(submitData).length === 0) {
        submitResult.value = '没有需要保存的更改';
        submitSuccess.value = false;
        return;
    }

    submitting.value = true;
    submitResult.value = '';

    try {
        const result = await ConfigAPI.updateConfig(submitData);
        if (result && result.code === 200) {
            submitResult.value = result.msg || '保存成功';
            submitSuccess.value = true;
            // 重新加载最新配置
            await loadConfig();
        } else {
            submitResult.value = result?.msg || '保存失败';
            submitSuccess.value = false;
        }
    } catch (e) {
        submitResult.value = '保存失败，请检查网络';
        submitSuccess.value = false;
    } finally {
        submitting.value = false;
    }
}

onMounted(() => {
    loadConfig();
});
</script>

<style scoped>
.config-container {
    padding: 24px;
}

.config-form {
    background: #fff;
    border-radius: 8px;
    padding: 24px;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
}

@media (max-width: 768px) {
    .config-container {
        padding: 16px;
        min-height: 100vh;
        background: #f5f5f5;
    }

    .config-form {
        border-radius: 12px;
        padding: 20px;
    }
}

.config-object-title {
    display: flex;
    align-items: center;
    margin: 1.5rem 0 0.75rem;
    padding-bottom: 0.5rem;
    border-bottom: 1px solid var(--color-border);
}

.config-object-title span {
    font-size: 16px;
    font-weight: 600;
    color: var(--color-text-secondary);
}

.config-checkbox {
    width: 1.25rem;
    height: 1.25rem;
    cursor: pointer;
}

.config-tip {
    font-size: 12px;
    color: var(--color-text-muted);
    margin-top: 1rem;
}

.config-msg-success {
    font-size: 14px;
    color: var(--color-primary-text);
    background: var(--color-primary-light-bg);
    padding: 0.75rem 1rem;
    border-radius: 6px;
    margin-top: 1rem;
}

.config-msg-error {
    font-size: 14px;
    color: var(--color-danger-text);
    background: var(--color-danger-light-bg);
    padding: 0.75rem 1rem;
    border-radius: 6px;
    margin-top: 1rem;
}

.tag-input-container {
    display: flex;
    flex-wrap: wrap;
    gap: 0.5rem;
    padding: 0.5rem;
    border: 1px solid var(--color-border);
    border-radius: 6px;
    min-height: 2.5rem;
    align-items: center;
}

.tag-input-container:focus-within {
    border-color: var(--color-primary);
}

.tag-item {
    display: inline-flex;
    align-items: center;
    gap: 0.25rem;
    background: var(--color-primary-light-bg);
    color: var(--color-primary-text);
    padding: 0.25rem 0.5rem;
    border-radius: 4px;
    font-size: 14px;
}

.tag-remove {
    cursor: pointer;
    font-size: 16px;
    line-height: 1;
    color: var(--color-text-muted);
}

.tag-remove:hover {
    color: var(--color-danger);
}

.tag-input {
    border: none;
    outline: none;
    font-size: 14px;
    flex: 1;
    min-width: 6rem;
    padding: 0.25rem;
}
</style>