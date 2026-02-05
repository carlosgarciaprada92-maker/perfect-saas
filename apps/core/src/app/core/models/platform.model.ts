export type ModuleStatus = 'Active' | 'Beta' | 'Disabled';

export interface ModuleCatalog {
  id: string;
  name: string;
  slug: string;
  baseUrl: string;
  icon?: string | null;
  status: ModuleStatus;
  createdAt: string;
}

export interface ModuleCatalogRequest {
  name: string;
  slug: string;
  baseUrl: string;
  status: ModuleStatus;
  icon?: string | null;
}

export type TenantStatus = 'Active' | 'Suspended';

export interface TenantSummary {
  id: string;
  name: string;
  displayName?: string | null;
  slug: string;
  status: TenantStatus;
  plan: string;
  createdAt: string;
}

export interface TenantStatusUpdate {
  status: TenantStatus;
}

export interface ModuleAssignment {
  moduleId: string;
  name: string;
  slug: string;
  baseUrl: string;
  status: ModuleStatus;
  enabled: boolean;
  activatedAt?: string | null;
  notes?: string | null;
}

export interface AssignmentUpdateItem {
  moduleId: string;
  enabled: boolean;
  notes?: string | null;
}

export interface AssignmentUpdateRequest {
  tenantId: string;
  modules: AssignmentUpdateItem[];
}
