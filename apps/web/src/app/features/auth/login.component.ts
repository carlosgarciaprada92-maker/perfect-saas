import { Component } from '@angular/core';
import { FormsModule, ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { NgIf } from '@angular/common';
import { Router } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { DividerModule } from 'primeng/divider';
import { TagModule } from 'primeng/tag';
import { CheckboxModule } from 'primeng/checkbox';
import { MessageService } from 'primeng/api';
import { AuthService } from '../../core/services/auth.service';
import { Usuario } from '../../core/models/usuario.model';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    FormsModule,
    ReactiveFormsModule,
    NgIf,
    TranslateModule,
    InputTextModule,
    SelectModule,
    ButtonModule,
    CardModule,
    DividerModule,
    TagModule,
    CheckboxModule
  ],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  readonly demoAccounts = [
    { label: 'Admin', value: 'admin@perfect.demo', roleLabel: 'Admin' },
    { label: 'Ventas', value: 'ventas@perfect.demo', roleLabel: 'Ventas' },
    { label: 'Bodega', value: 'bodega@perfect.demo', roleLabel: 'Bodega' }
  ];

  readonly form;
  selectedAccount = this.demoAccounts[0].value;
  selectedRoleLabel = this.demoAccounts[0].roleLabel;

  constructor(
    private readonly fb: FormBuilder,
    private readonly auth: AuthService,
    private readonly router: Router,
    private readonly messages: MessageService
  ) {
    this.form = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      remember: [true]
    });

    this.form.controls.email.valueChanges.subscribe((value) => {
      const account = this.demoAccounts.find((item) => item.value === value);
      this.selectedRoleLabel = account?.roleLabel ?? '';
      if (account) {
        this.selectedAccount = account.value;
      }
    });
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const { email } = this.form.value;
    const user = this.auth.loginWithEmail(email!);
    if (!user) {
      this.messages.add({
        severity: 'error',
        summary: 'Usuario no encontrado',
        detail: 'Usuario no encontrado (demo)'
      });
      return;
    }
    this.router.navigate(['/app/dashboard']);
  }

  selectAccount(email: string): void {
    const account = this.demoAccounts.find((item) => item.value === email);
    this.selectedAccount = email;
    this.selectedRoleLabel = account?.roleLabel ?? '';
    this.form.patchValue({ email });
  }

  quickLogin(email: string): void {
    this.selectAccount(email);
    this.submit();
  }
}
