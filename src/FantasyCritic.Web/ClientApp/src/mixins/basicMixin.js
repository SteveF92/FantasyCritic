let basicMixin = {
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
