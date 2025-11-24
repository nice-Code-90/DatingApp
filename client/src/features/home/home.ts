import { Component, inject, signal } from '@angular/core';
import { Register } from '../account/register/register';
import { AccountService } from '../../core/services/account-service';

@Component({
  selector: 'app-home',
  imports: [Register],
  templateUrl: './home.html',
  styleUrl: './home.css',
})
export class Home {
  protected registerMode = signal(false);
  protected learnMoreMode = signal(false);
  protected accountService = inject(AccountService);

  showRegister(value: boolean) {
    this.registerMode.set(value);

    if (value) this.learnMoreMode.set(false);
  }

  learnMore() {
    this.learnMoreMode.set(true);

    setTimeout(() => {
      const element = document.getElementById('learn-more-section');
      if (element) {
        element.scrollIntoView({ behavior: 'smooth', block: 'start' });
      }
    }, 100);
  }
}
