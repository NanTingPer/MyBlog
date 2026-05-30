import { apiFetch } from "../config/apiConfig";
import { sessionStore } from "./sessionStore";
import { JSEncrypt } from "jsencrypt";
import type { UserInput } from "../types/auth/UserInput";
import type { PublicKey } from "../types/auth/PublicKey";
import type { BaseResult } from "../types/auth/BaseResult";

const apiEndpoint = "/api/auth";

export class AuthAPI {
    /**
     * 获取 RSA 公钥
     */
    public static async getPublicKey(): Promise<PublicKey | null> {
        try {
            const response = await apiFetch(`${apiEndpoint}/public`, {
                method: "POST"
            });

            if (response.status === 200) {
                const data: BaseResult<PublicKey> = await response.json();
                if (data.code === 200 && data.data) {
                    return data.data;
                }
            }
            return null;
        } catch (error) {
            console.error('获取公钥失败:', error);
            return null;
        }
    }

    /**
     * 使用 RSA 公钥加密密码
     */
    public static encryptPassword(publicKey: string, password: string): string | null {
        const encrypt = new JSEncrypt();
        encrypt.setPublicKey(publicKey);
        const encrypted = encrypt.encrypt(password);
        return encrypted ? encrypted : null;
    }

    /**
     * 登录：获取公钥 → 加密密码 → 请求登录获取 token
     */
    public static async login(userName: string, password: string): Promise<BaseResult<string> | null> {
        // 1. 获取公钥
        const publicKey = await this.getPublicKey();
        if (!publicKey) {
            return null;
        }

        // 2. 加密密码
        const encryptedPassword = this.encryptPassword(publicKey.key, password);
        if (!encryptedPassword) {
            return null;
        }

        // 3. 登录请求
        try {
            const userInput: UserInput = {
                userName,
                password: encryptedPassword,
                rsaid: publicKey.requestId
            };

            const response = await apiFetch(`${apiEndpoint}/login`, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify(userInput)
            });

            if (response.status === 200) {
                const data: BaseResult<string> = await response.json();
                if (data.code === 200 && data.data) {
                    sessionStore.setJwt(data.data);
                    sessionStore.setUserName(userName);
                    sessionStore.setPassword(password);
                    return data;
                }
            }
            return null;
        } catch (error) {
            console.error('登录失败:', error);
            return null;
        }
    }

    /**
     * 使用保存的凭据刷新 token
     */
    public static async refreshToken(): Promise<boolean> {
        const userName = sessionStore.getUserName();
        const password = sessionStore.getPassword();
        if (!userName || !password) {
            return false;
        }

        const response = await this.login(userName, password);
        return response !== null;
    }

    /**
     * 获取 Authorization 头
     */
    public static getAuthorizationHeader(): string | null {
        const jwt = sessionStore.getJwt();
        if (!jwt) {
            return null;
        }
        return `Bearer ${jwt}`;
    }

    /**
     * 获取邮箱验证码
     */
    public static async getMailVerificationCode(mailAddress: string, userName?: string): Promise<BaseResult<{ id: string }> | null> {
        try {
            const response = await apiFetch(`${apiEndpoint}/getMailVerificationCode`, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({
                    mailAddress,
                    userName: userName || mailAddress
                })
            });

            if (response.status === 200) {
                const data: BaseResult<{ id: string }> = await response.json();
                if (data.code === 200 && data.data) {
                    return data;
                }
            }
            return null;
        } catch (error) {
            console.error('获取邮箱验证码失败:', error);
            return null;
        }
    }

    /**
     * 注册用户
     */
    public static async register(
        userName: string,
        password: string,
        mailAddress: string,
        mailId: string,
        mailCode: string
    ): Promise<BaseResult<string> | null> {
        // 1. 获取公钥
        const publicKey = await this.getPublicKey();
        if (!publicKey) {
            return null;
        }

        // 2. 加密密码
        const encryptedPassword = this.encryptPassword(publicKey.key, password);
        if (!encryptedPassword) {
            return null;
        }

        // 3. 注册请求
        try {
            const response = await apiFetch(`${apiEndpoint}/createUser`, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({
                    userName,
                    password: encryptedPassword,
                    rsaid: publicKey.requestId,
                    mailAddress,
                    mailId,
                    mailCode
                })
            });

            if (response.status === 200) {
                const data: BaseResult<string> = await response.json();
                return data;
            }
            return null;
        } catch (error) {
            console.error('注册失败:', error);
            return null;
        }
    }
}
