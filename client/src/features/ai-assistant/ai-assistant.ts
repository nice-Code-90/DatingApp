import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AiAgentService } from '../../core/services/ai-agent-service';
import { Router } from '@angular/router';
import { ChatMarkdownPipe } from '../../shared/pipes/chat-markdown.pipe';

@Component({
  selector: 'app-ai-assistant',
  standalone: true,
  imports: [FormsModule, CommonModule, ChatMarkdownPipe],
  templateUrl: './ai-assistant.html',
  styleUrl: './ai-assistant.css',
})
export class AiAssistant {
  agentService = inject(AiAgentService);
  private router = inject(Router);
  prompt = '';

  sendMessage() {
    if (!this.prompt.trim()) return;
    this.agentService.sendPrompt(this.prompt).subscribe();
    this.prompt = '';
  }
  handleContentClick(event: MouseEvent) {
    const target = event.target as HTMLElement;

    if (target.tagName === 'A') {
      const href = target.getAttribute('href');

      if (href && href.startsWith('/')) {
        event.preventDefault();
        this.router.navigateByUrl(href);
      }
    }
  }
}
