import { defineStore } from 'pinia';
import { accountClient, ApiException, FantasyCriticUserViewModel } from '@/api/clients';

export interface AuthStoreState {
  userInfo: FantasyCriticUserViewModel | null;
  userInfoLoaded: boolean;
  loadPromise: Promise<void> | null;
}

async function fetchCurrentUser(): Promise<FantasyCriticUserViewModel | null> {
  try {
    return await accountClient.currentUser();
  } catch (error) {
    if (ApiException.isApiException(error) && error.status === 401) {
      return null;
    }
    throw error;
  }
}

export const useAuthStore = defineStore('auth', {
  state: (): AuthStoreState => ({ userInfo: null, userInfoLoaded: false, loadPromise: null }),
  getters: {
    isAuth: (state) => state.userInfo !== null,
    isAdmin: (state) => state.userInfo?.roles.includes('Admin') ?? false,
    isActionRunner: (state) => state.userInfo?.roles.includes('ActionRunner') ?? false,
    isFactChecker: (state) => state.userInfo?.roles.includes('FactChecker') ?? false,
    isJobManager: (state) => state.userInfo?.roles.includes('JobManager') ?? false,
    isBetaTester: (state) => state.userInfo?.roles.includes('BetaTester') ?? false,
    isPlusUser: (state) => state.userInfo?.roles.includes('PlusUser') ?? false,
    decimalsToShow: (state) => (state.userInfo?.showDecimalPoints ? 1 : 0)
  },
  actions: {
    ensureUserLoaded(): Promise<void> {
      if (!this.loadPromise) {
        this.loadPromise = fetchCurrentUser().then(
          (userInfo) => {
            this.userInfo = userInfo;
            this.userInfoLoaded = true;
          },
          (error) => {
            this.loadPromise = null;
            throw error;
          }
        );
      }
      return this.loadPromise;
    }
  }
});
