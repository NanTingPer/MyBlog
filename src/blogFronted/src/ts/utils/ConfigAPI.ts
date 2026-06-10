import { apiFetch } from "../config/apiConfig";
import { AuthAPI } from "./AuthAPI";
import { JSEncrypt } from "jsencrypt";
import type { BaseResult } from "../types/auth/BaseResult";

const apiEndpoint = "/config";

export class ConfigAPI {
    /**
     * 获取全局配置（脱敏后）
     */
    public static async getConfig(): Promise<BaseResult<Record<string, unknown>> | null> {
        const authHeader = AuthAPI.getAuthorizationHeader();
        const headers: Record<string, string> = {};

        if (authHeader) {
            headers["Authorization"] = authHeader;
        }

        try {
            const response = await apiFetch(`${apiEndpoint}/getConfig`, {
                method: "GET",
                headers
            });

            if (response.status === 200) {
                const data: BaseResult<Record<string, unknown>> = await response.json();
                if (data.code === 200) {
                    return data;
                }
            }
            return null;
        } catch (error) {
            console.error('获取配置失败:', error);
            return null;
        }
    }

    /**
     * 更新配置：获取RSA公钥 → 加密配置JSON → 提交
     */
    public static async updateConfig(config: Record<string, unknown>): Promise<BaseResult<string> | null> {
        // 1. 获取RSA公钥
        const publicKey = await AuthAPI.getPublicKey();
        if (!publicKey) {
            console.error('获取RSA公钥失败');
            return null;
        }

        // 2. 加密配置JSON
        const configJson = JSON.stringify(config);
        const encrypt = new JSEncrypt();
        encrypt.setPublicKey(publicKey.key);
        const encrypted = encrypt.encrypt(configJson);
        if (!encrypted) {
            console.error('RSA加密失败');
            return null;
        }

        // 3. 提交更新
        const authHeader = AuthAPI.getAuthorizationHeader();
        const headers: Record<string, string> = {
            "Content-Type": "application/json"
        };

        if (authHeader) {
            headers["Authorization"] = authHeader;
        }

        try {
            const response = await apiFetch(`${apiEndpoint}/update`, {
                method: "POST",
                headers,
                body: JSON.stringify({
                    config: encrypted,
                    requestId: publicKey.requestId
                })
            });

            if (response.status === 200) {
                const data: BaseResult<string> = await response.json();
                return data;
            }
            return null;
        } catch (error) {
            console.error('更新配置失败:', error);
            return null;
        }
    }
}