import { describe, expect, it } from 'vitest';
import { reactive } from 'vue';
import { deepClone } from '@/utilities/deepClone';

describe('deepClone', () => {
  it('clones a plain array holding reactive objects', () => {
    const tag = reactive({ name: 'Remake' });
    const clone = deepClone([tag]);
    clone[0].name = 'Port';
    expect(clone).toEqual([{ name: 'Port' }]);
    expect(tag.name).toBe('Remake');
  });

  it('passes null and undefined through', () => {
    expect(deepClone(null)).toBeNull();
    expect(deepClone(undefined)).toBeUndefined();
  });
});
