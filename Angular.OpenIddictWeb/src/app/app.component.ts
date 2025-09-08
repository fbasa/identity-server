import { Component, OnInit, inject } from '@angular/core';
import { CommonModule, NgIf } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AuthService } from './auth/auth.service';
import { PaymentsApiService } from './services/payments-api.service';
import { AccountingApiService } from './services/accounting-api.service';


@Component({
  selector: 'app-root',
  standalone: true,
  imports: [NgIf, RouterLink, CommonModule],
  templateUrl: './app.component.html',
})
export class AppComponent implements OnInit {
  payments: any[] | null = null;
  accounting: any[] | null = null;
  private readonly paymentsApi = inject(PaymentsApiService);
  private readonly accountingApi = inject(AccountingApiService);
  public readonly auth = inject(AuthService);
  async ngOnInit() { 
    this.loadPayments(); 
    this.loadAccounting();
  }
  loadPayments() { this.paymentsApi.getAll().subscribe(r => this.payments = r); }
  loadAccounting() { this.accountingApi.post({}).subscribe(r => this.accounting = r as any[]); }
}