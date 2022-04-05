import { mapGetters } from 'vuex';

let basicMixin = {
  computed: {
    ...mapGetters(['isPlusUser', 'isAuth', 'userInfo', 'isAdmin', 'isBetaTester', 'authIsBusy']),
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
    }
  }
};

export default basicMixin;
