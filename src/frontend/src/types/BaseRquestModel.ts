/**
 * 基础接口请求返回类型
 */
export interface BaseRequestModel<T> {
    /** 接口响应代码 */
    code: number | 200,
    /** 接口响应消息 */
    msg: string,
    /** 接口响应的数据 */
    data?: T
}