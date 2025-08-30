import { Component, HostListener, inject, signal, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { EditableMember, Member } from '../../../types/member';
import { DatePipe } from '@angular/common';
import { MemberService } from '../../../core/services/member-service';
import { FormsModule, NgForm } from '@angular/forms';
import { ToastService } from '../../../core/services/toast-service';

@Component({
  selector: 'app-member-profile',
  imports: [DatePipe, FormsModule],
  templateUrl: './member-profile.html',
  styleUrl: './member-profile.css',
})
export class MemberProfile {
  @ViewChild('editForm') editForm?: NgForm;
  @HostListener('window:beforeunload', ['$event']) notify($event: BeforeUnloadEvent) {
    if (this.editForm?.dirty) {
      $event.preventDefault();
    }
  }
  protected memberService = inject(MemberService);
  private toast = inject(ToastService);
  private route = inject(ActivatedRoute);
  protected member = signal<Member | undefined>(undefined);

  protected editableMember: EditableMember = {
    displayName: '',
    description: '',
    city: '',
    country: '',
  };

  ngOnInit() {
    this.route.parent?.data.subscribe((data) => {
      this.member.set(data['member']);
    });

    this.editableMember = {
      displayName: this.member()?.displayName || '',
      description: this.member()?.description || '',
      city: this.member()?.city || '',
      country: this.member()?.country || '',
    };
  }

  updateProfile() {
    if (!this.member()) return;
    const updatedMember = { ...this.member(), ...this.editableMember };
    this.memberService.updateMember(updatedMember).subscribe({
      next: () => {
        this.toast.success('Profile updated successfully');
        this.memberService.editMode.set(false);
        this.editForm?.reset(updatedMember);
      },
    });
    this.toast.success('Profile updated successfully');
    this.memberService.editMode.set(false);
  }
  ngOnDestroy() {
    if (this.memberService.editMode()) {
      this.memberService.editMode.set(false);
    }
  }
}
