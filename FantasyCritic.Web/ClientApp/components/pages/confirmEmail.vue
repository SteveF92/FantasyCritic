<template>
  <div>
    <h1>Confirm Your Email</h1>
    <div class="alert alert-danger" v-show="errorInfo">
      {{errorInfo}}
    </div>
    <div v-if="isBusy" class="alert alert-info">
      <div>Confirming Email...</div>
    </div>
    <div v-if="attemptFailed" class="alert alert-warning">
      <div>If you are having issues, check out our <a href="/faq#bidding-system" target="_blank" class="text-secondary">FAQ</a> page.</div>
    </div>
    <div v-if="accountConfirmed" class="alert alert-success">
      <span>Email Confirmation Successful!</span>
      <span>You will be redirected in a few seconds...</span>
    </div>
    <b-button variant="primary" v-on:click="confirmFromURL" :disabled="isBusy">Click here to confirm your email.</b-button>
  </div>
</template>
<script>
  import axios from 'axios';

  export default {
    data() {
      return {
        isBusy: false,
        attemptFailed: false,
        errorInfo: ""
      }
    },
    computed: {
      accountConfirmed() {
        if (this.$store.getters.userInfo) {
          return this.$store.getters.userInfo.emailConfirmed;
        }
        return false;
      },
      isAuth() {
        return this.$store.getters.tokenIsCurrent();
      }
    },
    methods: {
      confirmFromURL() {
        this.attemptFailed = false;
        this.errorInfo = "";
        let request = {
          userID: this.$route.query.UserID,
          code: this.$route.query.Code
        };
        this.confirmEmail(request);
      },
      confirmEmail(request) {
        this.isBusy = true;
        axios
          .post('/api/account/ConfirmEmail', request)
          .then(response => {
            this.isBusy = false;
            if (this.isAuth) {
              this.$store.dispatch("getUserInfo")
                .then(() => {
                  setTimeout(() => this.$router.push({ name: "home" }), 3000);
                })
                .catch(returnedError => {
                });
            } else {
              setTimeout(() => this.$router.push({ name: "login" }), 3000);
            }
          })
          .catch(response => {
            this.isBusy = false;
            this.errorInfo = "Something went wrong with automatic confirmation. Try confirming manually.";
            this.attemptFailed = true;
          });
      },
    }
  }
</script>
