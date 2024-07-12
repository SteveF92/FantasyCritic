import axios from 'axios';
import _ from 'lodash';

export default {
  state: {
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
  },
  getters: {
    publisher: (state) => state.publisher,
    leagueYear: (state) => state.leagueYear,
    gameSlots: (state) => state.editableGameSlots,
    moveMode: (state) => state.moveMode,
    desiredPositions: (state) => state.desiredPositions,
    holdingGame: (state) => !!state.heldSlot,
    moveGameError: (state) => state.moveGameError,
    sortOrderMode: (state) => state.sortOrderMode,
    coverArtMode: (state) => state.coverArtMode,
    includeRemovedInSorted: (state) => state.includeRemovedInSorted
  },
  actions: {
    async initializePublisherPage(context, publisherID) {
      try {
        context.commit('clearPublisherStoreData');
        const leagueYearResponse = await axios.get('/api/League/GetLeagueYearForPublisher/' + publisherID);
        const leagueYear = leagueYearResponse.data;
        const publisher = leagueYear.publishers.filter((x) => x.publisherID === publisherID)[0];

        const initializeParams = { publisher, leagueYear };
        context.commit('initializeInternal', initializeParams);
        context.commit('setDesiredPositions');
      } catch (error) {
        this.errorInfo = error;
      }
    },
    moveGame(context, moveIntoSlot) {
      context.commit('moveGameInternal', moveIntoSlot);
      context.commit('setDesiredPositions');
    },
    async confirmPositions(context) {
      const existingPublisherObject = context.getters.publisher;
      let request = {
        publisherID: existingPublisherObject.publisherID,
        slotStates: context.getters.desiredPositions
      };

      try {
        await axios.post('/api/League/ReorderPublisherGames', request);
        context.commit('completeMoveMode');

        const publisherResponse = await axios.get('/api/League/GetPublisher/' + existingPublisherObject.publisherID);
        const publisher = publisherResponse.data;
        const leagueYearResponse = await axios.get('/api/League/GetLeagueYear?leagueID=' + existingPublisherObject.leagueID + '&year=' + existingPublisherObject.year);
        const leagueYear = leagueYearResponse.data;

        const initializeParams = { publisher, leagueYear };
        context.commit('initializeInternal', initializeParams);
        context.commit('setDesiredPositions');
      } catch (error) {
        context.commit('setMoveError', error.response.data);
        context.commit('cancelMoveMode');
      }
    }
  },
  mutations: {
    initializeInternal(state, initializeParams) {
      state.publisher = initializeParams.publisher;
      state.leagueYear = initializeParams.leagueYear;
      state.cleanGameSlots = _.cloneDeep(state.publisher.gameSlots);
      state.editableGameSlots = _.cloneDeep(state.publisher.gameSlots);
    },
    clearPublisherStoreData(state) {
      state.publisher = null;
      state.leagueYear = null;
      state.moveMode = false;
      state.desiredPositions = null;
      state.heldSlot = null;
      state.editableGameSlots = [];
    },
    enterMoveMode(state) {
      state.moveMode = true;
    },
    cancelMoveMode(state) {
      state.moveMode = false;
      state.desiredPositions = null;
      state.heldSlot = null;
      state.editableGameSlots = _.cloneDeep(state.cleanGameSlots);
    },
    completeMoveMode(state) {
      state.moveMode = false;
      state.desiredPositions = null;
      state.heldSlot = null;
    },
    holdGame(state, holdSlot) {
      state.heldSlot = holdSlot;
    },
    moveGameInternal(state, moveIntoSlot) {
      let tempSlot = _.cloneDeep(moveIntoSlot);
      moveIntoSlot.publisherGame = _.cloneDeep(state.heldSlot.publisherGame);
      state.heldSlot.publisherGame = _.cloneDeep(tempSlot.publisherGame);
      state.heldSlot = null;
    },
    setDesiredPositions(state) {
      let standardSlotsOnly = state.editableGameSlots.filter((x) => !x.counterPick);
      state.desiredPositions = {};
      standardSlotsOnly.forEach((slot) => {
        if (slot.publisherGame) {
          state.desiredPositions[slot.slotNumber] = slot.publisherGame.publisherGameID;
        } else {
          state.desiredPositions[slot.slotNumber] = null;
        }
      });
    },
    setMoveError(state, error) {
      state.moveGameError = error;
    },
    setSortOrderMode(state, sortOrderMode) {
      state.sortOrderMode = sortOrderMode;
    },
    setCoverArtMode(state, coverArtMode) {
      state.coverArtMode = coverArtMode;
    },
    setIncludeRemovedInSorted(state, includeRemovedInSorted) {
      state.includeRemovedInSorted = includeRemovedInSorted;
    }
  }
};
