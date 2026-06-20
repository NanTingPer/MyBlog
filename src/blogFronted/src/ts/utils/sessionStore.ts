import { watch } from "vue";
import { ref } from "vue";

const JWT_KEY = 'mellow_admin_jwt';
const PASSWORD_KEY = 'mellow_admin_password';
const USERNAME_KEY = 'mellow_admin_username';
const ROLES_KEY = 'mellow_admin_roles';
let jwtToken = ref("");
let callBacks : Array<() => void> = [];
sessionStorage[JWT_KEY] = "";
//sessionStorage.getItem(JWT_KEY)
watch(() => jwtToken.value, () => {
    callBacks.forEach(c => c());
    console.log("监听");
})

export const sessionStore = {
    getJwt(): string | null {
        return sessionStorage.getItem(JWT_KEY);
    },

    setJwt(jwt: string): void {
        jwtToken.value = jwt;
        sessionStorage.setItem(JWT_KEY, jwt);
    },

    removeJwt(): void {
        jwtToken.value = "";
        sessionStorage.removeItem(JWT_KEY);
    },

    getPassword(): string | null {
        return sessionStorage.getItem(PASSWORD_KEY);
    },

    setPassword(password: string): void {
        sessionStorage.setItem(PASSWORD_KEY, password);
    },

    removePassword(): void {
        sessionStorage.removeItem(PASSWORD_KEY);
    },

    getUserName(): string | null {
        return sessionStorage.getItem(USERNAME_KEY);
    },

    setUserName(userName: string): void {
        sessionStorage.setItem(USERNAME_KEY, userName);
    },

    removeUserName(): void {
        sessionStorage.removeItem(USERNAME_KEY);
    },

    getRoles(): string[] {
        const roles = sessionStorage.getItem(ROLES_KEY);
        if (roles) {
            try {
                return JSON.parse(roles);
            } catch {
                return [];
            }
        }
        return [];
    },

    setRoles(roles: string[]): void {
        sessionStorage.setItem(ROLES_KEY, JSON.stringify(roles));
    },

    removeRoles(): void {
        sessionStorage.removeItem(ROLES_KEY);
    },

    isAdmin(): boolean {
        return this.getRoles().includes('Admin');
    },

    clearAll(): void {
        this.removeJwt();
        this.removePassword();
        this.removeUserName();
        this.removeRoles();
    },

    isLoggedIn(): boolean {
        return !!this.getJwt();
    },

    addJWTChangeCallback(callback: () => void): void {
        callBacks.push(callback);
    },

    removeJWTChangeCallback(callback: () => void): void {
        callBacks = callBacks.filter(c => c.toString() != callback.toString());
    }
};