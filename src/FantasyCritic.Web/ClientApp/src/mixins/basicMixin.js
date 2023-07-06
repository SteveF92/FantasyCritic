import { mapGetters, mapState } from 'vuex';
import GlobalFunctions from '@/globalFunctions.js';
import * as Filters from '@/filters.js';

let basicMixin = {
  computed: {
    ...mapGetters('auth', ['isPlusUser', 'isAuth', 'userInfo', 'isAdmin', 'isBetaTester', 'isFactChecker', 'authIsBusy']),
    ...mapGetters('modals', ['modals']),
    ...mapState({
      possibleLeagueOptions: (state) => state.interLeague.possibleLeagueOptions
    }),
    ...mapState({
      nextUniqueID: (state) => state.interLeague.uniqueID
    }),
    displayName() {
      if (!this.$store.getters['auth/userInfo']) {
        return;
      }
      return this.$store.getters['auth/userInfo'].displayName;
    }
  },
  methods: {
    getUniqueID() {
      this.$store.commit('interLeague/incrementUniqueID');
      return this.nextUniqueID;
    },
    toggleModal(modalName) {
      this.$store.dispatch('modal/toggleModal', modalName);
    },
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
