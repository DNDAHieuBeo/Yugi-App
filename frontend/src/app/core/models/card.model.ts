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
