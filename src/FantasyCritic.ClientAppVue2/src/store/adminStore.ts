import { defineStore } from 'pinia';
import { combinedDataClient } from '@/api/clients';

export const useAdminStore = defineStore('admin', {
  state: () => ({ adminTaskCounts: null }),
  getters: {
    adminTaskCounts: (state) => state.adminTaskCounts
  },
  actions: {
    async fetchAdminTaskCounts() {
      //The endpoint is only available to fact checkers and admins, so don't bother calling it for anyone else.
      if (!this.isFactChecker && !this.isAdmin) {
        return;
      }

      try {
        const adminTaskCounts = await combinedDataClient.adminTaskCounts();
        this.adminTaskCounts = adminTaskCounts;
      } catch (error) {
        console.log(error);
      }
    }
  }
});
