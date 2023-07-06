export default {
  namespaced: true,
  state: {
    modals: {}
  },
  getters: {
    modals: (state) => state.modals
  },
  actions: {
    toggleModal(context, modalName) {
      if (!context.state.modals[modalName]) {
        context.commit('setModalVisibility', { modalName: modalName, visible: true });
      } else {
        context.commit('setModalVisibility', { modalName: modalName, visible: false });
      }
    }
  },
  mutations: {
    setModalVisibility(state, param) {
      state.modals[param.modalName] = param.visible;
    }
  }
};
