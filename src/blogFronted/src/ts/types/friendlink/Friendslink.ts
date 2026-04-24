/**
 * 友链数据结构
 */
export interface Friendslink {
    /** 主键 */
    id?: string;
    /** 名称 */
    name?: string;
    /** 跳转链接 */
    url?: string;
    /** 格言 */
    dictum?: string;
    /** 头像url */
    avatar?: string;
    /** 创建时间戳 */
    createUnixEpochTick?: number;
    /** 创建时间字符串 */
    createTime?: string;
}