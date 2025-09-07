import { AuthConfig, NullValidationHandler } from 'angular-oauth2-oidc';
import { environment } from '../../environments/environment';


export const authConfig: AuthConfig = {
    issuer: environment.auth.issuer,
    clientId: environment.auth.clientId,
    redirectUri: environment.auth.redirectUri,
    postLogoutRedirectUri: environment.auth.postLogoutRedirectUri,
    responseType: 'code', // Authorization Code + PKCE
    scope: environment.auth.scopes, // 'openid profile payments.read accounting.read'
    showDebugInformation: !environment.production,
    requireHttps: true,
    useSilentRefresh: false, // we will use refresh tokens instead
    sessionChecksEnabled: false,
    // storage: sessionStorage is default for this lib; can set explicitly:
    // customHashFragment: window.location.hash, // not needed for code flow
    // note: library handles PKCE internally
};