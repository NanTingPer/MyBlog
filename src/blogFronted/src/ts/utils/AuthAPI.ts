import { apiFetch } from "../config/apiConfig";
import { sessionStore } from "./sessionStore";

const apiEndpoint = "/api/auth";

export interface TokenResponse {
    code: number;
    data: string;
    message?: string;
}

export class AuthAPI {
    public static async getToken(password: string): Promise<TokenResponse | null> {
        try {
            const response = await apiFetch(`${apiEndpoint}/getToken`, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({ password })
            });

            if (response.status === 200) {
                const data = await response.json();
                if (data.code === 200 && data.data) {
                    sessionStore.setJwt(data.data);
                    sessionStore.setPassword(password);
                    return data;
                }
            }
            return null;
        } catch (error) {
            console.error('获取 token 失败:', error);
            return null;
        }
    }

    public static async refreshToken(): Promise<boolean> {
        const password = sessionStore.getPassword();
        if (!password) {
            return false;
        }

        const response = await this.getToken(password);
        return response !== null;
    }

    public static getAuthorizationHeader(): string | null {
        const jwt = sessionStore.getJwt();
        if (!jwt) {
            return null;
        }
        return `Bearer ${jwt}`;
    }
}