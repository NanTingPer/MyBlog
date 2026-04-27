export const API_BASE_URL = 'http://localhost:7777';

export function apiFetch(url: string, options: RequestInit = {}): Promise<Response> {
    const fullUrl = url.startsWith('/') ? `${API_BASE_URL}${url}` : url;
    const method = options.method?.toUpperCase() || 'GET';
    const defaultHeaders: Record<string, string> = {
        'Accept': 'application/json'
    };
    
    if (method !== 'GET' && method !== 'HEAD') {
        defaultHeaders['Content-Type'] = 'application/json';
    }
    
    const headers = { ...defaultHeaders, ...(options.headers as Record<string, string>) };
    return fetch(fullUrl, { ...options, headers });
}