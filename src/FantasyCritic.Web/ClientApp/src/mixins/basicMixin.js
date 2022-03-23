let basicMixin = {
  computed: {
    isPlusUser() {
      return this.$store.getters.isPlusUser;
    },
    isAuth() {
      return this.$store.getters.isAuthenticated;
    },
    userInfo() {
      return this.$store.getters.userInfo;
    },
    isAdmin() {
      return this.$store.getters.isAdmin;
    },
    isBetaTester() {
      return this.$store.getters.isBetaTester;
    },
    displayName() {
      return this.$store.getters.userInfo.displayName;
    },
    authIsBusy() {
      return this.$store.getters.authIsBusy;
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
