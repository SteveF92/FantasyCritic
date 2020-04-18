<template>
  <div>
    <div class="col-md-10 offset-md-1 col-sm-12">
      <h1>Confirm Your Email</h1>
      <div v-if="accountConfirmed" class="alert alert-success">
        <span>Email Confirmation Successful!</span>
        <span>You will be redirected in a few seconds...</span>
      </div>
      <div v-else>
        <div class="alert alert-danger" v-show="errorInfo">
          {{errorInfo}}
        </div>
        <div v-if="isBusy" class="alert alert-info">
          <div>Confirming Email...</div>
        </div>
        <div v-if="attemptFailed" class="alert alert-warning">
          <div>If you are having issues, check out our <a href="/faq#bidding-system" target="_blank" class="text-secondary">FAQ</a> page.</div>
        </div>

        <b-button variant="primary" v-on:click="confirmFromURL" :disabled="isBusy">Click to confirm your email automatically</b-button>

        <hr />

        <h2>Manual Confirmation</h2>
        <h5>If you are having issues with confirming your email, try using this form.</h5>
        <h6>Check the email you received for how to fill out this form.</h6>
        <ValidationObserver v-slot="{ invalid }">
          <form method="post" class="form-horizontal col-md-6" role="form" v-on:submit.prevent="manualConfirm">
            <div class="form-group">
              <label for="manaulUserID" class="control-label">User ID</label>
              <ValidationProvider rules="required" v-slot="{ errors }" name="User ID">
                <input v-model="manaulUserID" id="manaulUserID" name="manaulUserID" type="text" class="form-control input" />
                <span class="text-danger">{{ errors[0] }}</span>
              </ValidationProvider>
            </div>

            <div class="form-group">
              <label for="manualConfirmCode" class="control-label">Confirmation Code</label>
              <ValidationProvider rules="required" v-slot="{ errors }" name="Confirmation Code">
                <input v-model="manualConfirmCode" id="manualConfirmCode" name="manualConfirmCode" type="text" class="form-control input" />
                <span class="text-danger">{{ errors[0] }}</span>
              </ValidationProvider>
            </div>

            <div class="register-button-area">
              <button type="submit" class="btn btn-primary register-button" :disabled="invalid || isBusy">
                Confirm Email Manually
              </button>
            </div>
          </form>
        </ValidationObserver>
      </div>
    </div>
  </div>
</template>
<script>
  import axios from 'axios';

  export default {
    data() {
      return {
        isBusy: false,
        attemptFailed: false,
        errorInfo: "",
        manaulUserID: "",
        manualConfirmCode: ""
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
      manualConfirm() {
        this.attemptFailed = false;
        this.errorInfo = "";
        let request = {
          userID: this.manaulUserID,
          code: this.manualConfirmCode
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
