import { Component, OnInit, inject } from '@angular/core';
import { NgIf, NgFor, CommonModule } from '@angular/common';
import { RouterLink, RouterOutlet } from '@angular/router';
import { AuthService } from './auth/auth.service';
import { LoginStatusComponent } from './components/login-status.component';
import { PaymentsApiService } from './services/payments-api.service';
import { AccountingApiService } from './services/accounting-api.service';


@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, LoginStatusComponent],
  templateUrl: './app.component.html',
})
export class AppComponent implements OnInit {
  payments: any[] | null = null;
  accounting: any[] | null = null;


  private readonly paymentsApi = inject(PaymentsApiService);
  private readonly accountingApi = inject(AccountingApiService);
  public readonly auth = inject(AuthService);


  async ngOnInit() {
    // no-op; AuthService init runs at bootstrap
    this.loadPayments();
    this.loadAccounting();
  }


  loadPayments() {
    this.paymentsApi.getAll().subscribe(r => this.payments = r);
  }
  loadAccounting() {
    this.accountingApi.getAll().subscribe(r => this.accounting = r as any[]);
  }
}