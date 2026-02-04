import centroid from '@turf/centroid';
import type { Geometry } from '@turf/helpers';

export type GeoJSONGeometry = Geometry;

/**
 * Computes the centroid of a GeoJSON geometry using Turf.js.
 * Returns [longitude, latitude] coordinates.
 */
export function computeCentroid(geometry: GeoJSONGeometry): [number, number] {
  const center = centroid(geometry);
  return center.geometry.coordinates as [number, number];
}
