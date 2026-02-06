import type { GeoJSONGeometry } from '../utils/geometry';

// Unified OSM feature type - matches both tile and search responses (camelCase from backend)
// Backend type: OsmFeatureSearchResult
export interface OsmFeature {
  id: number;
  type: number;
  tags: Record<string, string>;
  geometry?: GeoJSONGeometry | null;  // Present in search results, null/absent in tiles
  featureId: number | null;
}

// Unified Cadastre feature type - matches both tile and search responses (camelCase from backend)
// Backend type: CadastreFeatureSearchResult
export interface CadastreFeature {
  id: string;
  geometry?: GeoJSONGeometry | null;  // Present in search results, null/absent in tiles
  elementNameBel: string;
  elementName: string;
  featureId: number | null;
  elementTypeShortNameBel: string;
  shortInfo: string | null;
  location: string | null;
  reason: string | null;
  classification: number | null;
  comment: string | null;
  historicName: string | null;
  historicPossible: boolean;
  yearNamed: string | null;
  nameCategory: string | null;
}

// Helper functions for display names
export function getOsmName(feature: OsmFeature): string {
  return feature.tags['name:be-tarask'] || feature.tags['name:be'] || feature.tags['name'] || feature.tags['name:ru'] || `OSM ${feature.type} #${feature.id}`;
}

export function getCadastreName(feature: CadastreFeature): string {
  return feature.elementNameBel || feature.elementName;
}

// Legacy aliases for backwards compatibility during migration
// TODO: Remove these after all usages are updated
export type OsmFeatureProperties = OsmFeature;
export type CadastreFeatureProperties = CadastreFeature;
