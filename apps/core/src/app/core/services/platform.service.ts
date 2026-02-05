import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import {
  AssignmentUpdateRequest,
  ModuleAssignment,
  ModuleCatalog,
  ModuleCatalogRequest,
  TenantStatusUpdate,
  TenantSummary
} from '../models/platform.model';

@Injectable({ providedIn: 'root' })
export class PlatformService {
  constructor(private readonly api: ApiService) {}

  listModules(): Observable<ModuleCatalog[]> {
    return this.api.get<ModuleCatalog[]>('/platform/modules');
  }

  createModule(request: ModuleCatalogRequest): Observable<ModuleCatalog> {
    return this.api.post<ModuleCatalog>('/platform/modules', request);
  }

  updateModule(id: string, request: ModuleCatalogRequest): Observable<ModuleCatalog> {
    return this.api.put<ModuleCatalog>(`/platform/modules/${id}`, request);
  }

  deleteModule(id: string): Observable<{ deleted: boolean }>{
    return this.api.delete<{ deleted: boolean }>(`/platform/modules/${id}`);
  }

  listTenants(search?: string): Observable<TenantSummary[]> {
    const params = search ? { search } : undefined;
    return this.api.get<TenantSummary[]>('/platform/tenants', params);
  }

  updateTenantStatus(id: string, status: TenantStatusUpdate): Observable<TenantSummary> {
    return this.api.put<TenantSummary>(`/platform/tenants/${id}/status`, status);
  }

  listAssignments(tenantId: string): Observable<ModuleAssignment[]> {
    return this.api.get<ModuleAssignment[]>('/platform/assignments', { tenantId });
  }

  updateAssignments(request: AssignmentUpdateRequest): Observable<{ updated: boolean }>{
    return this.api.put<{ updated: boolean }>('/platform/assignments', request);
  }
}
