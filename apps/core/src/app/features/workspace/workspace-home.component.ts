import { Component, OnInit } from '@angular/core';
import { NgFor, NgIf } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { WorkspaceService } from '../../core/services/workspace.service';
import { WorkspaceApp } from '../../core/models/workspace.model';

@Component({
  selector: 'app-workspace-home',
  standalone: true,
  imports: [NgFor, NgIf, TranslateModule, CardModule, ButtonModule, TagModule],
  templateUrl: './workspace-home.component.html',
  styleUrl: './workspace-home.component.scss'
})
export class WorkspaceHomeComponent implements OnInit {
  apps: WorkspaceApp[] = [];
  loading = true;

  constructor(private readonly workspace: WorkspaceService) {}

  ngOnInit(): void {
    this.workspace.listApps().subscribe({
      next: (apps) => {
        this.apps = apps;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      }
    });
  }

  openApp(app: WorkspaceApp): void {
    if (!app.baseUrl) {
      return;
    }
    window.open(app.baseUrl, '_blank', 'noopener');
  }
}
