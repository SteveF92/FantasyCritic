import { beforeEach, describe, expect, it, vi } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { ApiException, leagueClient } from '@/api/clients';
import type { LeagueYearViewModel, PublisherViewModel } from '@/api/clients';
import { usePublisherStore } from '@/store/publisherStore';

const publisherID = '11111111-1111-1111-1111-111111111111';

function publisher() {
  return { publisherID, gameSlots: [], formerGames: [] } as unknown as PublisherViewModel;
}

function leagueYearWithPublisher() {
  return { publishers: [publisher()] } as unknown as LeagueYearViewModel;
}

function storeWithPublisher() {
  const store = usePublisherStore();
  store.publisher = publisher();
  return store;
}

function badRequest(message: string) {
  return new ApiException('Bad Request', 400, message, { 'content-type': 'text/plain; charset=utf-8' }, null);
}

describe('publisherStore move errors', () => {
  beforeEach(() => {
    setActivePinia(createPinia());
    vi.restoreAllMocks();
    vi.spyOn(leagueClient, 'getLeagueYearForPublisher').mockResolvedValue(leagueYearWithPublisher());
  });

  it('shows the server message when a move fails', async () => {
    vi.spyOn(leagueClient, 'reorderPublisherGames').mockRejectedValue(badRequest('You cannot move that game.'));
    const store = storeWithPublisher();

    await store.confirmPositions();

    expect(store.moveGameError).toBe('You cannot move that game.');
  });

  it('clears the error when a later move succeeds', async () => {
    vi.spyOn(leagueClient, 'reorderPublisherGames').mockRejectedValueOnce(badRequest('You cannot move that game.')).mockResolvedValueOnce(undefined);
    const store = storeWithPublisher();

    await store.confirmPositions();
    await store.confirmPositions();

    expect(store.moveGameError).toBe('');
  });

  it('clears the error when the store is cleared for another page', () => {
    const store = storeWithPublisher();
    store.moveGameError = 'You cannot move that game.';

    store.clearPublisherStoreData();

    expect(store.moveGameError).toBe('');
  });
});
