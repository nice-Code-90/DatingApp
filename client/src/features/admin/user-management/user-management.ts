import { Component, inject, signal } from '@angular/core';
import { AdminService } from '../../../core/services/admin-service';
import { User } from '../../../types/user';

@Component({
  selector: 'app-user-management',
  imports: [],
  templateUrl: './user-management.html',
  styleUrl: './user-management.css',
})
export class UserManagement {
  private adminServie = inject(AdminService);
  protected users = signal<User[]>([]);

  ngOnInit() {
    this.getUserWithRoles();
  }

  getUserWithRoles() {
    this.adminServie.getUserWithRoles().subscribe({
      next: (users) => {
        this.users.set(users);
      },
    });
  }
}
