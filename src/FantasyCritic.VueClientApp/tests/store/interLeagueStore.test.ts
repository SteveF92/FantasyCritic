import { beforeEach, describe, expect, it, vi } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { combinedDataClient } from '@/api/clients';
import type { BasicDataViewModel } from '@/api/clients';
import { useInterLeagueStore } from '@/store/interLeagueStore';

const basicData = {
  bidTimes: { nextPublicBiddingTime: '2026-11-08T00:00:00Z', nextBidLockTime: '2026-11-09T00:00:00Z', actionProcessingMode: false },
  masterGameTags: [],
  leagueOptions: {},
  supportedYears: [],
  activeRoyaleQuarter: { year: 2026, quarter: 4 }
} as unknown as BasicDataViewModel;

describe('interLeagueStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia());
    vi.restoreAllMocks();
  });

  it('throws when its data is read before it loads', () => {
    const store = useInterLeagueStore();

    expect(store.interLeagueDataLoaded).toBe(false);
    expect(() => store.bidTimes).toThrow();
  });

  it('shares one request between overlapping loads', async () => {
    const basicDataCall = vi.spyOn(combinedDataClient, 'basicData').mockResolvedValue(basicData);
    const store = useInterLeagueStore();

    await Promise.all([store.ensureLoaded(), store.ensureLoaded()]);
    await store.ensureLoaded();

    expect(basicDataCall).toHaveBeenCalledTimes(1);
    expect(store.interLeagueDataLoaded).toBe(true);
    expect(store.bidTimes).toEqual(basicData.bidTimes);
  });

  it('retries on the next load after a failed one', async () => {
    const basicDataCall = vi.spyOn(combinedDataClient, 'basicData').mockRejectedValueOnce(new Error('API down')).mockResolvedValueOnce(basicData);
    const store = useInterLeagueStore();

    await expect(store.ensureLoaded()).rejects.toThrow('API down');
    expect(store.interLeagueDataLoaded).toBe(false);

    await store.ensureLoaded();

    expect(basicDataCall).toHaveBeenCalledTimes(2);
    expect(store.interLeagueDataLoaded).toBe(true);
  });
});
