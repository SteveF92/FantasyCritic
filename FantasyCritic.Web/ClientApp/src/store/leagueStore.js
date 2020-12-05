export default {
  state: {
    advancedProjectionsInternal: false
  },
  getters: {
    advancedProjections: (state) => state.advancedProjectionsInternal
  },
  mutations: {
    setAdvancedProjections(state, advancedProjections) {
      state.advancedProjectionsInternal = advancedProjections;
    }
  }
};
