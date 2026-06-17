import { Component, inject, signal, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CardService } from '../../../core/services/card.service';
import { Card, CardFilter, PagedResult } from '../../../core/models/card.model';
import { CardThumbnailComponent } from '../../../shared/components/card-thumbnail/card-thumbnail.component';
import { PaginationComponent } from '../../../shared/components/pagination/pagination.component';
import { CardDetailPanelComponent } from '../../../shared/components/card-detail-panel/card-detail-panel.component';

const MONSTER_RACES = [
  'Aqua', 'Beast', 'Beast-Warrior', 'Cyberse', 'Dinosaur', 'Divine-Beast',
  'Dragon', 'Fairy', 'Fiend', 'Fish', 'Illusion', 'Insect', 'Machine',
  'Plant', 'Psychic', 'Pyro', 'Reptile', 'Rock', 'Sea Serpent',
  'Spellcaster', 'Thunder', 'Warrior', 'Winged Beast', 'Wyrm', 'Zombie',
];
const SPELL_RACES = ['Normal', 'Quick-Play', 'Continuous', 'Equip', 'Field', 'Ritual'];
const TRAP_RACES  = ['Normal', 'Continuous', 'Counter'];

const MONSTER_TYPES = [
  'Normal Monster', 'Effect Monster', 'Flip Effect Monster', 'Toon Monster',
  'Union Effect Monster', 'Gemini Monster', 'Spirit Monster', 'Tuner Monster',
  'Pendulum Normal Monster', 'Pendulum Effect Monster',
  'Ritual Monster', 'Ritual Effect Monster',
  'Fusion Monster', 'Pendulum Effect Fusion Monster',
  'Synchro Monster', 'Synchro Tuner Monster', 'Synchro Pendulum Effect Monster',
  'XYZ Monster', 'XYZ Pendulum Effect Monster',
  'Link Monster',
];
const SPELL_TYPES = ['Spell Card'];
const TRAP_TYPES  = ['Trap Card'];

const ATTRIBUTES = ['DARK', 'LIGHT', 'EARTH', 'WATER', 'FIRE', 'WIND', 'DIVINE'];
const LEVELS = [1,2,3,4,5,6,7,8,9,10,11,12];
const BAN_STATUS = ['Forbidden', 'Limited', 'Semi-Limited'];
const ORDER_OPTIONS = [
  { value: 'name',  label: 'Name (A–Z)' },
  { value: 'atk',   label: 'ATK (High)' },
  { value: 'def',   label: 'DEF (High)' },
  { value: 'level', label: 'Level (High)' },
];

@Component({
  selector: 'app-card-browser',
  standalone: true,
  imports: [FormsModule, CardThumbnailComponent, PaginationComponent, CardDetailPanelComponent],
  templateUrl: './card-browser.component.html',
})
export class CardBrowserComponent implements OnInit {
  private readonly cardService = inject(CardService);

  readonly attributes = ATTRIBUTES;
  readonly levels = LEVELS;
  readonly banStatuses = BAN_STATUS;
  readonly orderOptions = ORDER_OPTIONS;
  readonly monsterTypes = MONSTER_TYPES;
  readonly spellTypes = SPELL_TYPES;
  readonly trapTypes = TRAP_TYPES;

  filterName = '';
  filterDesc = '';
  filterCategory = '';
  filterType = '';
  filterRace = '';
  filterAttribute = '';
  filterLevel = '';
  filterMinAtk = '';
  filterMaxAtk = '';
  filterMinDef = '';
  filterMaxDef = '';
  filterBan = '';
  filterOrder = 'name';

  get availableRaces(): string[] {
    if (this.filterCategory === 'Monster') return MONSTER_RACES;
    if (this.filterCategory === 'Spell')   return SPELL_RACES;
    if (this.filterCategory === 'Trap')    return TRAP_RACES;
    return [...MONSTER_RACES, ...SPELL_RACES, ...TRAP_RACES].filter((v, i, a) => a.indexOf(v) === i);
  }

  result = signal<PagedResult<Card> | null>(null);
  loading = signal(false);
  syncing = signal(false);
  syncMessage = signal('');
  currentPage = signal(1);
  selectedCard = signal<Card | null>(null);

  ngOnInit(): void {
    this.loadCards();
  }

  onCategoryChange(): void {
    this.filterType = '';
    this.filterRace = '';
    if (this.filterCategory !== 'Monster') {
      this.filterAttribute = '';
      this.filterLevel = '';
      this.filterMinAtk = '';
      this.filterMaxAtk = '';
      this.filterMinDef = '';
      this.filterMaxDef = '';
    }
    this.search();
  }

  private searchTimer: ReturnType<typeof setTimeout> | null = null;

  onTextInput(): void {
    if (this.searchTimer) clearTimeout(this.searchTimer);
    this.searchTimer = setTimeout(() => this.search(), 400);
  }

  search(): void {
    this.loadCards(1);
  }

  reset(): void {
    this.filterName = '';
    this.filterDesc = '';
    this.filterCategory = '';
    this.filterType = '';
    this.filterRace = '';
    this.filterAttribute = '';
    this.filterLevel = '';
    this.filterMinAtk = '';
    this.filterMaxAtk = '';
    this.filterMinDef = '';
    this.filterMaxDef = '';
    this.filterBan = '';
    this.filterOrder = 'name';
    this.loadCards(1);
  }

  loadCards(page = 1): void {
    this.currentPage.set(page);
    this.loading.set(true);
    const f: CardFilter = {
      page, pageSize: 24,
      name:      this.filterName      || undefined,
      desc:      this.filterDesc      || undefined,
      category:  this.filterCategory  || undefined,
      type:      this.filterType      || undefined,
      race:      this.filterRace      || undefined,
      attribute: this.filterAttribute || undefined,
      level:     this.filterLevel     ? +this.filterLevel  : undefined,
      minAtk:    this.filterMinAtk    ? +this.filterMinAtk : undefined,
      maxAtk:    this.filterMaxAtk    ? +this.filterMaxAtk : undefined,
      minDef:    this.filterMinDef    ? +this.filterMinDef : undefined,
      maxDef:    this.filterMaxDef    ? +this.filterMaxDef : undefined,
      banTcg:    this.filterBan       || undefined,
      orderBy:   this.filterOrder     || undefined,
    };
    this.cardService.getCards(f).subscribe({
      next: r => { this.result.set(r); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }

  syncCards(): void {
    this.syncing.set(true);
    this.syncMessage.set('');
    this.cardService.syncCards().subscribe({
      next: res => {
        this.syncing.set(false);
        this.syncMessage.set(`Synced ${res.synced.toLocaleString()} cards!`);
        this.loadCards(1);
        setTimeout(() => this.syncMessage.set(''), 5000);
      },
      error: () => { this.syncing.set(false); this.syncMessage.set('Sync failed.'); },
    });
  }
}
