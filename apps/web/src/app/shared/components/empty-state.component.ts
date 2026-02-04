import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-empty-state',
  standalone: true,
  template: `
    <div class="empty-state card-surface">
      <i [class]="icon"></i>
      <h3>{{ title }}</h3>
      <p class="app-muted">{{ subtitle }}</p>
      <ng-content></ng-content>
    </div>
  `
})
export class EmptyStateComponent {
  @Input() icon = 'pi pi-inbox';
  @Input({ required: true }) title!: string;
  @Input() subtitle = '';
}
