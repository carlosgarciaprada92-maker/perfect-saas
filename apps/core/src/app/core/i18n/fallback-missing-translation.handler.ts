import { Injectable } from '@angular/core';
import { MissingTranslationHandler, MissingTranslationHandlerParams } from '@ngx-translate/core';

@Injectable()
export class FallbackMissingTranslationHandler implements MissingTranslationHandler {
  handle(params: MissingTranslationHandlerParams): string {
    const store = params.translateService as unknown as {
      store?: { translations?: Record<string, unknown> };
    };
    const fallback = store.store?.translations?.['es'] as Record<string, unknown> | undefined;
    const resolved = fallback ? resolveTranslation(fallback, params.key) : null;
    return typeof resolved === 'string' ? resolved : params.key;
  }
}

function resolveTranslation(source: Record<string, unknown>, path: string): unknown {
  return path.split('.').reduce<unknown>((acc, key) => {
    if (acc && typeof acc === 'object' && key in acc) {
      return (acc as Record<string, unknown>)[key];
    }
    return null;
  }, source);
}
