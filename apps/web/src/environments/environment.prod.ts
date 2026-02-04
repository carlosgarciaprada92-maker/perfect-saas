export const environment = {
  production: true,
  apiBaseUrl:
    (globalThis as any).__env?.PERFECT_API_BASE_URL ??
    'https://api.perfect.example.com/api/v1'
};