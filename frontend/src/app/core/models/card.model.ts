export interface Card {
  id: number;
  name: string;
  type: string;
  frameType: string;
  desc: string;
  atk?: number;
  def?: number;
  level?: number;
  race?: string;
  attribute?: string;
  archetype?: string;
  imageUrl?: string;
  imageUrlSmall?: string;
  banTcg?: string;
  banOcg?: string;
  cardmarketPrice?: number;
  tcgplayerPrice?: number;
  ebayPrice?: number;
  amazonPrice?: number;
}

export interface MarketFilter {
  name?: string;
  category?: string;
  archetype?: string;
  minPrice?: number;
  maxPrice?: number;
  priceSource?: 'cardmarket' | 'tcgplayer' | 'ebay' | 'amazon';
  orderBy?: 'price_desc' | 'price_asc' | 'name';
  page?: number;
  pageSize?: number;
}

export interface CardFilter {
  name?: string;
  desc?: string;
  category?: string;
  type?: string;
  race?: string;
  attribute?: string;
  archetype?: string;
  level?: number;
  minAtk?: number;
  maxAtk?: number;
  minDef?: number;
  maxDef?: number;
  banTcg?: string;
  orderBy?: string;
  page?: number;
  pageSize?: number;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}
