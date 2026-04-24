export const API_BASE_URL = 'http://localhost:5162';

export function apiFetch(url: string, options: RequestInit = {}): Promise<Response> {
    const fullUrl = url.startsWith('/') ? `${API_BASE_URL}${url}` : url;
    return fetch(fullUrl, options);
}