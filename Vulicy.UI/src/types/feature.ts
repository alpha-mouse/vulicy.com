import type { GeoJSONGeometry } from '../utils/geometry';

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
  geometry: GeoJSONGeometry;
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
