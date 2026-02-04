import { InjectionToken } from '@angular/core';
import { environment } from '../../../environments/environment';

export const PERFECT_API_BASE_URL = new InjectionToken<string>('PERFECT_API_BASE_URL', {
  providedIn: 'root',
  factory: () => environment.apiBaseUrl
});