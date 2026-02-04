export const environment = {
  production: true,
  apiBaseUrl: (globalThis as any).__env?.PERFECT_API_BASE_URL ?? '/api/v1'
};
