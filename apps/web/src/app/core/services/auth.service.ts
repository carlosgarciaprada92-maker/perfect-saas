import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { RolUsuario, Usuario } from '../models/usuario.model';
import { RolesService } from './roles.service';
import { StorageService } from './storage.service';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly storageKey = 'inv_current_user';
  private readonly currentUserSubject = new BehaviorSubject<Usuario | null>(null);

  readonly currentUser$ = this.currentUserSubject.asObservable();
  readonly demoMode = false;

  constructor(private readonly storage: StorageService, private readonly roles: RolesService) {
    const stored = this.storage.get<Usuario | null>(this.storageKey, null);
    if (stored && !stored.email) {
      this.storage.remove(this.storageKey);
      this.currentUserSubject.next(null);
      return;
    }
    this.currentUserSubject.next(stored);
  }

  get snapshot(): Usuario | null {
    return this.currentUserSubject.value;
  }

  isAuthenticated(): boolean {
    return !!this.snapshot;
  }

  loginWithEmail(email: string): Usuario | null {
    const existing = this.roles.findByEmail(email);
    if (!existing) {
      return null;
    }
    const resolved: Usuario = { ...existing };
    this.currentUserSubject.next(resolved);
    this.storage.set(this.storageKey, resolved);
    this.roles.upsertUsuario(resolved);
    return resolved;
  }

  logout(): void {
    this.currentUserSubject.next(null);
    this.storage.remove(this.storageKey);
  }

  switchRole(rol: RolUsuario): void {
    const current = this.snapshot;
    if (!current) {
      return;
    }
    const updated = { ...current, rol };
    this.currentUserSubject.next(updated);
    this.storage.set(this.storageKey, updated);
    this.roles.updateUsuario(updated.id, updated);
  }
}
