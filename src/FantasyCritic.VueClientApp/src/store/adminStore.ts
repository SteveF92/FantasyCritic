import { defineStore } from 'pinia';
import { useAuthStore } from './authStore';
import { AdminTaskCountsViewModel, combinedDataClient } from '@/api/clients';

export interface AdminStoreState {
  adminTaskCounts: AdminTaskCountsViewModel;
}

export const useAdminStore = defineStore('admin', {
  state: (): AdminStoreState => ({
    adminTaskCounts: {
      totalCount: 0
    }
  }),
  actions: {
    async fetchAdminTaskCounts() {
      //The endpoint is only available to fact checkers and admins, so don't bother calling it for anyone else.
      const authStore = useAuthStore();
      if (!authStore.isFactChecker && !authStore.isAdmin) {
        return;
      }

      try {
        this.adminTaskCounts = await combinedDataClient.adminTaskCounts();
      } catch (error) {
        console.log(error);
      }
    }
  }
});
