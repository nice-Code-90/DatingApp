import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AiAgentService } from '../../core/services/ai-agent-service';

@Component({
  selector: 'app-ai-assistant',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './ai-assistant.html',
  styleUrl: './ai-assistant.css',
})
export class AiAssistant {
  agentService = inject(AiAgentService);
  prompt = '';

  sendMessage() {
    if (!this.prompt.trim()) return;
    this.agentService.sendPrompt(this.prompt).subscribe();
    this.prompt = '';
  }
}
