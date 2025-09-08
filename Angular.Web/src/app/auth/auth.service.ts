import { Injectable, inject } from '@angular/core';
import { OAuthService, OAuthEvent, OAuthSuccessEvent, OAuthErrorEvent } from 'angular-oauth2-oidc';
import { authConfig } from './auth.config';
import { Router } from '@angular/router';
import { filter } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly oauth = inject(OAuthService);
  private readonly router = inject(Router);

  async initAuth(): Promise<void> {
    this.oauth.configure(authConfig);

    // 1) Load discovery + try to complete the code flow on /auth/callback
    await this.oauth.loadDiscoveryDocument();
    await this.oauth.tryLoginCodeFlow({
      onTokenReceived: async (_ctx) => {
        // 2) Clean up the ugly callback URL and navigate somewhere pleasant
        window.history.replaceState({}, document.title, window.location.origin + '/');
        await this.oauth.loadUserProfile(); // optional
      }
    });

    // (Optional) Debug: log OAuth events
    this.oauth.events
      .pipe(filter((e: OAuthEvent) => e instanceof OAuthSuccessEvent || e instanceof OAuthErrorEvent))
      .subscribe(e => console.log('[OAuthEvent]', e));
  }

  login(): void {
    // Ensure discovery is loaded, then redirect to Duende
    this.oauth.loadDiscoveryDocumentAndLogin();
  }

  logout(): void {
    this.oauth.logOut(); // redirects to end session
  }

  isLoggedIn(): boolean {
    return this.oauth.hasValidAccessToken();
  }

  get accessToken(): string | null {
    return this.oauth.getAccessToken() || null;
  }
}
