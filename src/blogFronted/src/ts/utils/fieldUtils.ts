import type { FieldConfig } from '../../components/ObjectForm.vue';

/**
 * 通用隐藏字段集合（无业务意义或由后端自动管理的字段）
 * 表格和表单均跳过这些 key
 */
const HIDDEN_KEYS = new Set([
    'id',
    'delete',
    'userId',
    'user',
    'createUnixEpochTick',
]);

/**
 * 长文本字段（使用 textarea 渲染）
 */
const TEXTAREA_KEYS = new Set([
    'content',
    'html',
    'description',
    'dictum',
    'failingText',
]);

/**
 * 只读字段（仅编辑模式展示，不可修改）
 */
const READONLY_KEYS = new Set([
    'createTime',
    'editTime',
]);

/**
 * 下拉选择字段（需要外部传入 options）
 */
const SELECT_KEYS = new Set([
    'state',
]);

/**
 * 从数据对象自动生成 ObjectForm 所需的 FieldConfig[]
 * @param data 后端返回的单条数据对象
 * @param extraHidden 额外需要隐藏的字段 key
 * @param selectOptions select 字段的选项映射，key 为字段名，value 为选项列表
 */
export function generateFields(
    data: Record<string, any>,
    extraHidden: string[] = [],
    selectOptions: Record<string, string[]> = {},
): FieldConfig[] {
    const hidden = new Set([...HIDDEN_KEYS, ...extraHidden]);
    const fields: FieldConfig[] = [];

    for (const [key, value] of Object.entries(data)) {
        if (hidden.has(key)) continue;

        const field: FieldConfig = {
            key,
            label: key,
            type: inferType(key, value),
            order: inferOrder(key),
            hideOnAdd: READONLY_KEYS.has(key),
        };
        // select 类型注入选项
        if (field.type === 'select' && selectOptions[key]) {
            field.options = selectOptions[key];
        }
        fields.push(field);
    }

    return fields.sort((a, b) => (a.order ?? 0) - (b.order ?? 0));
}

/**
 * 从数据对象推断表格应展示的列 key[]
 * @param data 后端返回的单条数据对象
 * @param extraHidden 额外需要隐藏的字段 key
 */
export function generateTableColumns(
    data: Record<string, any>,
    extraHidden: string[] = [],
): string[] {
    const hidden = new Set([...HIDDEN_KEYS, ...extraHidden]);
    return Object.keys(data).filter(k => !hidden.has(k));
}

/**
 * 根据字段名和值推断 FieldConfig.type
 */
function inferType(key: string, value: unknown): FieldConfig['type'] {
    if (READONLY_KEYS.has(key)) return 'readonly';
    if (SELECT_KEYS.has(key)) return 'select';
    if (TEXTAREA_KEYS.has(key)) return 'textarea';
    if (Array.isArray(value)) return 'array';
    return 'text';
}

/**
 * 根据字段名推断排序权重（越小越靠前）
 */
function inferOrder(key: string): number {
    // 名称类字段排最前
    if (key === 'name' || key === 'title') return -10;
    // 内容类字段
    if (key === 'content' || key === 'html') return 50;
    // 时间类字段排最后
    if (key === 'createTime' || key === 'editTime') return 90;
    if (key === 'state') return 95;
    return 0;
}

/**
 * 格式化单元格显示值
 * - 数组：逗号分隔
 * - 对象：JSON.stringify
 * - null/undefined：空字符串
 */
export function formatCellValue(value: unknown): string {
    if (value === null || value === undefined) return '';
    if (Array.isArray(value)) return value.join(', ');
    if (typeof value === 'object') return JSON.stringify(value);
    return String(value);
}
