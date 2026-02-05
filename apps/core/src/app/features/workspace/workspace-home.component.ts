import { Component, OnInit } from '@angular/core';
import { NgFor, NgIf } from '@angular/common';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { MessageService } from 'primeng/api';
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

  constructor(
    private readonly workspace: WorkspaceService,
    private readonly messages: MessageService,
    private readonly translate: TranslateService
  ) {}

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

  async openApp(app: WorkspaceApp): Promise<void> {
    const baseUrl = (app.baseUrl ?? '').trim();
    if (!baseUrl) {
      this.messages.add({
        severity: 'warn',
        summary: this.translate.instant('common.notice'),
        detail: this.translate.instant('workspace.urlMissing')
      });
      return;
    }

    try {
      new URL(baseUrl);
    } catch {
      this.messages.add({
        severity: 'error',
        summary: this.translate.instant('common.error'),
        detail: this.translate.instant('workspace.urlInvalid')
      });
      return;
    }

    const reachable = await this.checkUrl(baseUrl);
    if (!reachable) {
      this.messages.add({
        severity: 'error',
        summary: this.translate.instant('common.error'),
        detail: this.translate.instant('workspace.urlUnavailable')
      });
      return;
    }

    window.open(baseUrl, '_blank', 'noopener');
  }

  private async checkUrl(url: string): Promise<boolean> {
    const controller = new AbortController();
    const timeout = window.setTimeout(() => controller.abort(), 3500);
    try {
      await fetch(url, { method: 'HEAD', mode: 'no-cors', signal: controller.signal });
      return true;
    } catch {
      return false;
    } finally {
      clearTimeout(timeout);
    }
  }
}
