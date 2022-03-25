import axios from 'axios';

export default {
  state: {
    publisher: null,
    leagueYear: null,
    cleanGameSlots: null,
    editableGameSlots: null,
    desiredPositions: null,
    moveMode: false,
    heldSlot: null,
    moveGameError: ''
  },
  getters: {
    publisher: (state) => state.publisher,
    leagueYear: (state) => state.leagueYear,
    gameSlots: (state) => state.editableGameSlots,
    moveMode: (state) => state.moveMode,
    desiredPositions: (state) => state.desiredPositions,
    holdingGame: (state) => !!state.heldSlot,
    moveGameError: (state) => state.moveGameError
  },
  actions: {
    async initializePublisherPage(context, publisherID) {
      try {
        context.commit('clearPublisherStoreData');
        const publisherResponse = await axios.get('/api/League/GetPublisher/' + publisherID);
        const publisher = publisherResponse.data;
        const leagueYearResponse = await axios.get('/api/League/GetLeagueYear?leagueID=' + publisher.leagueID + '&year=' + publisher.year);
        const leagueYear = leagueYearResponse.data;

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
    confirmPositions(context) {
      return new Promise(function (resolve, reject) {
        let request = {
          publisherID: context.getters.publisher.publisherID,
          slotStates: context.getters.desiredPositions
        };
        axios
          .post('/api/League/ReorderPublisherGames', request)
          .then((response) => {
            if (response.status === 200) {
              context.commit('completeMoveMode');
              resolve();
            }
          })
          .catch((error) => {
            context.commit('setMoveError', error.response.data);
            context.commit('cancelMoveMode');
            reject();
          });
      });
    }
  },
  mutations: {
    initializeInternal(state, intializeParams) {
      state.publisher = intializeParams.publisher;
      state.leagueYear = intializeParams.leagueYear;
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
      state.heldSlot.gameMeetsSlotCriteria = true;
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
    }
  }
};
