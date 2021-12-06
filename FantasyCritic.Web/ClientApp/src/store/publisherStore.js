export default {
  state: {
    cleanGameSlots: null,
    editableGameSlots: null,
    desiredPositions: null,
    moveMode: false,
    heldSlot: null
  },
  getters: {
    gameSlots: (state) => state.editableGameSlots,
    moveMode: (state) => state.moveMode,
    desiredPositions: (state) => state.desiredPositions,
    holdingGame: (state) => !!state.heldSlot
  },
  mutations: {
    initializeGameSlots(state, initialGameSlots) {
      state.cleanGameSlots = _.cloneDeep(initialGameSlots);
      state.editableGameSlots = _.cloneDeep(initialGameSlots);

      let standardSlotsOnly = state.editableGameSlots.filter(x => !x.counterPick);
      state.desiredPositions = {};
      standardSlotsOnly.forEach(slot => {
        if (slot.publisherGame) {
          state.desiredPositions[slot.slotNumber] = slot.publisherGame.gameName;
        } else {
          state.desiredPositions[slot.slotNumber] = null;
        }
      });
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
    holdGame(state, holdSlot) {
      state.heldSlot = holdSlot;
    },
    setDesiredPosition(state, moveIntoSlot) {
      let tempSlot = _.cloneDeep(moveIntoSlot);
      moveIntoSlot.publisherGame = _.cloneDeep(state.heldSlot.publisherGame);
      state.heldSlot.publisherGame = _.cloneDeep(tempSlot.publisherGame);

      state.heldSlot = null;

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
