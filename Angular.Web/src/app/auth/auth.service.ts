import { Injectable, inject } from '@angular/core';
import { OAuthService, OAuthEvent, OAuthSuccessEvent, OAuthErrorEvent } from 'angular-oauth2-oidc';
import { authConfig } from './auth.config';
import { filter, firstValueFrom, map } from 'rxjs';


@Injectable({ providedIn: 'root' })
export class AuthService {
    private readonly oauth = inject(OAuthService);


    async initAuth(): Promise<void> {
        this.oauth.configure(authConfig);
        this.oauth.setupAutomaticSilentRefresh(); // uses refresh tokens when available


        // Try to parse tokens on app start (handles /auth/callback)
        await this.oauth.loadDiscoveryDocument();
        await this.oauth.tryLoginCodeFlow();


        // If not logged in, you can choose to trigger login here or keep app public.
        // if (!this.isLoggedIn()) this.login();


        // Optionally load user profile (requires 'openid profile' scopes)
        if (this.isLoggedIn()) {
            await this.oauth.loadUserProfile();
        }


        // Log auth events in dev
        if (!window || !('location' in window)) return;
        this.oauth.events
            .pipe(filter((e: OAuthEvent) => e instanceof OAuthSuccessEvent || e instanceof OAuthErrorEvent))
            .subscribe(e => console.log('[OAuthEvent]', e));
    }


    login(): void {
        console.log('Starting login flow');
        this.oauth.initLoginFlow();
    }


    async logout(): Promise<void> {
        // end-session at Duende + local cleanup
        await this.oauth.logOut();
    }


    isLoggedIn(): boolean {
        return this.oauth.hasValidAccessToken();
    }


    get accessToken(): string | null {
        return this.oauth.getAccessToken() || null;
    }


    get idToken(): string | null {
        return this.oauth.getIdToken() || null;
    }


    get claims(): any {
        return this.oauth.getIdentityClaims();
    }
}