import { Injectable, signal, computed, inject, PLATFORM_ID } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { isPlatformBrowser } from '@angular/common';
import { Observable, finalize, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthResponse, CurrentUser, LoginDto, RegisterDto, RefreshTokenDto } from '../models/auth.model';

const REFRESH_TOKEN_KEY = 'cardvault-refresh-token';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly isBrowser = isPlatformBrowser(inject(PLATFORM_ID));
  private readonly api = `${environment.apiUrl}/auth`;

  private readonly _accessToken = signal<string | null>(null);
  private readonly _currentUser = signal<CurrentUser | null>(null);
  private readonly _initializing = signal(true);

  readonly isLoggedIn = computed(() => this._accessToken() !== null);
  readonly currentUser = this._currentUser.asReadonly();
  readonly initializing = this._initializing.asReadonly();

  constructor() {
    if (this.isBrowser && this.getRefreshToken()) {
      this.refreshAccessToken().pipe(
        finalize(() => this._initializing.set(false))
      ).subscribe({
        error: () => this.clearSession()
      });
    } else {
      this._initializing.set(false);
    }
  }

  getAccessToken(): string | null {
    return this._accessToken();
  }

  getRefreshToken(): string | null {
    return this.isBrowser ? localStorage.getItem(REFRESH_TOKEN_KEY) : null;
  }

  hasRefreshToken(): boolean {
    return !!this.getRefreshToken();
  }

  login(dto: LoginDto): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.api}/login`, dto).pipe(
      tap(res => this.setSession(res))
    );
  }

  register(dto: RegisterDto): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.api}/register`, dto).pipe(
      tap(res => this.setSession(res))
    );
  }

  refreshAccessToken(): Observable<AuthResponse> {
    const refreshToken = this.getRefreshToken();
    return this.http.post<AuthResponse>(`${this.api}/refresh`, { refreshToken } as RefreshTokenDto).pipe(
      tap(res => this.setSession(res))
    );
  }

  logout(): void {
    const refreshToken = this.getRefreshToken();
    if (refreshToken) {
      this.http.post(`${this.api}/revoke`, { refreshToken }).subscribe();
    }
    this.clearSession();
  }

  private setSession(res: AuthResponse): void {
    this._accessToken.set(res.accessToken);
    this._currentUser.set({ userId: res.userId, username: res.username });
    if (this.isBrowser) {
      localStorage.setItem(REFRESH_TOKEN_KEY, res.refreshToken);
    }
  }

  private clearSession(): void {
    this._accessToken.set(null);
    this._currentUser.set(null);
    if (this.isBrowser) {
      localStorage.removeItem(REFRESH_TOKEN_KEY);
    }
  }
}
