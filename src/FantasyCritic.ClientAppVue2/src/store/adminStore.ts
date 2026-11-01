import { defineStore } from 'pinia';
import { useAuthStore } from './authStore';
import { combinedDataClient } from '@/api/clients';

export const useAdminStore = defineStore('admin', {
  state: () => ({ adminTaskCounts: null }),
  actions: {
    async fetchAdminTaskCounts() {
      //The endpoint is only available to fact checkers and admins, so don't bother calling it for anyone else.
      const authStore = useAuthStore();
      if (!authStore.isFactChecker && !authStore.isAdmin) {
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
