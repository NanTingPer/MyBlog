import type { Friendslink } from "../types/friendlink/Friendslink";
import type { DeleteByIdInput } from "../types/friendlink/DeleteByIdInput";
import { apiFetch } from "../config/apiConfig";
import { AuthAPI } from "./AuthAPI";

const apiEndpoint = "/api/friendlink";

export class FriendlinkAPI {
    public static async getAll(): Promise<Response> {
        return apiFetch(`${apiEndpoint}/getall`, {
            method: "GET"
        });
    }

    /**
     * 根据当前用户Token获取友链（需要身份认证）
     * Admin返回全部，普通用户返回自己的友链
     */
    public static async getUserLink(): Promise<Response> {
        const authHeader = AuthAPI.getAuthorizationHeader();
        const headers: Record<string, string> = {};
        if (authHeader) {
            headers["Authorization"] = authHeader;
        }
        return apiFetch(`${apiEndpoint}/getUserLink`, {
            method: "GET",
            headers
        });
    }

    /**
     * 获取所有状态枚举字符串（需要身份认证）
     */
    public static async getStatuStrings(): Promise<Response> {
        const authHeader = AuthAPI.getAuthorizationHeader();
        const headers: Record<string, string> = {};
        if (authHeader) {
            headers["Authorization"] = authHeader;
        }
        return apiFetch(`${apiEndpoint}/getStatuStrings`, {
            method: "GET",
            headers
        });
    }

    public static async delete(input: DeleteByIdInput): Promise<Response> {
        const authHeader = AuthAPI.getAuthorizationHeader();
        const headers: Record<string, string> = {
            "Content-Type": "application/json"
        };
        
        if (authHeader) {
            headers["Authorization"] = authHeader;
        }

        return apiFetch(`${apiEndpoint}/delete`, {
            method: "POST",
            headers,
            body: JSON.stringify(input)
        });
    }

    public static async addOrUpdate(friendlink: Friendslink): Promise<Response> {
        const authHeader = AuthAPI.getAuthorizationHeader();
        const headers: Record<string, string> = {
            "Content-Type": "application/json"
        };
        
        if (authHeader) {
            headers["Authorization"] = authHeader;
        }

        return apiFetch(`${apiEndpoint}/addOrUpdate`, {
            method: "POST",
            headers,
            body: JSON.stringify(friendlink)
        });
    }
}