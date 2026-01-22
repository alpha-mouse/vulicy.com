export interface User {
  id: string;
  username: string;
  email: string;
  name?: string;
  avatarUrl?: string;
  isAdmin: boolean;
}

export interface DossierRecord {
  Id: number;
  NameBeTarask?: string;
  Classification: number;
  DescriptionBe?: string;
  DescriptionRu?: string;
  NamingCategoryId?: number;
}

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
  HistoricPossible: boolean;
  YearNamed?: number;
  ForumRelativeLink?: string;
  // Admin-only fields from tile-details endpoint
  Comment?: string;
  DossierRecordId?: number;
  DossierRecordNameBeTarask?: string;
  DossierRecordClassification?: number;
  DossierRecordDescriptionBe?: string;
  DossierRecordDescriptionRu?: string;
  DossierRecordNamingCategoryId?: number;
}

export interface SearchResult {
  id: number;
  type: number;
  nameBeTarask?: string;
  nameBeNark?: string;
  nameRu?: string;
  location?: string;
  latitude: number;
  longitude: number;
}

export interface DossierRecordSearchResult {
  id: number;
  nameBeTarask?: string;
  nameBeNark?: string;
  nameRu?: string;
  descriptionBe?: string;
  descriptionRu?: string;
  classification: number;
  namingCategoryId?: number;
  numFeatures: number;
}

export interface NamingCategory {
  id: number;
  name: string;
}

export interface Viewport {
  lat: number;
  lng: number;
}

export interface FeatureEditRequest {
  nameBeTarask: string;
  nameBeNark: string;
  nameRu: string;
  classification: number;
  type: number;
  renamingReason: string | null;
  historicNames: string | null;
  comment: string | null;
  historicPossible: boolean;
  yearNamed: string | null;
  namingCategoryId: number | null;
  dossierRecordId: number | null;
}
