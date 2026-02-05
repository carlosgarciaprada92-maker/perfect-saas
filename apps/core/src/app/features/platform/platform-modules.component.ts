import { Component, OnInit, ChangeDetectorRef, NgZone } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators, FormGroup } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { TableModule } from 'primeng/table';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { MessageService } from 'primeng/api';
import { catchError, finalize, of } from 'rxjs';
import { PlatformService } from '../../core/services/platform.service';
import { ModuleCatalog, ModuleCatalogRequest, ModuleStatus } from '../../core/models/platform.model';

@Component({
  selector: 'app-platform-modules',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    TranslateModule,
    TableModule,
    DialogModule,
    InputTextModule,
    SelectModule,
    ButtonModule,
    TagModule
  ],
  templateUrl: './platform-modules.component.html',
  styleUrl: './platform-modules.component.scss'
})
export class PlatformModulesComponent implements OnInit {
  modules: ModuleCatalog[] = [];
  loading = true;
  dialogOpen = false;
  editing: ModuleCatalog | null = null;

  readonly statusOptions = [
    { label: 'Active', value: 'Active' as ModuleStatus },
    { label: 'Beta', value: 'Beta' as ModuleStatus },
    { label: 'Disabled', value: 'Disabled' as ModuleStatus }
  ];

  readonly form: FormGroup;

  constructor(
    private readonly fb: FormBuilder,
    private readonly platform: PlatformService,
    private readonly messages: MessageService,
    private readonly translate: TranslateService,
    private readonly cdr: ChangeDetectorRef,
    private readonly zone: NgZone
  ) {
    this.form = this.fb.group({
      name: ['', [Validators.required]],
      slug: ['', [Validators.required]],
      baseUrl: ['', [Validators.required]],
      status: ['Active' as ModuleStatus, [Validators.required]],
      icon: ['']
    });
  }

  ngOnInit(): void {
    this.loadModules();
  }

  loadModules(): void {
    this.loading = true;
    this.platform
      .listModules()
      .pipe(
        catchError(() => {
          this.zone.run(() => {
            this.modules = [];
            this.messages.add({
              severity: 'error',
              summary: this.translate.instant('common.error'),
              detail: this.translate.instant('platform.modules.loadError')
            });
          });
          return of([] as ModuleCatalog[]);
        }),
        finalize(() => {
          this.zone.run(() => {
            this.loading = false;
            this.cdr.markForCheck();
          });
        })
      )
      .subscribe((modules) => {
        this.zone.run(() => {
          this.modules = [...modules];
          this.cdr.markForCheck();
        });
      });
  }

  openCreate(): void {
    this.editing = null;
    this.form.reset({ status: 'Active' });
    this.dialogOpen = true;
  }

  openEdit(module: ModuleCatalog): void {
    this.editing = module;
    this.form.reset({
      name: module.name,
      slug: module.slug,
      baseUrl: module.baseUrl,
      status: module.status,
      icon: module.icon ?? ''
    });
    this.dialogOpen = true;
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const payload = this.form.getRawValue() as ModuleCatalogRequest;
    const request$ = this.editing
      ? this.platform.updateModule(this.editing.id, payload)
      : this.platform.createModule(payload);

    request$.subscribe(() => {
      this.dialogOpen = false;
      this.loadModules();
    });
  }

  remove(module: ModuleCatalog): void {
    this.platform.deleteModule(module.id).subscribe(() => this.loadModules());
  }
}
