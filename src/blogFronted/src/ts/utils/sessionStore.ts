const JWT_KEY = 'mellow_admin_jwt';
const PASSWORD_KEY = 'mellow_admin_password';

export const sessionStore = {
    getJwt(): string | null {
        return sessionStorage.getItem(JWT_KEY);
    },

    setJwt(jwt: string): void {
        sessionStorage.setItem(JWT_KEY, jwt);
    },

    removeJwt(): void {
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

    clearAll(): void {
        this.removeJwt();
        this.removePassword();
    },

    isLoggedIn(): boolean {
        return !!this.getJwt();
    }
};