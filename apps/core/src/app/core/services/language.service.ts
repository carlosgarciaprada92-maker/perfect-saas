import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { TranslateService } from '@ngx-translate/core';

export type AppLanguage = 'es' | 'en';

@Injectable({ providedIn: 'root' })
export class LanguageService {
  private readonly storageKey = 'core_lang';
  private readonly currentLangSubject = new BehaviorSubject<AppLanguage>('es');
  readonly currentLang$ = this.currentLangSubject.asObservable();

  constructor(private readonly translate: TranslateService) {}

  init(): void {
    const saved = localStorage.getItem(this.storageKey);
    const initial = saved === 'en' ? 'en' : 'es';
    this.translate.addLangs(['es', 'en']);
    this.translate.setDefaultLang('es');
    this.translate.use(initial);
    this.currentLangSubject.next(initial);
  }

  getCurrentLang(): AppLanguage {
    return this.currentLangSubject.value;
  }

  setLang(lang: AppLanguage): void {
    if (lang === this.currentLangSubject.value) {
      return;
    }
    this.translate.use(lang);
    localStorage.setItem(this.storageKey, lang);
    this.currentLangSubject.next(lang);
  }

  toggle(): void {
    this.setLang(this.currentLangSubject.value === 'es' ? 'en' : 'es');
  }
}
