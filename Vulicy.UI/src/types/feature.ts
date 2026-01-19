export interface FeatureProperties {
  Id: number;
  Type: number;
  NameBeTarask?: string;
  NameBeNark?: string;
  NameRu?: string;
  Classification: number;
  EtymologyBeTarask?: string;
  RenamingReason?: string;
  NamingCategoryId?: number;
  HistoricNames?: string;
  YearNamed?: number;
  ForumRelativeLink?: string;
}

export interface SearchResult {
  id: number;
  type: number;
  nameBeTarask?: string;
  nameBeNark?: string;
  nameRu?: string;
  latitude: number;
  longitude: number;
}

export interface NamingCategory {
  id: number;
  name: string;
}

export interface Viewport {
  lat: number;
  lng: number;
}
