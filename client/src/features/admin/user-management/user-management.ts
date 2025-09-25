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

  toggleRole(event: Event, role: string) {
    if (!this.selectedUser) return;
    const isChecked = (event.target as HTMLInputElement).checked;
    if (isChecked) {
      this.selectedUser.roles.push(role);
    } else {
      this.selectedUser.roles = this.selectedUser.roles.filter((r) => r !== role);
    }
  }

  updateRoles() {
    if (!this.selectedUser) return;
    this.adminServie.updateUserRoles(this.selectedUser.id, this.selectedUser.roles).subscribe({
      next: (updatedRoles) => {
        this.users.update((users) =>
          users.map((u) => {
            if (u.id === this.selectedUser?.id) u.roles = updatedRoles;
            return u;
          })
        );
        this.rolesModal.nativeElement.close();
      },
      error: (error) => console.log('Failed to update roles', error),
    });
  }
}
