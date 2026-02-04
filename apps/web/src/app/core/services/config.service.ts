import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { StorageService } from './storage.service';

export interface AppConfig {
  diasNuevoProducto: number;
  umbralProximoVencer: number;
  cargoAdminPct: number;
}

const DEFAULT_CONFIG: AppConfig = {
  diasNuevoProducto: 30,
  umbralProximoVencer: 5,
  cargoAdminPct: 2
};

@Injectable({ providedIn: 'root' })
export class ConfigService {
  private readonly storageKey = 'inv_config';
  private readonly configSubject = new BehaviorSubject<AppConfig>(DEFAULT_CONFIG);

  readonly config$ = this.configSubject.asObservable();

  constructor(private readonly storage: StorageService) {
    const stored = this.storage.get(this.storageKey, DEFAULT_CONFIG);
    this.configSubject.next(stored);
    this.persist();
  }

  get snapshot(): AppConfig {
    return this.configSubject.value;
  }

  update(partial: Partial<AppConfig>): void {
    this.configSubject.next({ ...this.configSubject.value, ...partial });
    this.persist();
  }

  private persist(): void {
    this.storage.set(this.storageKey, this.configSubject.value);
  }
}
