import type { Friendslink } from "../types/friendlink/Friendslink";
import type { DeleteByIdInput } from "../types/friendlink/DeleteByIdInput";
import { apiFetch } from "../config/apiConfig";

const apiEndpoint = "/api/friendlink";

export class FriendlinkAPI {
    public static async getAll(): Promise<Response> {
        return apiFetch(`${apiEndpoint}/getall`, {
            method: "GET"
        });
    }

    public static async delete(input: DeleteByIdInput): Promise<Response> {
        return apiFetch(`${apiEndpoint}/delete`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(input)
        });
    }

    public static async addOrUpdate(friendlink: Friendslink): Promise<Response> {
        return apiFetch(`${apiEndpoint}/addOrUpdate`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(friendlink)
        });
    }
}