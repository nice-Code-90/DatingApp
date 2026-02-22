import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Member } from '../../types/member';
import { PaginatedResult } from '../../types/pagination';
import { of, tap } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class LikesService {
  private baseUrl = environment.apiUrl;
  private http = inject(HttpClient);

  likeIds = signal<string[]>([]);
  private needsRefresh = signal(true);
  private lastResult = signal<PaginatedResult<Member> | null>(null);
  private lastPredicate = '';

  markDirty() {
    this.needsRefresh.set(true);
  }

  toggleLike(targetMemberId: string) {
    return this.http.post(`${this.baseUrl}likes/${targetMemberId}`, {}).subscribe({
      next: () => {
        this.markDirty();
        if (this.likeIds().includes(targetMemberId)) {
          this.likeIds.update((ids) => ids.filter((x) => x !== targetMemberId));
        } else {
          this.likeIds.update((ids) => [...ids, targetMemberId]);
        }
      },
    });
  }

  getLikes(predicate: string, pageNumber: number, pageSize: number) {
    if (!this.needsRefresh() && this.lastResult() && this.lastPredicate === predicate) {
      return of(this.lastResult()!);
    }
    let params = new HttpParams();

    params = params.append('pageNumber', pageNumber);
    params = params.append('pageSize', pageSize);
    params = params.append('predicate', predicate);
    params = params.append('_t', Date.now().toString()); // cache buster

    return this.http.get<PaginatedResult<Member>>(this.baseUrl + 'likes', { params }).pipe(
      tap((res) => {
        this.lastResult.set(res);
        this.lastPredicate = predicate;
        this.needsRefresh.set(false);
      }),
    );
  }

  getLikeIds() {
    const params = new HttpParams().set('_t', Date.now().toString());
    return this.http.get<string[]>(this.baseUrl + 'likes/list', { params }).subscribe({
      next: (ids) => {
        this.likeIds.set(ids);
        this.markDirty();
      },
    });
  }

  clearLikeIds() {
    this.likeIds.set([]);
  }
}
