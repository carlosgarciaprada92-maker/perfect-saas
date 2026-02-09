import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { TranslateService } from '@ngx-translate/core';

export type AppLanguage = 'es' | 'en';

@Injectable({ providedIn: 'root' })
export class LanguageService {
  private readonly storageKey = 'core_lang';
  private readonly currentLangSubject = new BehaviorSubject<AppLanguage>('es');
  readonly currentLang$ = this.currentLangSubject.asObservable();

  constructor(private readonly translate: TranslateService) {
    this.translate.onLangChange.subscribe((event) => {
      const lang = (event.lang === 'en' ? 'en' : 'es') as AppLanguage;
      this.currentLangSubject.next(lang);
    });
  }

  init(): void {
    const saved = localStorage.getItem(this.storageKey);
    const initial = saved === 'en' ? 'en' : 'es';
    this.translate.addLangs(['es', 'en']);
    this.translate.setDefaultLang('es');
    this.translate.use(initial).subscribe(() => {
      this.currentLangSubject.next(initial);
    });
    localStorage.setItem(this.storageKey, initial);
  }

  getCurrentLang(): AppLanguage {
    return this.currentLangSubject.value;
  }

  setLang(lang: AppLanguage): void {
    if (lang === this.currentLangSubject.value) {
      return;
    }
    this.translate.use(lang).subscribe(() => {
      this.currentLangSubject.next(lang);
    });
    localStorage.setItem(this.storageKey, lang);
  }

  toggle(): void {
    this.setLang(this.currentLangSubject.value === 'es' ? 'en' : 'es');
  }
}
