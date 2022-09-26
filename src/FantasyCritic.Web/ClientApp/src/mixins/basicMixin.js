import { mapGetters } from 'vuex';
import GlobalFunctions from '@/globalFunctions';

let basicMixin = {
  computed: {
    ...mapGetters(['isPlusUser', 'isAuth', 'userInfo', 'isAdmin', 'isBetaTester', 'isFactChecker', 'authIsBusy']),
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
    }
  }
};

export default basicMixin;
