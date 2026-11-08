import { defineStore } from 'pinia';
import { useAuthStore } from '@/store/authStore';
import { deepClone } from '@/utilities/deepClone';
import { getApiErrorMessage } from '@/utilities/apiError';
import { leagueClient, LeagueYearViewModel, PublisherSlotViewModel, PublisherViewModel, ReorderPublisherGamesRequest } from '@/api/clients';

interface PublisherStoreState {
  publisher: PublisherViewModel | null;
  leagueYear: LeagueYearViewModel | null;
  cleanGameSlots: PublisherSlotViewModel[] | null;
  editableGameSlots: PublisherSlotViewModel[] | null;
  desiredPositions: Record<number, string | null>;
  moveMode: boolean;
  heldSlot: PublisherSlotViewModel | null;
  moveGameError: string;
  sortOrderMode: boolean;
  coverArtMode: boolean;
  includeRemovedInSorted: boolean;
}

export const usePublisherStore = defineStore('publisher', {
  state: (): PublisherStoreState => ({
    publisher: null,
    leagueYear: null,
    cleanGameSlots: null,
    editableGameSlots: null,
    desiredPositions: {},
    moveMode: false,
    heldSlot: null,
    moveGameError: '',
    sortOrderMode: false,
    coverArtMode: false,
    includeRemovedInSorted: false
  }),
  getters: {
    gameSlots: (state) => state.editableGameSlots,
    userIsPublisher(state) {
      const authStore = useAuthStore();
      return !!authStore.userInfo && state.publisher?.userID === authStore.userInfo.userID;
    },
    hasFormerGames: (state) => (state.publisher?.formerGames.length ?? 0) > 0
  },
  actions: {
    async initializePublisherPage(publisherID: string) {
      this.clearPublisherStoreData();
      const leagueYear = await leagueClient.getLeagueYearForPublisher(publisherID);
      const publisher = leagueYear?.publishers.find((x) => x.publisherID === publisherID);
      if (!publisher) {
        throw new Error(`Publisher ${publisherID} not found`);
      }

      this._initializeInternal(publisher, leagueYear);
      this._setDesiredPositions();
    },
    moveGame(moveIntoSlot: PublisherSlotViewModel) {
      this._moveGameInternal(moveIntoSlot);
      this._setDesiredPositions();
    },
    async confirmPositions() {
      if (!this.publisher) {
        throw Error('Publisher is not defined.');
      }

      const request: ReorderPublisherGamesRequest = {
        publisherID: this.publisher.publisherID,
        // TODO: NJsonSchema's TypeScript generator drops nullability on dictionary values, so slotStates is typed
        // { [key: string]: string } though the server takes Dictionary<int, Guid?>. Remove this cast by changing
        // SlotStates to a list of { SlotNumber, PublisherGameID? } records, a breaking API change.
        slotStates: this.desiredPositions as ReorderPublisherGamesRequest['slotStates']
      };

      try {
        await leagueClient.reorderPublisherGames(request);
      } catch (error) {
        this.cancelMoveMode();
        this.moveGameError = getApiErrorMessage(error);
      }

      this._completeMoveMode();

      const leagueYear = await leagueClient.getLeagueYearForPublisher(request.publisherID);
      const publisher = leagueYear?.publishers.find((x) => x.publisherID === request.publisherID);
      if (!publisher) {
        throw new Error(`Publisher ${request.publisherID} not found`);
      }

      this._initializeInternal(publisher, leagueYear);
      this._setDesiredPositions();
    },
    clearPublisherStoreData() {
      this.publisher = null;
      this.leagueYear = null;
      this.moveMode = false;
      this.desiredPositions = {};
      this.heldSlot = null;
      this.editableGameSlots = [];
    },
    enterMoveMode() {
      this.moveMode = true;
    },
    cancelMoveMode() {
      this.moveMode = false;
      this.desiredPositions = {};
      this.heldSlot = null;
      this.editableGameSlots = deepClone(this.cleanGameSlots);
    },
    holdGame(holdSlot: PublisherSlotViewModel) {
      this.heldSlot = holdSlot;
    },
    setSortOrderMode(sortOrderMode: boolean) {
      this.sortOrderMode = sortOrderMode;
    },
    setCoverArtMode(coverArtMode: boolean) {
      this.coverArtMode = coverArtMode;
    },
    setIncludeRemovedInSorted(includeRemovedInSorted: boolean) {
      this.includeRemovedInSorted = includeRemovedInSorted;
    },
    _initializeInternal(publisher: PublisherViewModel, leagueYear: LeagueYearViewModel) {
      this.publisher = publisher;
      this.leagueYear = leagueYear;
      this.cleanGameSlots = deepClone(publisher.gameSlots);
      this.editableGameSlots = deepClone(publisher.gameSlots);
    },
    _completeMoveMode() {
      this.moveMode = false;
      this.desiredPositions = {};
      this.heldSlot = null;
    },
    _moveGameInternal(moveIntoSlot: PublisherSlotViewModel) {
      if (!this.heldSlot) {
        throw new Error('No held game');
      }

      const tempSlot = deepClone(moveIntoSlot);
      moveIntoSlot.publisherGame = deepClone(this.heldSlot.publisherGame);
      this.heldSlot.publisherGame = deepClone(tempSlot.publisherGame);
      this.heldSlot = null;
    },
    _setDesiredPositions() {
      if (!this.editableGameSlots) {
        throw new Error('No games to set positions for.');
      }

      const standardSlotsOnly = this.editableGameSlots.filter((x) => !x.counterPick);
      this.desiredPositions = {};
      standardSlotsOnly.forEach((slot) => {
        if (slot.publisherGame) {
          this.desiredPositions[slot.slotNumber] = slot.publisherGame.publisherGameID;
        } else {
          this.desiredPositions[slot.slotNumber] = null;
        }
      });
    }
  }
});
