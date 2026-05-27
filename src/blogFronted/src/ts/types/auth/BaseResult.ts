/**
 * 控制器标准返回结果
 */
export interface BaseResult<T> {
    /** 内部执行结果 */
    code: number;
    /** 内部结果消息 */
    msg?: string;
    /** 请求要返回的结果 */
    data?: T;
}