export default {
  state: {
    advancedProjectionsInternal: false,
    draftOrderViewInternal: false
  },
  getters: {
    advancedProjections: (state) => state.advancedProjectionsInternal,
    draftOrderView: (state) => state.draftOrderViewInternal
  },
  mutations: {
    setAdvancedProjections(state, advancedProjections) {
      state.advancedProjectionsInternal = advancedProjections;
    },
    setDraftOrderView(state, draftOrderView) {
      state.draftOrderViewInternal = draftOrderView;
    }
  }
};
