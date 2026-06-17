import { Component, inject, signal, computed, OnInit, input } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { DeckService } from '../../../core/services/deck.service';
import { CardService } from '../../../core/services/card.service';
import { DeckDetail } from '../../../core/models/deck.model';
import { Card, PagedResult } from '../../../core/models/card.model';
import { PaginationComponent } from '../../../shared/components/pagination/pagination.component';
import { isExtraTypeCard } from '../../../core/utils/card.utils';

@Component({
  selector: 'app-deck-builder',
  standalone: true,
  imports: [FormsModule, RouterLink, PaginationComponent],
  templateUrl: './deck-builder.component.html',
})
export class DeckBuilderComponent implements OnInit {
  readonly id = input.required<string>();

  private readonly deckService = inject(DeckService);
  private readonly cardService = inject(CardService);
  private readonly router = inject(Router);

  deck = signal<DeckDetail | null>(null);
  loading = signal(true);
  saving = signal(false);
  saveSuccess = signal(false);
  validationErrors = signal<string[]>([]);

  searchName = '';
  browserCards = signal<PagedResult<Card> | null>(null);
  browserLoading = signal(false);
  browserPage = signal(1);

  deckName = signal('');
  deckDesc = signal('');

  mainDeck = signal<Card[]>([]);
  extraDeck = signal<Card[]>([]);
  sideDeck = signal<Card[]>([]);

  mainCount = computed(() => this.mainDeck().length);
  extraCount = computed(() => this.extraDeck().length);
  sideCount = computed(() => this.sideDeck().length);

  // Drag state
  draggedCard: Card | null = null;
  dragOverZone: string | null = null;
  dragSource: 'browser' | 'deck' | null = null;
  draggedFrom: { zone: 'main' | 'extra' | 'side'; index: number } | null = null;

  ngOnInit(): void {
    this.loadDeck();
    this.searchCards();
  }

  loadDeck(): void {
    this.loading.set(true);
    this.deckService.getDeck(+this.id()).subscribe({
      next: d => {
        this.deck.set(d);
        this.deckName.set(d.name);
        this.deckDesc.set(d.description ?? '');
        this.mainDeck.set([...d.mainDeck]);
        this.extraDeck.set([...d.extraDeck]);
        this.sideDeck.set([...d.sideDeck]);
        this.loading.set(false);
      },
      error: () => { this.loading.set(false); this.router.navigate(['/decks']); }
    });
  }

  private searchTimer: ReturnType<typeof setTimeout> | null = null;

  searchCards(page = 1): void {
    this.browserPage.set(page);
    this.browserLoading.set(true);
    this.cardService.getCards({ name: this.searchName || undefined, page, pageSize: 12 }).subscribe({
      next: r => { this.browserCards.set(r); this.browserLoading.set(false); },
      error: () => this.browserLoading.set(false)
    });
  }

  onBrowserSearchInput(): void {
    if (this.searchTimer) clearTimeout(this.searchTimer);
    this.searchTimer = setTimeout(() => this.searchCards(1), 400);
  }

  isExtraType(card: Card): boolean {
    return isExtraTypeCard(card.type);
  }

  countInDeck(card: Card): number {
    return [...this.mainDeck(), ...this.extraDeck(), ...this.sideDeck()]
      .filter(c => c.id === card.id).length;
  }

  // ── Drag handlers ──────────────────────────────────────────────

  onBrowserDragStart(event: DragEvent, card: Card): void {
    this.draggedCard = card;
    this.dragSource = 'browser';
    this.draggedFrom = null;
    if (event.dataTransfer) event.dataTransfer.effectAllowed = 'copy';
  }

  onDeckDragStart(event: DragEvent, card: Card, zone: 'main' | 'extra' | 'side', index: number): void {
    this.draggedCard = card;
    this.dragSource = 'deck';
    this.draggedFrom = { zone, index };
    if (event.dataTransfer) event.dataTransfer.effectAllowed = 'move';
  }

  onDragEnd(): void {
    this.draggedCard = null;
    this.dragOverZone = null;
    this.dragSource = null;
    this.draggedFrom = null;
  }

  onDragOver(event: DragEvent, zone: string): void {
    event.preventDefault();
    if (event.dataTransfer) event.dataTransfer.dropEffect = this.dragSource === 'deck' ? 'move' : 'copy';
    this.dragOverZone = zone;
  }

  // Browser panel only accepts drops when dragging from deck (= remove)
  onBrowserPanelDragOver(event: DragEvent): void {
    if (this.dragSource !== 'deck') return;
    event.preventDefault();
    this.dragOverZone = 'remove';
  }

  onDragLeave(event: DragEvent, zone: string): void {
    const el = event.currentTarget as Element;
    if (!el.contains(event.relatedTarget as Node) && this.dragOverZone === zone) {
      this.dragOverZone = null;
    }
  }

  // Drop on a deck zone — add (from browser) or move (from deck)
  onDrop(event: DragEvent, zone: 'main' | 'extra' | 'side'): void {
    event.preventDefault();
    this.dragOverZone = null;
    const card = this.draggedCard;
    const source = this.dragSource;
    const from = this.draggedFrom;
    this.draggedCard = null;
    this.dragSource = null;
    this.draggedFrom = null;
    if (!card) return;

    if (source === 'deck' && from) {
      if (from.zone === zone) return;
      // Validate before removing from source
      if (zone === 'main' && (this.isExtraType(card) || this.mainCount() >= 60)) return;
      if (zone === 'extra' && (!this.isExtraType(card) || this.extraCount() >= 15)) return;
      if (zone === 'side' && this.sideCount() >= 15) return;
      // Move: remove from source, add to target
      if (from.zone === 'main') this.mainDeck.update(d => d.filter((_, i) => i !== from.index));
      else if (from.zone === 'extra') this.extraDeck.update(d => d.filter((_, i) => i !== from.index));
      else this.sideDeck.update(d => d.filter((_, i) => i !== from.index));
      if (zone === 'main') this.mainDeck.update(d => [...d, card]);
      else if (zone === 'extra') this.extraDeck.update(d => [...d, card]);
      else this.sideDeck.update(d => [...d, card]);
    } else if (source === 'browser') {
      if (this.countInDeck(card) >= 3) return;
      if (zone === 'main') {
        if (this.isExtraType(card) || this.mainCount() >= 60) return;
        this.mainDeck.update(d => [...d, card]);
      } else if (zone === 'extra') {
        if (!this.isExtraType(card) || this.extraCount() >= 15) return;
        this.extraDeck.update(d => [...d, card]);
      } else {
        if (this.sideCount() >= 15) return;
        this.sideDeck.update(d => [...d, card]);
      }
    }
    this.validationErrors.set([]);
  }

  // Drop on browser panel — remove from deck
  onDropRemove(event: DragEvent): void {
    event.preventDefault();
    this.dragOverZone = null;
    if (this.dragSource !== 'deck' || !this.draggedFrom) return;
    const { zone, index } = this.draggedFrom;
    if (zone === 'main') this.mainDeck.update(d => d.filter((_, i) => i !== index));
    else if (zone === 'extra') this.extraDeck.update(d => d.filter((_, i) => i !== index));
    else this.sideDeck.update(d => d.filter((_, i) => i !== index));
    this.draggedCard = null;
    this.dragSource = null;
    this.draggedFrom = null;
    this.validationErrors.set([]);
  }

  save(): void {
    this.saving.set(true);
    this.saveSuccess.set(false);
    this.deckService.saveDeck(+this.id(), {
      name: this.deckName(),
      description: this.deckDesc() || undefined,
      mainDeck: this.mainDeck().map(c => c.id),
      extraDeck: this.extraDeck().map(c => c.id),
      sideDeck: this.sideDeck().map(c => c.id),
    }).subscribe({
      next: () => {
        this.saving.set(false);
        this.saveSuccess.set(true);
        setTimeout(() => this.saveSuccess.set(false), 3000);
      },
      error: () => this.saving.set(false)
    });
  }

  validate(): void {
    this.deckService.validateDeck(+this.id()).subscribe({
      next: r => this.validationErrors.set(r.errors)
    });
  }
}
