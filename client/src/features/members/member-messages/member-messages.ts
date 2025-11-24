import {
  Component,
  effect,
  ElementRef,
  inject,
  model,
  OnDestroy,
  OnInit,
  signal,
  ViewChild,
} from '@angular/core';
import { MessageService } from '../../../core/services/message-service';
import { DatePipe } from '@angular/common';
import { TimeAgoPipe } from '../../../core/pipes/time-ago-pipe';
import { FormsModule } from '@angular/forms';
import { PresenceService } from '../../../core/services/presence-service';

import { ActivatedRoute } from '@angular/router';
import { AiHelperService } from '../../../core/services/ai-helper-service';

@Component({
  selector: 'app-member-messages',
  imports: [DatePipe, TimeAgoPipe, FormsModule],
  templateUrl: './member-messages.html',
  styleUrl: './member-messages.css',
})
export class MemberMessages implements OnInit, OnDestroy {
  @ViewChild('messageEndRef') messageEndRef!: ElementRef;
  protected messageService = inject(MessageService);
  protected presenceService = inject(PresenceService);
  private route = inject(ActivatedRoute);
  private aiHelperService = inject(AiHelperService);
  protected messageContent = model('');
  protected isGettingSuggestion = signal(false);

  constructor() {
    effect(() => {
      const currentMessages = this.messageService.messageThread();
      if (currentMessages.length > 0) {
        this.scrollToBottom();
      }
    });
  }

  ngOnInit(): void {
    this.route.parent?.paramMap.subscribe({
      next: (params) => {
        const otherUserId = params.get('id');
        if (!otherUserId) throw new Error('Cannot connect to hub');
        this.messageService.createHubConnection(otherUserId);
      },
    });
  }

  sendMessage() {
    const recipientId = this.route.parent?.snapshot.paramMap.get('id');
    if (!recipientId || !this.messageContent()) return;
    this.messageService.sendMessage(recipientId, this.messageContent())?.then(() => {
      this.messageContent.set('');
    });
  }

  getAiSuggestion() {
    const recipientId = this.route.parent?.snapshot.paramMap.get('id');
    if (!recipientId) return;

    this.isGettingSuggestion.set(true);
    this.aiHelperService.getSuggestion(recipientId).subscribe({
      next: (response) => {
        this.messageContent.set(response.suggestion);
        this.isGettingSuggestion.set(false);
      },
      error: () => this.isGettingSuggestion.set(false),
    });
  }

  scrollToBottom() {
    setTimeout(() => {
      if (this.messageEndRef) {
        this.messageEndRef.nativeElement.scrollIntoView({ behavior: 'smooth' });
      }
    });
  }

  ngOnDestroy(): void {
    this.messageService.stopHubConnection();
  }
}
