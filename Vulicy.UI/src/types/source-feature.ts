// Source feature types for the Sources view

// Cadastre feature properties from cadastre-tile endpoint
export interface CadastreFeatureProperties {
  Id: string;
  ElementNameBel: string;
  ElementName: string;
  ElementTypeShortNameBel: string;
  ShortInfo: string;
  FeatureId: number | null;
}

// OSM feature properties from osm-tile endpoint
// Tags are flattened directly onto the object by PostgreSQL
export interface OsmFeatureProperties {
  Id: number;
  Type: string;
  FeatureId: number | null;
  // Common tag properties (flattened from OSM tags)
  name?: string;
  'name:be'?: string;
  'name:ru'?: string;
  highway?: string;
  // Allow other flattened tag properties
  [key: string]: string | number | null | undefined;
}
