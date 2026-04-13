import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { Router } from '@angular/router';
import { AuthResult, LoginRequest, RegisterRequest } from '../models/models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly BASE = 'http://localhost:5000/api/auth';
  private _user$ = new BehaviorSubject<{ name: string; role: string; customerId: string, email: string } | null>(null);
  user$ = this._user$.asObservable();

  constructor(private http: HttpClient, private router: Router) {
    this.loadFromStorage();
  }

  private loadFromStorage() {
    const token = localStorage.getItem('as_token');
    const name  = localStorage.getItem('as_name');
    const role  = localStorage.getItem('as_role');
    const id    = localStorage.getItem('as_id');
    const email = localStorage.getItem('as_email');
    if (token && name && role && id && email) {
      this._user$.next({ name, role, customerId: id, email });
    } else if (token || name || role || id || email) {
      // Clean up broken partial session
      this.clearStorage();
    }
  }

  private clearStorage() {
    localStorage.removeItem('as_token');
    localStorage.removeItem('as_name');
    localStorage.removeItem('as_role');
    localStorage.removeItem('as_id');
    localStorage.removeItem('as_email');
    this._user$.next(null);
  }

  register(req: RegisterRequest): Observable<AuthResult> {
    return this.http.post<AuthResult>(`${this.BASE}/register`, req).pipe(
      tap(res => { if (res.success && res.token) this.saveSession(res); })
    );
  }



  registerAgent(req: RegisterRequest): Observable<AuthResult> {
    return this.http.post<AuthResult>(`${this.BASE}/register-agent`, req).pipe(
      tap(res => { if (res.success && res.token) this.saveSession(res); })
    );
  }

  login(req: LoginRequest): Observable<AuthResult> {
    return this.http.post<AuthResult>(`${this.BASE}/login`, req).pipe(
      tap(res => { if (res.success && res.token) this.saveSession(res); })
    );
  }

  private saveSession(res: AuthResult) {
    localStorage.setItem('as_token', res.token!);
    localStorage.setItem('as_name', res.name ?? '');
    localStorage.setItem('as_role', res.role ?? 'Customer');
    localStorage.setItem('as_id', res.customerId ?? '');
    localStorage.setItem('as_email', res.email ?? '');
    this._user$.next({ name: res.name!, role: res.role!, customerId: res.customerId!, email: res.email! });
  }

  logout() {
    this.clearStorage();
    this.router.navigate(['/']);
  }

  get token(): string | null { return localStorage.getItem('as_token'); }
  get isLoggedIn(): boolean  { return !!this.token; }
  get currentUser()          { return this._user$.value; }
  get isArtist(): boolean    { return this._user$.value?.role === 'Artist'; }
  get isAdmin(): boolean     { return this._user$.value?.role === 'Admin'; }
  get canManageArtworks(): boolean { return this.isArtist || this.isAdmin; }
}
