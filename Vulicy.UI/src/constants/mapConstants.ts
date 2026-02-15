export const SOURCES_CADASTRE_COLOR = '#8B0A50'; // Cadastre features
export const SOURCES_OSM_COLOR = '#FF8C00'; // OSM features

export const CLASSIFICATION_COLORS: Record<number, string> = {
  1: '#ff4d4f', // Priority - Vibrant Red
  2: '#ff7875', // Required - Lighter Red
  3: '#ffa940', // Suggested - Orange
  4: '#ffec3d', // Possible - Yellow
  5: '#73d13d', // Unneeded - Green
  0: '#cbbef8ff', // None/Unknown - Purple
};

export const FEATURE_TYPE_LABELS: Record<number, string> = {
  11: 'вул.',
  12: 'пр-т',
  14: 'пл.',
  15: 'бульв.',
  16: 'тракт',
  17: 'наб.',
  18: 'шаша',
  19: 'кальцо',
  21: 'зав.',
  22: 'пр-зд',
  23: 'тупік',
  24: 'спуск',
  25: 'заезд',
  34: 'парк',
  39: 'сквэр',
};

const CLASSIFICATION_TEXTS: Record<number, string> = {
  0: 'Статус невядомы',
  1: 'Перайменаваньне неабходнае ў прыярытэтным парадку',
  2: 'Перайменаваньне неабходнае',
  3: 'Перайменаваньне пажаданае',
  4: 'Перайменаваньне магчымае',
  5: 'Перайменаваньне не патрэбнае',
};

export function getClassificationText(lvl: number | string): string {
  const level = typeof lvl === 'string' ? parseInt(lvl, 10) : lvl;
  return CLASSIFICATION_TEXTS[level] || CLASSIFICATION_TEXTS[0];
}

// Dropdown options for editing
export const CLASSIFICATION_OPTIONS = [
  { value: 0, label: 'з прывязанага імені' },
  { value: 1, label: 'Перайменаваньне неабходнае ў прыярытэтным парадку' },
  { value: 2, label: 'Перайменаваньне неабходнае' },
  { value: 3, label: 'Перайменаваньне пажаданае' },
  { value: 4, label: 'Перайменаваньне магчымае' },
  { value: 5, label: 'Перайменаваньне не патрэбнае' },
];

export const DOSSIER_CLASSIFICATION_OPTIONS = [{ value: 0, label: 'Статус невядомы' },].concat(CLASSIFICATION_OPTIONS.filter(o => o.value !== 0));

export const FEATURE_TYPE_OPTIONS = [
  { value: 11, label: 'вуліца' },
  { value: 12, label: 'праспэкт' },
  { value: 14, label: 'плошча' },
  { value: 15, label: 'бульвар' },
  { value: 16, label: 'тракт' },
  { value: 17, label: 'набярэжная' },
  { value: 18, label: 'шаша' },
  { value: 19, label: 'кальцо' },
  { value: 21, label: 'завулак' },
  { value: 22, label: 'праезд' },
  { value: 23, label: 'тупік' },
  { value: 24, label: 'спуск' },
  { value: 25, label: 'заезд' },
  { value: 34, label: 'парк' },
  { value: 39, label: 'сквэр' },
];
