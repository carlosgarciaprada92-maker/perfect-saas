import { ModuleStatus } from './platform.model';

export interface WorkspaceApp {
  moduleId: string;
  name: string;
  slug: string;
  baseUrl: string;
  status: ModuleStatus;
  enabled: boolean;
}

export interface WorkspaceUser {
  id: string;
  name: string;
  email: string;
  roles: string[];
  isActive: boolean;
}
