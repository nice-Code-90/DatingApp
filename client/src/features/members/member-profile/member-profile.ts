import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Member } from '../../../types/member';

@Component({
  selector: 'app-member-profile',
  imports: [],
  templateUrl: './member-profile.html',
  styleUrl: './member-profile.css',
})
export class MemberProfile {
  private route = inject(ActivatedRoute);
  protected member = signal<Member | undefined>(undefined);

  ngOnInit() {
    this.route.parent?.data.subscribe((data) => {
      this.member.set(data['member']);
    });
  }
}
