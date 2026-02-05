import { Component } from '@angular/core';
import { NgIf } from '@angular/common';
import { FormBuilder, Validators, ReactiveFormsModule, FormGroup } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { DividerModule } from 'primeng/divider';
import { MessageService } from 'primeng/api';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    NgIf,
    ReactiveFormsModule,
    TranslateModule,
    InputTextModule,
    PasswordModule,
    ButtonModule,
    CardModule,
    DividerModule
  ],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  readonly form: FormGroup;
  mode: 'platform' | 'portal' = 'platform';

  loading = false;

  constructor(
    private readonly fb: FormBuilder,
    private readonly auth: AuthService,
    private readonly router: Router,
    private readonly route: ActivatedRoute,
    private readonly messages: MessageService,
    public readonly translate: TranslateService
  ) {
    this.form = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required]],
      tenantSlug: ['demo', [Validators.required]]
    });

    this.route.data.subscribe((data) => {
      const mode = data['mode'] === 'portal' ? 'portal' : 'platform';
      this.applyMode(mode);
    });
  }

  submit(): void {
    if (this.form.invalid || this.loading) {
      this.form.markAllAsTouched();
      return;
    }
    this.loading = true;
    const { email, password } = this.form.getRawValue();
    const tenantSlug = this.mode === 'platform' ? 'platform' : (this.form.get('tenantSlug')?.value ?? '');

    if (this.mode === 'platform' && tenantSlug !== 'platform') {
      this.form.get('tenantSlug')?.setValue('platform');
      this.messages.add({
        severity: 'info',
        summary: this.translate.instant('common.notice'),
        detail: this.translate.instant('login.platformOnly')
      });
    }

    this.auth.login({ email: email!, password: password!, tenantSlug: tenantSlug! }).subscribe({
      next: (response) => {
        const roles = response.user.roles;
        const route = roles.includes('PlatformAdmin') ? '/platform/tenants' : '/workspace';
        this.router.navigate([route]);
        this.loading = false;
      },
      error: (err) => {
        const message =
          err?.error?.errors?.[0]?.message ||
          err?.error?.message ||
          this.translate.instant('login.error');
        this.messages.add({
          severity: 'error',
          summary: this.translate.instant('common.error'),
          detail: message
        });
        this.loading = false;
      }
    });
  }

  switchLanguage(): void {
    const next = this.translate.currentLang === 'es' ? 'en' : 'es';
    this.translate.use(next);
    localStorage.setItem('core_lang', next);
  }

  fillDemo(type: 'platform' | 'tenant'): void {
    if (type === 'platform') {
      this.form.patchValue({
        email: 'platform.admin@perfect.demo',
        password: 'Platform123!',
        tenantSlug: 'platform'
      });
      this.submit();
      return;
    }
    this.form.patchValue({
      email: 'admin@perfect.demo',
      password: 'Admin123!',
      tenantSlug: 'demo'
    });
    this.submit();
  }

  private applyMode(mode: 'platform' | 'portal'): void {
    this.mode = mode;
    const tenantControl = this.form.get('tenantSlug');
    if (!tenantControl) {
      return;
    }

    if (mode === 'platform') {
      tenantControl.setValue('platform');
      tenantControl.disable({ emitEvent: false });
    } else {
      if (tenantControl.disabled) {
        tenantControl.enable({ emitEvent: false });
      }
      if (!tenantControl.value) {
        tenantControl.setValue('demo');
      }
    }
  }
}
