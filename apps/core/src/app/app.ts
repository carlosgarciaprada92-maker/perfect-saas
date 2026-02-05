import { Component } from '@angular/core';
import { AsyncPipe, NgIf } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Observable } from 'rxjs';
import { ConnectivityService } from './core/services/connectivity.service';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, NgIf, AsyncPipe, ToastModule, ConfirmDialogModule, TranslateModule],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  readonly online$: Observable<boolean>;

  constructor(
    private readonly translate: TranslateService,
    private readonly connectivity: ConnectivityService
  ) {
    const savedLang = localStorage.getItem('core_lang') ?? 'es';
    const lang = savedLang === 'en' ? 'en' : 'es';
    this.translate.addLangs(['es', 'en']);
    this.translate.setDefaultLang('es');
    this.translate.use(lang);
    this.online$ = this.connectivity.online$;
  }
}
