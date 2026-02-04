import { Component } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { AsyncPipe } from '@angular/common';
import { TableModule } from 'primeng/table';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { SelectModule } from 'primeng/select';
import { ConfirmationService, MessageService } from 'primeng/api';
import { CustomersService } from '../../core/services/customers.service';
import { Cliente } from '../../core/models/cliente.model';
import { PageHeaderComponent } from '../../shared/components/page-header.component';
import { TranslateModule } from '@ngx-translate/core';
import { RolesService } from '../../core/services/roles.service';
import { AuthService } from '../../core/services/auth.service';
import { PermissionKey } from '../../core/models/app-module.model';

@Component({
  selector: 'app-clientes',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    AsyncPipe,
    TableModule,
    DialogModule,
    ButtonModule,
    InputTextModule,
    InputNumberModule,
    SelectModule,
    PageHeaderComponent,
    TranslateModule
  ],
  templateUrl: './clientes.component.html'
})
export class ClientesComponent {
  readonly clientes$;
  visible = false;
  isEdit = false;
  currentId: string | null = null;

  readonly form;

  readonly plazos = [
    { label: '8 dias', value: 8 },
    { label: '15 dias', value: 15 },
    { label: '30 dias', value: 30 },
    { label: 'Otro', value: 45 }
  ];

  constructor(
    private readonly fb: FormBuilder,
    private readonly customers: CustomersService,
    private readonly confirm: ConfirmationService,
    private readonly messages: MessageService,
    private readonly roles: RolesService,
    private readonly auth: AuthService
  ) {
    this.clientes$ = this.customers.clientes$;
    this.form = this.fb.group({
      nombre: ['', Validators.required],
      identificacion: [''],
      telefono: [''],
      email: [''],
      plazoCreditoDias: [15, Validators.required],
      activo: [true]
    });
  }

  openNew(): void {
    if (!this.can('clientes:create')) {
      return;
    }
    this.isEdit = false;
    this.currentId = null;
    this.form.reset({ activo: true, plazoCreditoDias: 15 });
    this.visible = true;
  }

  openEdit(cliente: Cliente): void {
    if (!this.can('clientes:edit')) {
      return;
    }
    this.isEdit = true;
    this.currentId = cliente.id;
    this.form.patchValue(cliente);
    this.visible = true;
  }

  save(): void {
    if (this.isEdit && !this.can('clientes:edit')) {
      return;
    }
    if (!this.isEdit && !this.can('clientes:create')) {
      return;
    }
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const value = this.form.getRawValue() as Partial<Cliente>;
    if (this.isEdit && this.currentId) {
      this.customers.update(this.currentId, value);
      this.messages.add({ severity: 'info', summary: 'Actualizado', detail: 'Cliente actualizado' });
    } else {
      this.customers.create(value as any);
      this.messages.add({ severity: 'info', summary: 'Creado', detail: 'Cliente creado' });
    }
    this.visible = false;
  }

  delete(cliente: Cliente): void {
    if (!this.can('clientes:delete')) {
      return;
    }
    this.confirm.confirm({
      message: 'Eliminar cliente?',
      header: 'Confirmacion',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.customers.remove(cliente.id);
        this.messages.add({ severity: 'info', summary: 'Eliminado', detail: 'Cliente eliminado' });
      }
    });
  }

  can(permission: PermissionKey): boolean {
    const user = this.auth.snapshot;
    if (!user) {
      return false;
    }
    return this.roles.hasPermission(user, permission);
  }
}
