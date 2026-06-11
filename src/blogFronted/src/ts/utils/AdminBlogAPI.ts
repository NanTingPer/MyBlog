import type { BlogInfo } from "../types/blogs/BlogInfo";
import type { SearchBlogInput } from "../types/blogs/SearchBlogInput";
import type { DeleteInput } from "../types/blogs/DeleteInput";
import { apiFetch } from "../config/apiConfig";
import { AuthAPI } from "./AuthAPI";

const apiEndpoint = "/api/blog";

export class AdminBlogAPI {
    public static async getAllToPage(input?: SearchBlogInput): Promise<Response> {
        let params = "";
        if (input) {
            const searchParams = new URLSearchParams();
            Object.entries(input).forEach(([k, v]) => {
                if (v != null && v != '') {
                    searchParams.append(k, String(v));
                }
            });
            const paramsText = searchParams.toString();
            if (paramsText) {
                params = `?${paramsText}`;
            }
        }

        return apiFetch(`${apiEndpoint}/getAllToPage${params}`, {
            method: "GET",
            headers: AdminBlogAPI.GetHeaders()
        });
    }

    /**
     * 从数据库实时获取页数（需要管理员权限）
     * 与 BlogAPI.GetPageCount 不同，此接口不走缓存，保证数据实时性
     * @param limit 每页数量
     * @returns Promise<Response>
     */
    public static async getTotalPages(limit: number): Promise<Response> {
        return apiFetch(`${apiEndpoint}/dbPageCount?limit=${limit}`, {
            method: "GET",
            headers: AdminBlogAPI.GetHeaders()
        });
    }

    public static async addOrReplace(post: BlogInfo): Promise<Response> {
        return apiFetch(`${apiEndpoint}/addOrReplace`, {
            method: "POST",
            headers: AdminBlogAPI.GetHeaders(),
            body: JSON.stringify(post)
        });
    }

    public static async delete(input: DeleteInput): Promise<Response> {
        return apiFetch(`${apiEndpoint}/delete`, {
            method: "POST",
            headers: AdminBlogAPI.GetHeaders(),
            body: JSON.stringify(input)
        });
    }

    private static GetHeaders() {
        const authHeader = AuthAPI.getAuthorizationHeader();
        const headers: Record<string, string> = {
            "Content-Type": "application/json"
        };

        if (authHeader) {
            headers["Authorization"] = authHeader;
        }
        return headers;
    }
}