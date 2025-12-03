import { Component, inject, signal } from '@angular/core';
import { AccountService } from '../../core/services/account-service';
import { UserManagement } from './user-management/user-management';
import { PhotoManagement } from './photo-management/photo-management';
import { AdminService } from '../../core/services/admin-service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-admin',
  imports: [UserManagement, PhotoManagement, CommonModule],
  templateUrl: './admin.html',
  styleUrl: './admin.css',
})
export class Admin {
  protected accountService = inject(AccountService);
  private adminService = inject(AdminService);
  isSeeding = signal(false);
  isReindexing = signal(false);

  activeTab = 'photos';

  tabs = [
    { label: 'Photo moderation', value: 'photos' },
    { label: 'User managament', value: 'roles' },
    { label: 'Actions', value: 'actions' },
  ];

  setTab(tab: string) {
    this.activeTab = tab;
  }

  seedUsers() {
    this.isSeeding.set(true);
    this.adminService.seedUsers().subscribe({
      next: () => {},
      error: (err) => {
        alert('An error occurred: ' + err.error);
        this.isSeeding.set(false);
      },
      complete: () => {
        setTimeout(() => this.isSeeding.set(false), 5000);
      }
    });
  }

  reindexQdrant() {
    this.isReindexing.set(true);
    this.adminService.reindexQdrant().subscribe({
      next: () => {},
      error: (err) => {
        alert('An error occurred: ' + err.error);
        this.isReindexing.set(false);
      },
      complete: () => {
        // Give feedback to the user that the process has started
        setTimeout(() => this.isReindexing.set(false), 5000);
      }
    });
  }
}
