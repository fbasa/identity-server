import { bootstrapApplication } from '@angular/platform-browser';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { importProvidersFrom } from '@angular/core';
import { provideRouter } from '@angular/router';
import { OAuthModule } from 'angular-oauth2-oidc';
import { AppComponent } from './app/app.component';
import { routes } from './app/app.routes';
import { AuthService } from './app/auth/auth.service';
import { environment } from './environments/environment';


async function initAuth(auth: AuthService) {
  await auth.initAuth();
}


bootstrapApplication(AppComponent, {
  providers: [
    provideRouter(routes),
    provideHttpClient(withInterceptorsFromDi()),
    importProvidersFrom(
      OAuthModule.forRoot({
        resourceServer: {
          allowedUrls: [environment.apis.payments, environment.apis.accounting],
          sendAccessToken: true // auto-attaches Authorization: Bearer <token>
        }
      })
    ),
    // run AuthService.initAuth() before the app renders
    {
      provide: 'APP_INIT',
      multi: true,
      useFactory: (auth: AuthService) => () => initAuth(auth),
      deps: [AuthService]
    }
  ]
}).catch(err => console.error(err));