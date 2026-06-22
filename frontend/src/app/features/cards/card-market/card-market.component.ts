import { Component, inject, signal, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CardService } from '../../../core/services/card.service';
import { Card, MarketFilter, PagedResult } from '../../../core/models/card.model';
import { CardDetailPanelComponent } from '../../../shared/components/card-detail-panel/card-detail-panel.component';
import { PaginationComponent } from '../../../shared/components/pagination/pagination.component';
import { BtnComponent } from '../../../shared/components/common/button/btn.component';

type PriceSource = 'cardmarket' | 'tcgplayer' | 'ebay' | 'amazon';
type SortOrder   = 'price_desc' | 'price_asc' | 'name';

@Component({
  selector: 'app-card-market',
  standalone: true,
  imports: [FormsModule, CardDetailPanelComponent, PaginationComponent, BtnComponent],
  templateUrl: './card-market.component.html',
})
export class CardMarketComponent implements OnInit {
  private readonly cardService = inject(CardService);

  result       = signal<PagedResult<Card> | null>(null);
  loading      = signal(false);
  currentPage  = signal(1);
  selectedCard = signal<Card | null>(null);

  // Filters
  filterName     = '';
  filterCategory = '';
  filterMinPrice: number | null = null;
  filterMaxPrice: number | null = null;
  priceSource: PriceSource = 'cardmarket';
  sortOrder: SortOrder = 'price_desc';

  readonly priceSources: { value: PriceSource; label: string }[] = [
    { value: 'cardmarket', label: 'Cardmarket' },
    { value: 'tcgplayer',  label: 'TCGPlayer' },
    { value: 'ebay',       label: 'eBay' },
    { value: 'amazon',     label: 'Amazon' },
  ];

  readonly sortOptions: { value: SortOrder; label: string }[] = [
    { value: 'price_desc', label: 'Price (High → Low)' },
    { value: 'price_asc',  label: 'Price (Low → High)' },
    { value: 'name',       label: 'Name (A–Z)' },
  ];

  ngOnInit(): void {
    this.load(1);
  }

  load(page = 1): void {
    this.currentPage.set(page);
    this.loading.set(true);
    const filter: MarketFilter = {
      page,
      pageSize: 24,
      priceSource: this.priceSource,
      orderBy: this.sortOrder,
      name:     this.filterName     || undefined,
      category: this.filterCategory || undefined,
      minPrice: this.filterMinPrice ?? undefined,
      maxPrice: this.filterMaxPrice ?? undefined,
    };
    this.cardService.getMarketCards(filter).subscribe({
      next: r => { this.result.set(r); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }

  onSourceChange(): void { this.load(1); }
  onSortChange():   void { this.load(1); }
  onSearch():       void { this.load(1); }

  clear(): void {
    this.filterName     = '';
    this.filterCategory = '';
    this.filterMinPrice = null;
    this.filterMaxPrice = null;
    this.load(1);
  }

  priceOf(card: Card): number | undefined {
    switch (this.priceSource) {
      case 'tcgplayer': return card.tcgplayerPrice;
      case 'ebay':      return card.ebayPrice;
      case 'amazon':    return card.amazonPrice;
      default:          return card.cardmarketPrice;
    }
  }

  formatPrice(val?: number): string {
    if (val == null) return '—';
    return '$' + val.toFixed(2);
  }
}
