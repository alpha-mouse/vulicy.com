export const CLASSIFICATION_COLORS: Record<number, string> = {
  1: '#ff4d4f', // Priority - Vibrant Red
  2: '#ff7875', // Required - Lighter Red
  3: '#ffa940', // Suggested - Orange
  4: '#ffec3d', // Possible - Yellow
  5: '#73d13d', // Unneeded - Green
  0: '#d9d9d9', // None/Unknown - Grey
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
