export const environment = {
  production: false,
  apiBaseUrl:
    (globalThis as any).__env?.PERFECT_API_BASE_URL ??
    'http://localhost:8080/api/v1'
};
