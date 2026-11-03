import { toRaw } from 'vue';

import { defineStore } from 'pinia';
import axios from 'axios';
import { useAuthStore } from '@/store/authStore';

export const usePublisherStore = defineStore('publisher', {
  state: () => ({
    publisher: null,
    leagueYear: null,
    cleanGameSlots: null,
    editableGameSlots: null,
    desiredPositions: null,
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
    async initializePublisherPage(publisherID) {
      this.clearPublisherStoreData();
      const leagueYearResponse = await axios.get('/api/League/GetLeagueYearForPublisher/' + publisherID);
      const leagueYear = leagueYearResponse.data;
      const publisher = leagueYear.publishers.filter((x) => x.publisherID === publisherID)[0];

      const initializeParams = { publisher, leagueYear };
      initializeInternal(this, initializeParams);
      setDesiredPositions(this);
    },
    moveGame(moveIntoSlot) {
      moveGameInternal(this, moveIntoSlot);
      setDesiredPositions(this);
    },
    async confirmPositions() {
      const existingPublisherObject = this.publisher;
      const request = {
        publisherID: existingPublisherObject.publisherID,
        slotStates: this.desiredPositions
      };

      try {
        await axios.post('/api/League/ReorderPublisherGames', request);
        completeMoveMode(this);

        const publisherResponse = await axios.get('/api/League/GetPublisher/' + existingPublisherObject.publisherID);
        const publisher = publisherResponse.data;
        const leagueYearResponse = await axios.get('/api/League/GetLeagueYear?leagueID=' + existingPublisherObject.leagueID + '&year=' + existingPublisherObject.year);
        const leagueYear = leagueYearResponse.data;

        const initializeParams = { publisher, leagueYear };
        initializeInternal(this, initializeParams);
        setDesiredPositions(this);
      } catch (error) {
        this.moveGameError = error.response.data;
        this.cancelMoveMode();
      }
    },
    clearPublisherStoreData() {
      this.publisher = null;
      this.leagueYear = null;
      this.moveMode = false;
      this.desiredPositions = null;
      this.heldSlot = null;
      this.editableGameSlots = [];
    },
    enterMoveMode() {
      this.moveMode = true;
    },
    cancelMoveMode() {
      this.moveMode = false;
      this.desiredPositions = null;
      this.heldSlot = null;
      this.editableGameSlots = structuredClone(toRaw(this.cleanGameSlots));
    },
    holdGame(holdSlot) {
      this.heldSlot = holdSlot;
    },
    setSortOrderMode(sortOrderMode) {
      this.sortOrderMode = sortOrderMode;
    },
    setCoverArtMode(coverArtMode) {
      this.coverArtMode = coverArtMode;
    },
    setIncludeRemovedInSorted(includeRemovedInSorted) {
      this.includeRemovedInSorted = includeRemovedInSorted;
    }
  }
});

function initializeInternal(store, initializeParams) {
  store.publisher = initializeParams.publisher;
  store.leagueYear = initializeParams.leagueYear;
  store.cleanGameSlots = structuredClone(toRaw(store.publisher.gameSlots));
  store.editableGameSlots = structuredClone(toRaw(store.publisher.gameSlots));
}

function completeMoveMode(store) {
  store.moveMode = false;
  store.desiredPositions = null;
  store.heldSlot = null;
}

function moveGameInternal(store, moveIntoSlot) {
  const tempSlot = structuredClone(toRaw(moveIntoSlot));
  moveIntoSlot.publisherGame = structuredClone(toRaw(store.heldSlot.publisherGame));
  store.heldSlot.publisherGame = structuredClone(tempSlot.publisherGame);
  store.heldSlot = null;
}

function setDesiredPositions(store) {
  const standardSlotsOnly = store.editableGameSlots.filter((x) => !x.counterPick);
  store.desiredPositions = {};
  standardSlotsOnly.forEach((slot) => {
    if (slot.publisherGame) {
      store.desiredPositions[slot.slotNumber] = slot.publisherGame.publisherGameID;
    } else {
      store.desiredPositions[slot.slotNumber] = null;
    }
  });
}
