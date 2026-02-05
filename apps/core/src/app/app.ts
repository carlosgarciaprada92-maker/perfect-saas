import { Component } from '@angular/core';
import { AsyncPipe, NgIf } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { Observable } from 'rxjs';
import { ConnectivityService } from './core/services/connectivity.service';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { LanguageService } from './core/services/language.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, NgIf, AsyncPipe, ToastModule, ConfirmDialogModule, TranslateModule],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  readonly online$: Observable<boolean>;

  constructor(
    private readonly connectivity: ConnectivityService,
    private readonly language: LanguageService
  ) {
    this.online$ = this.connectivity.online$;
  }
}
