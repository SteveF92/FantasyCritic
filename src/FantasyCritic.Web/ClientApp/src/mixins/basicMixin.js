import { mapGetters, mapState } from 'vuex';
import GlobalFunctions from '@/globalFunctions.js';
import * as Filters from '@/filters.js';

let basicMixin = {
  computed: {
    ...mapGetters(['isPlusUser', 'isAuth', 'userInfo', 'isAdmin', 'isBetaTester', 'isFactChecker', 'authIsBusy']),
    ...mapState({
      possibleLeagueOptions: (state) => state.interLeague.possibleLeagueOptions
    }),
    displayName() {
      if (!this.$store.getters.userInfo) {
        return;
      }
      return this.$store.getters.userInfo.displayName;
    }
  },
  methods: {
    makeToast(message) {
      this.$bvToast.toast(message, {
        autoHideDelay: 5000,
        variant: 'info',
        solid: true,
        noCloseButton: true
      });
    },
    formatLongDateTime(dateTime) {
      return GlobalFunctions.formatLongDateTime(dateTime);
    },
    formatLongDate(date) {
      return GlobalFunctions.formatLongDate(date);
    },
    ...Filters
  }
};

export default basicMixin;
