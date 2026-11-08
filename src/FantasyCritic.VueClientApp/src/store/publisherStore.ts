import { defineStore } from 'pinia';
import { useAuthStore } from '@/store/authStore';
import { deepClone } from '@/utilities/deepClone';
import { leagueClient, LeagueYearViewModel, PublisherSlotViewModel, PublisherViewModel, ReorderPublisherGamesRequest } from '@/api/clients';

interface PublisherStoreState {
  publisher: PublisherViewModel | null;
  leagueYear: LeagueYearViewModel | null;
  cleanGameSlots: PublisherSlotViewModel[] | null;
  editableGameSlots: PublisherSlotViewModel[] | null;
  desiredPositions: { [key: string]: string };
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
      const publisher = leagueYear?.publishers.filter((x) => x.publisherID === publisherID)[0];

      this.initializeInternal(publisher, leagueYear);
      this.setDesiredPositions();
    },
    moveGame(moveIntoSlot: PublisherSlotViewModel) {
      this.moveGameInternal(moveIntoSlot);
      this.setDesiredPositions();
    },
    async confirmPositions() {
      if (!this.publisher) {
        throw Error('Publisher is not defined.');
      }

      const request: ReorderPublisherGamesRequest = {
        publisherID: this.publisher.publisherID,
        slotStates: this.desiredPositions
      };

      try {
        await leagueClient.reorderPublisherGames(request);
        this.completeMoveMode();

        const leagueYear = await leagueClient.getLeagueYearForPublisher(request.publisherID);
        const publisher = leagueYear?.publishers.filter((x) => x.publisherID === request.publisherID)[0];

        this.initializeInternal(publisher, leagueYear);
        this.setDesiredPositions();
      } catch (error) {
        this.moveGameError = error.response.data;
        this.cancelMoveMode();
      }
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
    initializeInternal(publisher: PublisherViewModel, leagueYear: LeagueYearViewModel) {
      this.publisher = publisher;
      this.leagueYear = leagueYear;
      this.cleanGameSlots = deepClone(publisher.gameSlots);
      this.editableGameSlots = deepClone(publisher.gameSlots);
    },
    completeMoveMode() {
      this.moveMode = false;
      this.desiredPositions = {};
      this.heldSlot = null;
    },
    moveGameInternal(moveIntoSlot: PublisherSlotViewModel) {
      if (!this.heldSlot) {
        throw new Error('No held game');
      }

      const tempSlot = deepClone(moveIntoSlot);
      moveIntoSlot.publisherGame = deepClone(this.heldSlot.publisherGame);
      this.heldSlot.publisherGame = deepClone(tempSlot.publisherGame);
      this.heldSlot = null;
    },
    setDesiredPositions() {
      if (!this.editableGameSlots) {
        throw new Error('No games to sed positions for.');
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
