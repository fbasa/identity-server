import { Component, inject } from '@angular/core';
import { NgIf, AsyncPipe, CommonModule } from '@angular/common';
import { AuthService } from '../auth/auth.service';


@Component({
    selector: 'app-login-status',
    standalone: true,
    imports: [CommonModule],
    template: `
        <div class="flex items-center gap-3 p-3 border-b">
        <div *ngIf="auth.isLoggedIn(); else loggedOut">
        ✅ Logged in
        <button (click)="auth.logout()">Logout</button>
        </div>
        <ng-template #loggedOut>
        <button (click)="auth.login()">Login</button>
        </ng-template>
        </div>
    `
})
export class LoginStatusComponent { constructor(public auth: AuthService) {} }