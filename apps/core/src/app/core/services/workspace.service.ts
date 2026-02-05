import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { WorkspaceApp, WorkspaceUser } from '../models/workspace.model';

@Injectable({ providedIn: 'root' })
export class WorkspaceService {
  constructor(private readonly api: ApiService) {}

  listApps(): Observable<WorkspaceApp[]> {
    return this.api.get<WorkspaceApp[]>('/workspace/apps');
  }

  listUsers(): Observable<WorkspaceUser[]> {
    return this.api.get<WorkspaceUser[]>('/workspace/users');
  }
}
