import { Component } from '@angular/core';
import { FormBuilder, Validators, ReactiveFormsModule, FormGroup } from '@angular/forms';
import { Router } from '@angular/router';
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

  loading = false;

  constructor(
    private readonly fb: FormBuilder,
    private readonly auth: AuthService,
    private readonly router: Router,
    private readonly messages: MessageService,
    public readonly translate: TranslateService
  ) {
    this.form = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required]],
      tenantSlug: ['demo', [Validators.required]]
    });
  }

  submit(): void {
    if (this.form.invalid || this.loading) {
      this.form.markAllAsTouched();
      return;
    }
    this.loading = true;
    const { email, password, tenantSlug } = this.form.getRawValue();

    this.auth.login({ email: email!, password: password!, tenantSlug: tenantSlug! }).subscribe({
      next: (response) => {
        const roles = response.user.roles;
        const route = roles.includes('PlatformAdmin') ? '/platform/tenants' : '/workspace';
        this.router.navigate([route]);
        this.loading = false;
      },
      error: (err) => {
        const message = err?.error?.errors?.[0]?.message || err?.error?.message || 'Login failed';
        this.messages.add({ severity: 'error', summary: 'Error', detail: message });
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
      return;
    }
    this.form.patchValue({
      email: 'admin@perfect.demo',
      password: 'Admin123!',
      tenantSlug: 'demo'
    });
  }
}
