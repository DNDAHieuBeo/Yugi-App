import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Card, CardFilter, MarketFilter, PagedResult } from '../models/card.model';

@Injectable({ providedIn: 'root' })
export class CardService {
  private readonly http = inject(HttpClient);
  private readonly api = `${environment.apiUrl}/cards`;

  getCards(filter: CardFilter = {}): Observable<PagedResult<Card>> {
    let params = new HttpParams();
    if (filter.name) params = params.set('name', filter.name);
    if (filter.desc) params = params.set('desc', filter.desc);
    if (filter.category) params = params.set('category', filter.category);
    if (filter.type) params = params.set('type', filter.type);
    if (filter.race) params = params.set('race', filter.race);
    if (filter.attribute) params = params.set('attribute', filter.attribute);
    if (filter.archetype) params = params.set('archetype', filter.archetype);
    if (filter.level != null) params = params.set('level', filter.level.toString());
    if (filter.minAtk != null) params = params.set('minAtk', filter.minAtk.toString());
    if (filter.maxAtk != null) params = params.set('maxAtk', filter.maxAtk.toString());
    if (filter.minDef != null) params = params.set('minDef', filter.minDef.toString());
    if (filter.maxDef != null) params = params.set('maxDef', filter.maxDef.toString());
    if (filter.banTcg) params = params.set('banTcg', filter.banTcg);
    if (filter.orderBy) params = params.set('orderBy', filter.orderBy);
    if (filter.page) params = params.set('page', filter.page.toString());
    if (filter.pageSize) params = params.set('pageSize', filter.pageSize.toString());
    return this.http.get<PagedResult<Card>>(this.api, { params });
  }

  getMarketCards(filter: MarketFilter = {}): Observable<PagedResult<Card>> {
    let params = new HttpParams();
    if (filter.name)       params = params.set('name', filter.name);
    if (filter.category)   params = params.set('category', filter.category);
    if (filter.archetype)  params = params.set('archetype', filter.archetype);
    if (filter.minPrice != null) params = params.set('minPrice', filter.minPrice.toString());
    if (filter.maxPrice != null) params = params.set('maxPrice', filter.maxPrice.toString());
    if (filter.priceSource) params = params.set('priceSource', filter.priceSource);
    if (filter.orderBy)     params = params.set('orderBy', filter.orderBy);
    if (filter.page)        params = params.set('page', filter.page.toString());
    if (filter.pageSize)    params = params.set('pageSize', filter.pageSize.toString());
    return this.http.get<PagedResult<Card>>(`${this.api}/market`, { params });
  }

  getCard(id: number): Observable<Card> {
    return this.http.get<Card>(`${this.api}/${id}`);
  }

  syncCards(): Observable<{ synced: number }> {
    return this.http.post<{ synced: number }>(`${this.api}/sync`, {});
  }
}
