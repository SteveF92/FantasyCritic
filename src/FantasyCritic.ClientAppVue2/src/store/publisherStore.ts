import { defineStore } from 'pinia';
import axios from 'axios';

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
    gameSlots: (state) => state.editableGameSlots
  },
  actions: {
    async initializePublisherPage(publisherID) {
      this.clearPublisherStoreData();
      const leagueYearResponse = await axios.get('/api/League/GetLeagueYearForPublisher/' + publisherID);
      const leagueYear = leagueYearResponse.data;
      const publisher = leagueYear.publishers.filter((x) => x.publisherID === publisherID)[0];

      const initializeParams = { publisher, leagueYear };
      this.initializeInternal(initializeParams);
      this.setDesiredPositions();
    },
    moveGame(moveIntoSlot) {
      this.moveGameInternal(moveIntoSlot);
      this.setDesiredPositions();
    },
    async confirmPositions() {
      const existingPublisherObject = this.publisher;
      const request = {
        publisherID: existingPublisherObject.publisherID,
        slotStates: this.desiredPositions
      };

      try {
        await axios.post('/api/League/ReorderPublisherGames', request);
        this.completeMoveMode();

        const publisherResponse = await axios.get('/api/League/GetPublisher/' + existingPublisherObject.publisherID);
        const publisher = publisherResponse.data;
        const leagueYearResponse = await axios.get('/api/League/GetLeagueYear?leagueID=' + existingPublisherObject.leagueID + '&year=' + existingPublisherObject.year);
        const leagueYear = leagueYearResponse.data;

        const initializeParams = { publisher, leagueYear };
        this.initializeInternal(initializeParams);
        this.setDesiredPositions();
      } catch (error) {
        this.moveGameError = error.response.data;
        this.cancelMoveMode();
      }
    },
    initializeInternal(initializeParams) {
      this.publisher = initializeParams.publisher;
      this.leagueYear = initializeParams.leagueYear;
      this.cleanGameSlots = structuredClone(this.publisher.gameSlots);
      this.editableGameSlots = structuredClone(this.publisher.gameSlots);
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
      this.editableGameSlots = structuredClone(this.cleanGameSlots);
    },
    completeMoveMode() {
      this.moveMode = false;
      this.desiredPositions = null;
      this.heldSlot = null;
    },
    holdGame(holdSlot) {
      this.heldSlot = holdSlot;
    },
    moveGameInternal(moveIntoSlot) {
      const tempSlot = structuredClone(moveIntoSlot);
      moveIntoSlot.publisherGame = structuredClone(this.heldSlot.publisherGame);
      this.heldSlot.publisherGame = structuredClone(tempSlot.publisherGame);
      this.heldSlot = null;
    },
    setDesiredPositions() {
      const standardSlotsOnly = this.editableGameSlots.filter((x) => !x.counterPick);
      this.desiredPositions = {};
      standardSlotsOnly.forEach((slot) => {
        if (slot.publisherGame) {
          this.desiredPositions[slot.slotNumber] = slot.publisherGame.publisherGameID;
        } else {
          this.desiredPositions[slot.slotNumber] = null;
        }
      });
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
