import { Directive, inject, Input, TemplateRef, ViewContainerRef } from '@angular/core';
import { AccountService } from '../../core/services/account-service';

@Directive({
  selector: '[appHasRole]',
})
export class HasRole {
  @Input() appHasRole: string[] = [];
  private accountService = inject(AccountService);
  private viewContainerRef = inject(ViewContainerRef);
  private templateRef = inject(TemplateRef);

  ngOnInit(): void {
    if (this.accountService.currentUser()?.roles.some((r) => this.appHasRole.includes(r))) {
      this.viewContainerRef.createEmbeddedView(this.templateRef);
    } else {
      this.viewContainerRef.clear();
    }
  }
}
