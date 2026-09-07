import { combinedDataClient } from '@/api/clients';

export default {
  state: {
    adminTaskCounts: null
  },
  getters: {
    adminTaskCounts: (state) => state.adminTaskCounts
  },
  actions: {
    async fetchAdminTaskCounts(context) {
      //The endpoint is only available to fact checkers and admins, so don't bother calling it for anyone else.
      if (!context.getters.isFactChecker && !context.getters.isAdmin) {
        return;
      }

      try {
        const adminTaskCounts = await combinedDataClient.adminTaskCounts();
        context.commit('setAdminTaskCounts', adminTaskCounts);
      } catch (error) {
        console.log(error);
      }
    }
  },
  mutations: {
    setAdminTaskCounts(state, adminTaskCounts) {
      state.adminTaskCounts = adminTaskCounts;
    }
  }
};
