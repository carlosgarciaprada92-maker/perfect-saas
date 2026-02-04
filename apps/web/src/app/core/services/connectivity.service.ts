import { Injectable } from '@angular/core';
import { BehaviorSubject, fromEvent, merge, of } from 'rxjs';
import { mapTo } from 'rxjs/operators';

@Injectable({ providedIn: 'root' })
export class ConnectivityService {
  private readonly onlineSubject = new BehaviorSubject<boolean>(
    typeof navigator !== 'undefined' ? navigator.onLine : true
  );

  readonly online$ = this.onlineSubject.asObservable();

  constructor() {
    if (typeof window === 'undefined') {
      return;
    }
    merge(
      fromEvent(window, 'online').pipe(mapTo(true)),
      fromEvent(window, 'offline').pipe(mapTo(false)),
      of(navigator.onLine)
    ).subscribe((status) => this.onlineSubject.next(status));
  }
}
