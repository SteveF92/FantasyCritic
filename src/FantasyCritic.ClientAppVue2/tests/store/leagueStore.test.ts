import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { useLeagueStore } from '@/store/leagueStore';

function finishedDraft(draftNumber: number) {
  return {
    draftNumber,
    playStatus: 'DraftFinal',
    scheduledDate: undefined,
    draftOrderSet: true,
    playStarted: true,
    draftIsActive: false,
    draftIsPaused: false,
    draftFinished: true,
    readyToSetDraftOrder: false
  };
}

function activeDraft(draftNumber: number) {
  return { ...finishedDraft(draftNumber), playStatus: 'Drafting', draftIsActive: true, draftFinished: false };
}

function pendingDraft(draftNumber: number, overrides = {}) {
  return { ...finishedDraft(draftNumber), playStatus: 'NotStartedDraft', draftOrderSet: false, playStarted: false, draftFinished: false, ...overrides };
}

function leagueYear(overrides = {}) {
  return {
    enableBids: true,
    underReview: false,
    supportedYear: { finished: false },
    settings: { pickupSystem: 'SecretBidding', tradingSystem: 'Standard', oneShotMode: false, hasSpecialSlots: false },
    drafts: [finishedDraft(1)],
    publishers: [],
    publicBiddingGames: null,
    ...overrides
  };
}

function storeWith(overrides = {}) {
  const store = useLeagueStore();
  store.leagueYear = leagueYear(overrides);
  return store;
}

beforeEach(() => {
  setActivePinia(createPinia());
});

describe('counterPickBiddingAllowed', () => {
  const semiPublicCounterPicks = { pickupSystem: 'SemiPublicBiddingSemiPublicCounterPicks' };

  it('is false before the first draft finishes', () => {
    expect(storeWith({ drafts: [pendingDraft(1)] }).counterPickBiddingAllowed).toBe(false);
  });

  it('is false during a draft', () => {
    expect(storeWith({ drafts: [finishedDraft(1), activeDraft(2)] }).counterPickBiddingAllowed).toBe(false);
  });

  it('is false when bids are disabled', () => {
    expect(storeWith({ enableBids: false }).counterPickBiddingAllowed).toBe(false);
  });

  it('is true for pickup systems without semi-public counter picks', () => {
    const store = storeWith({ settings: { pickupSystem: 'SemiPublicBidding' }, publicBiddingGames: { anyHiddenCounterPickBids: false } });
    expect(store.counterPickBiddingAllowed).toBe(true);
  });

  it('is true with semi-public counter picks before the public bidding reveal', () => {
    expect(storeWith({ settings: semiPublicCounterPicks, publicBiddingGames: null }).counterPickBiddingAllowed).toBe(true);
  });

  it('stays open after the reveal while something is already being counter picked', () => {
    expect(storeWith({ settings: semiPublicCounterPicks, publicBiddingGames: { anyHiddenCounterPickBids: true } }).counterPickBiddingAllowed).toBe(true);
  });

  it('is closed after the reveal while nothing is being counter picked', () => {
    expect(storeWith({ settings: semiPublicCounterPicks, publicBiddingGames: { anyHiddenCounterPickBids: false } }).counterPickBiddingAllowed).toBe(false);
  });
});

describe('pending draft getters', () => {
  // Midday, well away from a daylight saving change, so day arithmetic is unambiguous.
  beforeEach(() => {
    vi.useFakeTimers();
    vi.setSystemTime(new Date(2026, 5, 10, 12, 0));
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  function withSecondDraft(overrides = {}) {
    return storeWith({ drafts: [finishedDraft(1), pendingDraft(2, overrides)] });
  }

  describe('hasPendingOrActiveDraft', () => {
    it('is false when every draft is finished', () => {
      expect(storeWith().hasPendingOrActiveDraft).toBe(false);
    });

    it('is true with a pending draft', () => {
      expect(withSecondDraft().hasPendingOrActiveDraft).toBe(true);
    });

    it('is true with an active draft', () => {
      expect(storeWith({ drafts: [finishedDraft(1), activeDraft(2)] }).hasPendingOrActiveDraft).toBe(true);
    });
  });

  describe('pendingDraftIsWithinSevenDays', () => {
    it('is false with no scheduled date', () => {
      expect(withSecondDraft().pendingDraftIsWithinSevenDays).toBe(false);
    });

    it('is true seven days out', () => {
      expect(withSecondDraft({ scheduledDate: '2026-06-17' }).pendingDraftIsWithinSevenDays).toBe(true);
    });

    it('is false eight days out', () => {
      expect(withSecondDraft({ scheduledDate: '2026-06-18' }).pendingDraftIsWithinSevenDays).toBe(false);
    });
  });

  describe('pendingDraftIsOverdue', () => {
    it('is false with no scheduled date', () => {
      expect(withSecondDraft().pendingDraftIsOverdue).toBe(false);
    });

    it('is false on the scheduled day', () => {
      expect(withSecondDraft({ scheduledDate: '2026-06-10' }).pendingDraftIsOverdue).toBe(false);
    });

    it('is true the day after', () => {
      expect(withSecondDraft({ scheduledDate: '2026-06-09' }).pendingDraftIsOverdue).toBe(true);
    });
  });

  describe('pendingDraftIsImminent', () => {
    it('is false with no pending draft', () => {
      expect(storeWith().pendingDraftIsImminent).toBe(false);
    });

    it('is true for the first draft', () => {
      expect(storeWith({ drafts: [pendingDraft(1)] }).pendingDraftIsImminent).toBe(true);
    });

    it('is true when the draft order is set and no date is scheduled', () => {
      expect(withSecondDraft({ draftOrderSet: true }).pendingDraftIsImminent).toBe(true);
    });

    it('is false when the draft order is not set and no date is scheduled', () => {
      expect(withSecondDraft().pendingDraftIsImminent).toBe(false);
    });

    it('is true within seven days', () => {
      expect(withSecondDraft({ scheduledDate: '2026-06-17' }).pendingDraftIsImminent).toBe(true);
    });

    it('is false further out', () => {
      expect(withSecondDraft({ scheduledDate: '2026-06-18' }).pendingDraftIsImminent).toBe(false);
    });
  });

  describe('pendingDraftCalloutVisible', () => {
    it('is hidden with no pending draft', () => {
      expect(storeWith().pendingDraftCalloutVisible).toBe(false);
    });

    it('is hidden while another draft is active', () => {
      expect(storeWith({ drafts: [finishedDraft(1), activeDraft(2), pendingDraft(3)] }).pendingDraftCalloutVisible).toBe(false);
    });

    it('is shown for the first draft', () => {
      expect(storeWith({ drafts: [pendingDraft(1)] }).pendingDraftCalloutVisible).toBe(true);
    });

    it('is shown when overdue', () => {
      expect(withSecondDraft({ scheduledDate: '2026-06-09' }).pendingDraftCalloutVisible).toBe(true);
    });

    it('is shown within seven days', () => {
      expect(withSecondDraft({ scheduledDate: '2026-06-17' }).pendingDraftCalloutVisible).toBe(true);
    });

    it('is hidden further out', () => {
      expect(withSecondDraft({ scheduledDate: '2026-06-18' }).pendingDraftCalloutVisible).toBe(false);
    });

    it('is hidden when the draft order is set and no date is scheduled', () => {
      expect(withSecondDraft({ draftOrderSet: true }).pendingDraftCalloutVisible).toBe(false);
    });
  });
});
