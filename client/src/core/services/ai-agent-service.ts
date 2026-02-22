import { inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { finalize, tap } from 'rxjs';
import { LikesService } from './likes-service';

export interface AgentResponse {
  message: string;
  actionTaken?: string;
  suggestions?: string[];
  affectedTargetIds?: string[];
}

export interface ChatMessage {
  role: 'user' | 'assistant';
  content: string;
}

@Injectable({ providedIn: 'root' })
export class AiAgentService {
  private http = inject(HttpClient);
  private likesService = inject(LikesService);
  private baseUrl = environment.apiUrl + 'aiagent/';

  messages = signal<ChatMessage[]>([
    {
      role: 'assistant',
      content:
        "Hello! I'm your AI Wingman. I don't just search; I can take action for you. " +
        "Try saying: 'Find active women in London who love traveling and like the best match for me!'",
    },
  ]);
  isLoading = signal(false);

  sendPrompt(prompt: string) {
    this.messages.update((prev) => [...prev, { role: 'user', content: prompt }]);
    this.isLoading.set(true);

    return this.http.post<AgentResponse>(this.baseUrl + 'process', { prompt }).pipe(
      tap((res) => {
        this.messages.update((prev) => [...prev, { role: 'assistant', content: res.message }]);

        setTimeout(() => {
          this.likesService.getLikeIds();
          this.likesService.markDirty();
        }, 500);
      }),
      finalize(() => this.isLoading.set(false)),
    );
  }
}
