import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api.model';
import { AuthTokens, LoginRequest, LoginResponse, UserSummary } from '../models/auth.model';
import { StorageService } from './storage.service';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly userKey = 'core_user';
  private readonly tokenKey = 'core_tokens';
  private readonly tenantKey = 'core_tenant_slug';

  private readonly currentUserSubject = new BehaviorSubject<UserSummary | null>(null);
  private readonly tokenSubject = new BehaviorSubject<AuthTokens | null>(null);

  readonly currentUser$ = this.currentUserSubject.asObservable();

  constructor(private readonly http: HttpClient, private readonly storage: StorageService) {
    const storedUser = this.storage.get<UserSummary | null>(this.userKey, null);
    const storedTokens = this.storage.get<AuthTokens | null>(this.tokenKey, null);
    if (storedUser?.email) {
      this.currentUserSubject.next(storedUser);
    }
    if (storedTokens?.accessToken) {
      this.tokenSubject.next(storedTokens);
    }
  }

  get user(): UserSummary | null {
    return this.currentUserSubject.value;
  }

  get accessToken(): string | null {
    return this.tokenSubject.value?.accessToken ?? null;
  }

  get tenantSlug(): string | null {
    return this.storage.get<string | null>(this.tenantKey, null);
  }

  setTenantSlug(slug: string | null): void {
    if (!slug) {
      this.storage.remove(this.tenantKey);
      return;
    }
    this.storage.set(this.tenantKey, slug);
  }

  isAuthenticated(): boolean {
    return !!this.tokenSubject.value?.accessToken;
  }

  hasRole(role: string): boolean {
    return this.user?.roles?.includes(role) ?? false;
  }

  login(request: LoginRequest): Observable<LoginResponse> {
    const url = `${environment.apiBaseUrl.replace(/\/$/, '')}/auth/login`;
    return this.http.post<ApiResponse<LoginResponse>>(url, request).pipe(
      map((res) => res.data),
      tap((response) => {
        const tokens: AuthTokens = {
          accessToken: response.accessToken,
          refreshToken: response.refreshToken,
          expiresAt: response.expiresAt
        };
        this.currentUserSubject.next(response.user);
        this.tokenSubject.next(tokens);
        this.storage.set(this.userKey, response.user);
        this.storage.set(this.tokenKey, tokens);
        if (request.tenantSlug) {
          this.storage.set(this.tenantKey, request.tenantSlug);
        }
      })
    );
  }

  logout(): void {
    this.currentUserSubject.next(null);
    this.tokenSubject.next(null);
    this.storage.remove(this.userKey);
    this.storage.remove(this.tokenKey);
    this.storage.remove(this.tenantKey);
  }
}
