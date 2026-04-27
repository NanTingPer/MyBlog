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