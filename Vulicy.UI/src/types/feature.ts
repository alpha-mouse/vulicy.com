import type { GeoJSONGeometry } from '../utils/geometry';

export interface NamedFeature {
  id: number;
  nameBeTarask?: string;
  nameBeNark?: string;
  nameRu?: string;
}

export interface FeatureProperties extends NamedFeature {
  type: number;
  nameBeTarask?: string;
  nameBeNark?: string;
  nameRu?: string;
  classification: number;
  dossierRecordNameBeTarask?: string;
  renamingReason?: string;
  namingCategoryId?: number;
  historicNames?: string;
  historicPossible: boolean;
  yearNamed?: string;
  forumRelativeLink?: string;
  // Admin-only fields from tile-details endpoint
  comment?: string;
  dossierRecordId?: number;
  dossierRecordClassification?: number;
  dossierRecordDescriptionBe?: string;
  dossierRecordDescriptionRu?: string;
  dossierRecordNamingCategoryId?: number;
}

export interface SearchResult extends NamedFeature {
  type: number;
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

export function getFeatureName(result: NamedFeature): string {
  return result.nameBeTarask || result.nameBeNark || result.nameRu || `#${result.id}`;
}

// Preview request to POST /api/features/preview
export interface FeaturePreviewRequest {
  osmId: number;
  osmType: number;
  cadastreId: string;
}

// Preview response - minimal details for pre-filling form
export interface FeaturePreviewResponse {
  geometry: GeoJSONGeometry;
  nameBeTarask: string;
  nameBeNark: string;
  nameRu: string;
  classification: number;
  type: number;
  renamingReason: string | null;
  historicNames: string | null;
  historicPossible: boolean;
  yearNamed: string | null;
  comment: string | null;
  dossierRecordId: number | null;
  dossierRecordNameBeTarask: string | null;
  namingCategoryId: number | null;
}

// Create request to POST /api/features/from-sources
export interface FeatureCreateFromSourcesRequest extends FeatureEditRequest {
  osmId: number;
  osmType: number;
  cadastreId: string;
}
