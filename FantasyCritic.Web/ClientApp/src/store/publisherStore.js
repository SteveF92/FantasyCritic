export default {
  state: {
    desiredPositions: null,
    heldGame: null
  },
  getters: {
    desiredPositions: (state) => state.desiredPositions,
    heldGame: (state) => state.heldGame
  },
  mutations: {
    initializeDesiredPositions(state, publisherGames) {
      let standardGames = publisherGames.filter(x => !x.counterPick);
      state.desiredPositions = Object.fromEntries(standardGames.map(x => [x.publisherGameID, x.slotNumber]));
    },
    clearDesiredPositions(state) {
      state.desiredPositions = null;
      state.heldGame = null;
    },
    holdGame(state, publisherGameID) {
      state.heldGame = publisherGameID;
    },
    setDesiredPosition(state, slotNumber) {
      state.desiredPositions[state.heldGame] = slotNumber;
      state.heldGame = null;
    }
  }
};
