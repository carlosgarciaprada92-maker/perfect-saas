import { Component } from '@angular/core';
import { NgIf, AsyncPipe } from '@angular/common';
import { FormBuilder, Validators, ReactiveFormsModule, FormGroup } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { DividerModule } from 'primeng/divider';
import { MessageService } from 'primeng/api';
import { map } from 'rxjs/operators';
import { Observable } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';
import { LanguageService } from '../../core/services/language.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    NgIf,
    AsyncPipe,
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
  readonly currentLangLabel$: Observable<string>;

  loading = false;

  constructor(
    private readonly fb: FormBuilder,
    private readonly auth: AuthService,
    private readonly router: Router,
    private readonly route: ActivatedRoute,
    private readonly messages: MessageService,
    public readonly translate: TranslateService,
    public readonly language: LanguageService
  ) {
    this.form = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required]],
      tenantSlug: ['', [Validators.required]]
    });

    this.route.data.subscribe((data) => {
      const mode = data['mode'] === 'portal' ? 'portal' : 'platform';
      this.applyMode(mode);
    });

    this.currentLangLabel$ = this.language.currentLang$.pipe(map((lang) => lang.toUpperCase()));
  }

  submit(): void {
    if (this.form.invalid || this.loading) {
      this.form.markAllAsTouched();
      return;
    }
    this.loading = true;
    const { email, password } = this.form.getRawValue();
    const rawTenant = this.form.get('tenantSlug')?.value ?? '';
    const tenantSlug = this.mode === 'platform' ? 'platform' : rawTenant;

    if (this.mode === 'platform' && rawTenant && rawTenant !== 'platform') {
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
    this.language.toggle();
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
      if (tenantControl.value === 'platform') {
        tenantControl.setValue('');
      }
    }
  }
}
