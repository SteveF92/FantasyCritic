import { defineStore } from 'pinia';
import { accountClient, FantasyCriticUserViewModel } from '@/api/clients';

export interface AuthStoreState {
  userInfo: FantasyCriticUserViewModel | null;
  isBusy: boolean;
}

export const useAuthStore = defineStore('auth', {
  state: (): AuthStoreState => ({ userInfo: null, isBusy: false }),
  getters: {
    isAuth: (state) => !!state.userInfo,
    authIsBusy: (state) => state.isBusy,
    isAdmin: (state) => state.userInfo && state.userInfo.roles.includes('Admin'),
    isActionRunner: (state) => state.userInfo && state.userInfo.roles.includes('ActionRunner'),
    isFactChecker: (state) => state.userInfo && state.userInfo.roles.includes('FactChecker'),
    isJobManager: (state) => state.userInfo && state.userInfo.roles.includes('JobManager'),
    isBetaTester: (state) => state.userInfo && state.userInfo.roles.includes('BetaTester'),
    isPlusUser: (state) => state.userInfo && state.userInfo.roles.includes('PlusUser'),
    decimalsToShow: (state) => (state.userInfo?.showDecimalPoints ? 1 : 0)
  },
  actions: {
    async getUserInfo() {
      this.isBusy = true;
      try {
        const userInfo = await accountClient.currentUser();
        if (!userInfo.userID) {
          this.userInfo = null;
        } else {
          this.userInfo = userInfo;
        }
      } catch {
        this.userInfo = null;
      } finally {
        this.isBusy = false;
      }
    }
  }
});
