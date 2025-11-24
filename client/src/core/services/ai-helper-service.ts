import { inject, Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';

interface AiSuggestion {
  suggestion: string;
}

@Injectable({
  providedIn: 'root',
})
export class AiHelperService {
  private baseUrl = environment.apiUrl;

  private http = inject(HttpClient);

  getSuggestion(recipientId: string) {
    return this.http.get<AiSuggestion>(`${this.baseUrl}aihelper/suggestion/${recipientId}`);
  }
}
