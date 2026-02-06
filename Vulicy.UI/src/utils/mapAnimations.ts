import { Map as MapLibreMap } from 'maplibre-gl';

/**
 * Interpolates between two hex colors.
 * @param hex1 Starting hex color (e.g., '#ff0000')
 * @param hex2 Ending hex color (e.g., '#ffffff')
 * @param factor Interpolation factor between 0 and 1
 * @returns RGB string (e.g., 'rgb(255, 128, 128)')
 */
export const interpolateHex = (hex1: string, hex2: string, factor: number): string => {
  const r1 = parseInt(hex1.substring(1, 3), 16);
  const g1 = parseInt(hex1.substring(3, 5), 16);
  const b1 = parseInt(hex1.substring(5, 7), 16);

  const r2 = parseInt(hex2.substring(1, 3), 16);
  const g2 = parseInt(hex2.substring(3, 5), 16);
  const b2 = parseInt(hex2.substring(5, 7), 16);

  const r = Math.round(r1 + factor * (r2 - r1));
  const g = Math.round(g1 + factor * (g2 - g1));
  const b = Math.round(b1 + factor * (b2 - b1));

  return `rgb(${r}, ${g}, ${b})`;
};

/**
 * Configuration for a single pulsing layer.
 */
export interface PulseLayerConfig {
  layerId: string;
  baseColor: string;
}

/**
 * Creates a pulse animation function for map selection glow effects.
 * The animation interpolates colors and opacity in a sine wave pattern.
 * 
 * @param mapRef Reference to the MapLibre map instance
 * @param animationFrameId Reference to store the animation frame ID for cleanup
 * @param getLayerConfigs Callback that returns layer configs to animate, or null to stop
 * @returns The pulse animation function to be called with requestAnimationFrame
 */
export const createPulseAnimation = (
  mapRef: React.MutableRefObject<MapLibreMap | null>,
  animationFrameId: React.MutableRefObject<number | null>,
  getLayerConfigs: () => PulseLayerConfig[] | null
) => {
  const startTime = Date.now();
  const duration = 1500;

  const pulse = (): void => {
    if (!mapRef.current) return;

    const configs = getLayerConfigs();
    if (!configs || configs.length === 0) {
      // No active selections - stop animation loop
      animationFrameId.current = null;
      return;
    }

    const time = (Date.now() - startTime) % duration;
    const t = (Math.sin((time / duration) * Math.PI * 2) + 1) / 2;

    for (const { layerId, baseColor } of configs) {
      if (mapRef.current.getLayer(layerId)) {
        const interpolatedColor = interpolateHex(baseColor, '#ffffff', t);
        mapRef.current.setPaintProperty(layerId, 'line-color', interpolatedColor);
        mapRef.current.setPaintProperty(layerId, 'line-opacity', 0.4 + t * 0.4);
        mapRef.current.setPaintProperty(layerId, 'line-blur', 2 + t * 8);
      }
    }

    animationFrameId.current = requestAnimationFrame(pulse);
  };

  return pulse;
};
