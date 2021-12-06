import axios from "axios";

export default {
  state: {
    publisherID: null,
    cleanGameSlots: null,
    editableGameSlots: null,
    desiredPositions: null,
    moveMode: false,
    heldSlot: null
  },
  getters: {
    publisherID: (state) => state.publisherID,
    gameSlots: (state) => state.editableGameSlots,
    moveMode: (state) => state.moveMode,
    desiredPositions: (state) => state.desiredPositions,
    holdingGame: (state) => !!state.heldSlot
  },
  actions: {
    initialize(context, publisher) {
      context.commit('setPublisherID', publisher.publisherID);
      context.commit('initializeInternal', publisher.gameSlots);
      context.commit('setDesiredPositions');
    },
    moveGame(context, moveIntoSlot) {
      context.commit('moveGameInternal', moveIntoSlot);
      context.commit('setDesiredPositions');
    },
    confirmPositions(context) {
      return new Promise(function (resolve, reject) {
        let request = {
          publisherID: context.getters.publisherID,
          slotStates: context.getters.desiredPositions
        };
        axios.post("/api/League/ReorderPublisherGames", request)
          .then((res) => {
            if (res.status === 200) {
              context.commit("completeMoveMode");
              resolve();
            }
          })
          .catch(error => {
            reject();
          });
      });
    }
  },
  mutations: {
    setPublisherID(state, publisherID) {
      state.publisherID = publisherID;
    },
    initializeInternal(state, initialGameSlots) {
      state.cleanGameSlots = _.cloneDeep(initialGameSlots);
      state.editableGameSlots = _.cloneDeep(initialGameSlots);
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
      let standardSlotsOnly = state.editableGameSlots.filter(x => !x.counterPick);
      state.desiredPositions = {};
      standardSlotsOnly.forEach(slot => {
        if (slot.publisherGame) {
          state.desiredPositions[slot.slotNumber] = slot.publisherGame.publisherGameID;
        } else {
          state.desiredPositions[slot.slotNumber] = null;
        }
      });
    }
  }
};
