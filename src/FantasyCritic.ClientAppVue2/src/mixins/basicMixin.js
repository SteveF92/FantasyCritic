import { mapState } from 'pinia';
import { useAuthStore } from '@/store/authStore';
import { useInterLeagueStore } from '@/store/interLeagueStore';

import { formatLongDateTime, formatLongDate } from '@/globalFunctions';

const basicMixin = {
  computed: {
    ...mapState(useAuthStore, ['isPlusUser', 'isAuth', 'userInfo', 'isAdmin', 'isBetaTester', 'isFactChecker', 'isActionRunner', 'isJobManager', 'authIsBusy']),
    ...mapState(useInterLeagueStore, ['interLeagueDataLoaded', 'possibleLeagueOptions', 'supportedYears', 'activeRoyaleQuarter']),
    displayName() {
      const authStore = useAuthStore();
      if (!authStore.userInfo) {
        return;
      }
      return authStore.userInfo.displayName;
    },
    isDevelopment() {
      return process.env.NODE_ENV === 'development';
    }
  },
  methods: {
    formatLongDateTime(dateTime) {
      return formatLongDateTime(dateTime);
    },
    formatLongDate(date) {
      return formatLongDate(date);
    }
  }
};

export default basicMixin;
