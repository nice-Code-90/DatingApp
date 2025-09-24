import { Component, ElementRef, inject, signal, ViewChild } from '@angular/core';
import { AdminService } from '../../../core/services/admin-service';
import { User } from '../../../types/user';

@Component({
  selector: 'app-user-management',
  imports: [],
  templateUrl: './user-management.html',
  styleUrl: './user-management.css',
})
export class UserManagement {
  @ViewChild('rolesModal') rolesModal!: ElementRef<HTMLDialogElement>;
  private adminServie = inject(AdminService);
  protected users = signal<User[]>([]);

  protected availableRoles = ['Member', 'Moderator', 'Admin'];

  protected selectedUser: User | null = null;

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

  openRolesModal(user: User) {
    this.selectedUser = user;
    this.rolesModal.nativeElement.showModal();
  }
}
