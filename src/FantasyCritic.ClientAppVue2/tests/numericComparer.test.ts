import { describe, expect, it } from 'vitest';
import { numericComparer } from '@/globalFunctions';

function sortByKey(items: unknown[], key: string) {
  return [...items].sort((a, b) => numericComparer(a, b, key));
}

describe('numericComparer', () => {
  it('compares decimals as numbers', () => {
    const items = [{ hype: 91.56 }, { hype: 91.09 }, { hype: 9.5 }];
    expect(sortByKey(items, 'hype')).toEqual([{ hype: 9.5 }, { hype: 91.09 }, { hype: 91.56 }]);
  });

  it('compares negative numbers', () => {
    const items = [{ points: 3 }, { points: -15 }, { points: -2.5 }];
    expect(sortByKey(items, 'points')).toEqual([{ points: -15 }, { points: -2.5 }, { points: 3 }]);
  });

  it('follows a dotted key into nested objects', () => {
    const items = [{ masterGame: { criticScore: 85.2 } }, { masterGame: { criticScore: 85.15 } }];
    expect(sortByKey(items, 'masterGame.criticScore')).toEqual([{ masterGame: { criticScore: 85.15 } }, { masterGame: { criticScore: 85.2 } }]);
  });

  it('sorts missing values before numbers', () => {
    const items = [{ score: 70 }, { score: null }, { score: -1 }, {}];
    expect(sortByKey(items, 'score')).toEqual([{ score: null }, {}, { score: -1 }, { score: 70 }]);
  });

  it('treats a missing parent object as a missing value', () => {
    const items = [{ masterGame: { criticScore: 70 } }, { masterGame: null }];
    expect(sortByKey(items, 'masterGame.criticScore')).toEqual([{ masterGame: null }, { masterGame: { criticScore: 70 } }]);
  });
});
